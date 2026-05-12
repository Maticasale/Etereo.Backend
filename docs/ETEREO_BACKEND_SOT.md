# Etereo Backend — Source of Truth

> Documento técnico maestro del backend. Refleja el diseño acordado del sistema de gestión de la estética Etereo.
> En caso de ambigüedad entre este documento y el código, el código prevalece y este documento se actualiza.

**Última actualización:** Mayo 2026 — v2: sexo de cliente, variantes de subservicio, sesiones con descuento automático, packs diferenciados, precios reales cargados.

---

## 0. TL;DR

- **Stack:** ASP.NET Core 10, Entity Framework Core 10, PostgreSQL, JWT (HS256), BCrypt (workFactor 12), Railway (hosting), Docker multi-stage.
- **Auth:** JWT access token 15min + refresh token 30d con rotación obligatoria. Google OAuth con `Google.Apis.Auth`.
- **Email:** Resend (3.000 emails/mes gratis permanente). Background job cada 15min para recordatorios y post-turno.
- **8 módulos:** Auth+Usuarios, Servicios+Subservicios, Operarios, Turnos, Cupones, Imputaciones, Emails+Notificaciones, Estadísticas+Dashboard.
- **22 tablas** en PostgreSQL.
- **Sexo del cliente** filtrado automático de subservicios por sexo del usuario registrado.
- **Variantes de subservicio** para alisados, trenzas, drenaje linfático (sub-sub-servicio).
- **Sesiones** agrupan múltiples zonas de láser/descartable con descuento automático configurable.
- **Packs diferenciados** con `es_pack=true` y `detalle_pack` para renderizado especial en frontend.
- **3 roles:** Admin, Operario, Cliente.
- Migraciones EF se aplican automáticamente al boot. Seeder idempotente corre en cada arranque.

---

## 1. Arquitectura y convenciones

### 1.1 Estructura de la solución (N-Tier, 5 proyectos)

```
Etereo.Backend/
├── src/Etereo.Api/             — Controllers, middleware, Program.cs, atributos de autorización
├── src/Etereo.Application/     — Services (interfaces + implementación), Result<T>, IXxxDbContext
├── src/Etereo.Domain/          — Entities, enums, EntityBase (sin dependencias de EF Core)
├── src/Etereo.Infrastructure/  — DbContext, EF configurations, seeder, infra de auth, email
├── src/Etereo.Shared/          — DTOs compartidos, constantes (Roles)
├── docs/ETEREO_BACKEND_SOT.md  — Este documento
└── Dockerfile                  — Multi-stage build
```

**Dependencias entre proyectos:**
```
Api ──► Application ──► Domain
                  │
                  └──► Shared ◄── (todos referencian Shared)
Api ──► Infrastructure ──► Application + Domain
```

### 1.2 Reglas innegociables del código

| Regla | Detalle |
|---|---|
| **IDs `int` autoincrement** | Sin GUIDs. Sin claves compuestas naturales. |
| **Snake_case en PostgreSQL, PascalCase en C#** | Mapping explícito en cada `XxxConfiguration` vía `HasColumnName`. |
| **Enums se persisten como `string`** | `HasConversion<string>().HasMaxLength(N)` en cada config. |
| **Sin navigation properties** | FKs son `int` o `int?` simples. Joins LINQ explícitos en services. Sin `HasOne/HasMany`. |
| **No se borran registros** | Se usa `activo` (bool) o `estado` (enum). |
| **`Result<T>` para errores de negocio** | `Result<T>.Success(value)` o `Result<T>.Failure(errorCode, message)`. No se lanzan excepciones por reglas de negocio. |
| **Formato de respuesta HTTP** | Éxito: `{ data: ... }`. Error: `{ error: { codigo, mensaje } }`. |
| **`EntityBase` minimalista** | Solo `public int Id { get; set; }`. Timestamps declarados explícitamente por entidad. |
| **Fechas SIEMPRE en UTC en backend** | `DateTime.UtcNow`. Frontend convierte a local. |
| **Strings nullables explícitos** | `string?` para opcionales, `string Prop { get; set; } = string.Empty` para no-nullables. |
| **Tablas en plural snake_case** | `usuarios`, `refresh_tokens`, `operario_subservicios`, etc. |

### 1.3 Patrón `IXxxDbContext`

- Solo expone `IQueryable<T>` (nunca `DbSet<T>`).
- Métodos `AddXxx` / `RemoveXxx` explícitos por entidad.
- `Task<int> SaveChangesAsync(CancellationToken ct)` disponible.
- `AppDbContext` implementa todas las interfaces usando explicit interface implementation.

### 1.4 Autorización por rol

Sin sistema RBAC complejo. Autorización directa contra el claim `rol` del JWT:

```csharp
// Atributo personalizado
[RequiereRol(Roles.Admin)]
[RequiereRol(Roles.Admin, Roles.Operario)]  // cualquiera de los dos
```

Claims en el JWT:
```
sub    → id del usuario (string)
rol    → "Admin" | "Operario" | "Cliente"
email  → email del usuario
```

### 1.5 Rate limiting

`AspNetCoreRateLimit` configurado en `Program.cs`:
```
POST /api/v1/turnos  →  máx 5 requests / 10 minutos / IP
POST /api/v1/auth/register → máx 3 requests / hora / IP
POST /api/v1/auth/forgot-password → máx 3 requests / hora / IP
```

---

## 2. Base de datos — 22 tablas

### 2.1 Módulo Auth + Usuarios (6 tablas)

#### `usuarios`
```
id                    int PK autoincrement
email                 varchar(255) UNIQUE NOT NULL
password_hash         varchar(255) NULL              — NULL si Google OAuth
nombre                varchar(100) NOT NULL
apellido              varchar(100) NOT NULL
telefono              varchar(30)  NULL
sexo                  varchar(15)  NULL DEFAULT 'NoEspecifica'  — enum: Masculino | Femenino | NoEspecifica
rol                   varchar(20)  NOT NULL DEFAULT 'Cliente'
auth_provider         varchar(20)  NOT NULL DEFAULT 'Local'
google_id             varchar(100) NULL UNIQUE
estado                varchar(30)  NOT NULL DEFAULT 'Activo'
motivo_bloqueo        varchar(500) NULL
debe_cambiar_password bool NOT NULL DEFAULT false
avatar_url            varchar(500) NULL
creado_en             timestamptz NOT NULL DEFAULT now()
actualizado_en        timestamptz NOT NULL DEFAULT now()
```

#### `refresh_tokens`
```
id           int PK autoincrement
usuario_id   int NOT NULL → usuarios
token_hash   varchar(255) UNIQUE NOT NULL
expira_en    timestamptz NOT NULL
revocado     bool NOT NULL DEFAULT false
creado_en    timestamptz NOT NULL DEFAULT now()
```

#### `password_reset_tokens`
```
id           int PK autoincrement
usuario_id   int NOT NULL → usuarios
token_hash   varchar(255) UNIQUE NOT NULL
expira_en    timestamptz NOT NULL            — TTL: 1 hora
usado        bool NOT NULL DEFAULT false
creado_en    timestamptz NOT NULL DEFAULT now()
```

#### `disponibilidad_salon`
```
id              int PK autoincrement
fecha           date NOT NULL UNIQUE
salon           varchar(10) NOT NULL         — enum: Salon1 | Salon2 | Ambos
motivo_id       int NOT NULL → motivos_bloqueo_salon
descripcion     varchar(300) NULL
creado_por_id   int NOT NULL → usuarios
creado_en       timestamptz NOT NULL DEFAULT now()
```

#### `disponibilidad_operario`
```
id              int PK autoincrement
operario_id     int NOT NULL → usuarios
fecha           date NOT NULL
trabaja         bool NOT NULL DEFAULT true
motivo_ausencia varchar(200) NULL
creado_en       timestamptz NOT NULL DEFAULT now()

UNIQUE(operario_id, fecha)
```

#### `operario_vistas`
```
id                      int PK autoincrement
operario_id             int NOT NULL UNIQUE → usuarios
ver_mis_turnos          bool NOT NULL DEFAULT true
ver_mis_comisiones      bool NOT NULL DEFAULT true
ver_mi_calificacion     bool NOT NULL DEFAULT false
ver_mis_estadisticas    bool NOT NULL DEFAULT false
creado_en               timestamptz NOT NULL DEFAULT now()
actualizado_en          timestamptz NOT NULL DEFAULT now()
```

### 2.2 Módulo Servicios & Subservicios (2 tablas)

#### `servicios`
```
id                        int PK autoincrement
nombre                    varchar(100) NOT NULL UNIQUE
salon                     varchar(10)  NOT NULL        — enum: Salon1 | Salon2 | Ambos
categoria_imputacion_id   int NULL → categorias_imputacion
activo                    bool NOT NULL DEFAULT true
orden_display             int  NOT NULL DEFAULT 0
creado_en                 timestamptz NOT NULL DEFAULT now()
```

#### `subservicios`
```
id                  int PK autoincrement
servicio_id         int NOT NULL → servicios
nombre              varchar(150) NOT NULL
descripcion         varchar(500) NULL
precio              numeric(10,2) NULL               — nullable si tiene variantes
duracion_min        int NULL                         — nullable si tiene variantes
requiere_silencio   bool NOT NULL DEFAULT false
es_pack             bool NOT NULL DEFAULT false      — renderizado especial en frontend
detalle_pack        varchar(500) NULL                — zonas incluidas, solo si es_pack=true
sexo                varchar(15)  NOT NULL DEFAULT 'Ambos'  — enum: Masculino | Femenino | Ambos
activo              bool NOT NULL DEFAULT true
orden_display       int  NOT NULL DEFAULT 0
creado_en           timestamptz NOT NULL DEFAULT now()

UNIQUE(servicio_id, nombre)
```

### 2.3 Módulo Variantes de Subservicio (1 tabla)

#### `variantes_subservicio`
```
id              int PK autoincrement
subservicio_id  int NOT NULL → subservicios
nombre          varchar(150) NOT NULL
precio          numeric(10,2) NOT NULL
duracion_min    int NOT NULL
sexo            varchar(15) NOT NULL DEFAULT 'Ambos'
activo          bool NOT NULL DEFAULT true
orden_display   int  NOT NULL DEFAULT 0
creado_en       timestamptz NOT NULL DEFAULT now()

UNIQUE(subservicio_id, nombre)
```

### 2.4 Módulo Operarios (1 tabla)

#### `operario_subservicios`
```
id                  int PK autoincrement
operario_id         int NOT NULL → usuarios
subservicio_id      int NOT NULL → subservicios
porcentaje_comision numeric(4,2) NOT NULL    — 0.55 = 55%

UNIQUE(operario_id, subservicio_id)
```

### 2.5 Módulo Turnos (3 tablas)

#### `sesiones`
```
id                  int PK autoincrement
cliente_id          int NULL → usuarios
nombre_anonimo      varchar(200) NULL
telefono_anonimo    varchar(30)  NULL
operario_id         int NOT NULL → usuarios
salon               varchar(10)  NOT NULL
fecha_hora_inicio   timestamptz NOT NULL
estado              varchar(30)  NOT NULL DEFAULT 'PendienteConfirmacion'
descuento_auto_pct  numeric(5,2) NULL
creado_en           timestamptz NOT NULL DEFAULT now()
```

#### `reglas_descuento_sesion`
```
id                   int PK autoincrement
servicio_id          int NOT NULL UNIQUE → servicios
zonas_minimas        int NOT NULL DEFAULT 3
porcentaje_descuento numeric(5,2) NOT NULL
activo               bool NOT NULL DEFAULT true
creado_en            timestamptz NOT NULL DEFAULT now()
actualizado_en       timestamptz NOT NULL DEFAULT now()
```

#### `turnos`
```
id                   int PK autoincrement
salon                varchar(10)  NOT NULL
cliente_id           int NULL → usuarios
nombre_anonimo       varchar(200) NULL
telefono_anonimo     varchar(30)  NULL
operario_id          int NOT NULL → usuarios
subservicio_id       int NOT NULL → subservicios
variante_id          int NULL → variantes_subservicio
sesion_id            int NULL → sesiones
fecha_hora_inicio    timestamptz NOT NULL
duracion_min         int NOT NULL
estado               varchar(30)  NOT NULL DEFAULT 'PendienteConfirmacion'
motivo_rechazo       varchar(500) NULL
precio_base          numeric(10,2) NOT NULL
porcentaje_descuento numeric(5,2)  NULL
cupon_id             int NULL → cupones
precio_final         numeric(10,2) NULL
metodo_pago_id       int NULL → metodos_pago
comision_calculada   numeric(10,2) NULL
notas                varchar(500) NULL
ip_origen            varchar(45)  NULL
creado_por_id        int NULL → usuarios
creado_en            timestamptz NOT NULL DEFAULT now()
actualizado_en       timestamptz NOT NULL DEFAULT now()

INDEX(fecha_hora_inicio)
INDEX(operario_id)
INDEX(cliente_id)
INDEX(estado)
INDEX(sesion_id)
```

### 2.6 Módulo Cupones (2 tablas)

#### `cupones`
```
id                  int PK autoincrement
codigo              varchar(50) UNIQUE NOT NULL
descripcion         varchar(300) NULL
tipo_descuento      varchar(20) NOT NULL
valor               numeric(10,2) NOT NULL
servicios_ids       int[] NULL
fecha_desde         date NOT NULL
fecha_hasta         date NOT NULL
usos_maximos        int NULL
usos_actuales       int NOT NULL DEFAULT 0
un_uso_por_cliente  bool NOT NULL DEFAULT true
activo              bool NOT NULL DEFAULT true
creado_en           timestamptz NOT NULL DEFAULT now()
```

#### `cupon_usos`
```
id           int PK autoincrement
cupon_id     int NOT NULL → cupones
cliente_id   int NOT NULL → usuarios
turno_id     int NOT NULL → turnos
usado_en     timestamptz NOT NULL DEFAULT now()

INDEX(cupon_id, cliente_id)
```

### 2.7 Módulo Imputaciones (4 tablas)

#### `categorias_imputacion`
```
id              int PK autoincrement
nombre          varchar(100) NOT NULL UNIQUE
tipo            varchar(10)  NOT NULL
descripcion     varchar(300) NULL
activo          bool NOT NULL DEFAULT true
orden_display   int  NOT NULL DEFAULT 0
creado_en       timestamptz NOT NULL DEFAULT now()
```

#### `metodos_pago`
```
id              int PK autoincrement
nombre          varchar(100) NOT NULL UNIQUE
activo          bool NOT NULL DEFAULT true
orden_display   int  NOT NULL DEFAULT 0
creado_en       timestamptz NOT NULL DEFAULT now()
```

#### `motivos_bloqueo_salon`
```
id              int PK autoincrement
nombre          varchar(100) NOT NULL UNIQUE
activo          bool NOT NULL DEFAULT true
orden_display   int  NOT NULL DEFAULT 0
creado_en       timestamptz NOT NULL DEFAULT now()
```

#### `imputaciones`
```
id              int PK autoincrement
fecha           date NOT NULL
tipo            varchar(10)  NOT NULL
categoria_id    int NOT NULL → categorias_imputacion
descripcion     varchar(500) NULL
monto           numeric(12,2) NOT NULL
turno_id        int NULL → turnos
operario_id     int NULL → usuarios
cargado_por_id  int NOT NULL → usuarios
origen          varchar(20) NOT NULL DEFAULT 'Manual'
creado_en       timestamptz NOT NULL DEFAULT now()

INDEX(fecha)
INDEX(tipo, categoria_id)
INDEX(turno_id)
INDEX(operario_id)
```

### 2.8 Módulo Emails & Notificaciones (3 tablas)

#### `configuracion_email`
```
id                      int PK (siempre 1 — fila única)
recordatorio_dias_antes int NOT NULL DEFAULT 1
postturno_horas_despues int NOT NULL DEFAULT 2
emails_activos          bool NOT NULL DEFAULT true
actualizado_en          timestamptz NOT NULL DEFAULT now()
```

#### `emails_enviados`
```
id              int PK autoincrement
tipo            varchar(50) NOT NULL
destinatario    varchar(255) NOT NULL
turno_id        int NULL → turnos
usuario_id      int NULL → usuarios
estado          varchar(20) NOT NULL
error_detalle   varchar(500) NULL
enviado_en      timestamptz NOT NULL DEFAULT now()

INDEX(tipo, turno_id)
INDEX(tipo, usuario_id)
```

#### `calificaciones`
```
id           int PK autoincrement
turno_id     int NOT NULL UNIQUE → turnos
cliente_id   int NOT NULL → usuarios
operario_id  int NOT NULL → usuarios
puntuacion   int NOT NULL
comentario   varchar(1000) NULL
creado_en    timestamptz NOT NULL DEFAULT now()
```

---

## 3. Enums

```csharp
public enum Rol              { Admin, Operario, Cliente }
public enum AuthProvider     { Local, Google }
public enum EstadoUsuario    { Activo, Inactivo, Bloqueado }
public enum Sexo             { Masculino, Femenino, NoEspecifica }
public enum SexoSubservicio  { Masculino, Femenino, Ambos }
public enum Salon            { Salon1, Salon2, Ambos }
public enum EstadoTurno      { PendienteConfirmacion, Confirmado, Rechazado,
                               Cancelado, Multa, Ausente, Realizado, Impago, Publicidad }
public enum TipoDescuento    { Porcentaje, MontoFijo }
public enum TipoImputacion   { Ingreso, Egreso }
public enum TipoCategoriaImp { Ingreso, Egreso, Ambos }
public enum OrigenImputacion { Manual, Automatico }
public enum TipoEmail        { ConfirmacionRegistro, ConfirmacionTurno, RechazoTurno,
                               RecordatorioTurno, PostTurnoCalificacion,
                               RecuperacionPassword, CambioPassword, Campana }
public enum EstadoEmail      { Enviado, Fallido }
```

---

## 4. Endpoints completos

> Base: `/api/v1/`

Ver secciones 4.1–4.10 del contrato SOT (ETEREO_CONTRATO_SOT.md) para la lista completa.

---

## 5. Lógica de negocio crítica

### 5.1 Filtrado de subservicios por sexo del cliente

```
Cliente con sexo=Masculino   → subservicios WHERE sexo IN ('Masculino', 'Ambos')
Cliente con sexo=Femenino    → subservicios WHERE sexo IN ('Femenino', 'Ambos')
Cliente NoEspecifica o anón  → todos los subservicios
```

### 5.2 Lógica de sesión y descuento automático

```
Al crear POST /sesiones con N zonas (subservicioIds del mismo servicio láser/descartable):
  1. Se crea la sesión
  2. Se crea un turno por cada zona con sesion_id → sesion.id
  3. Si N >= reglas_descuento_sesion.zonas_minimas para ese servicio:
       sesion.descuento_auto_pct = regla.porcentaje_descuento
       → se aplica a precio_final de TODOS los turnos de la sesión
Los packs (es_pack=true) NO usan sesiones. Su precio ya tiene descuento incorporado.
```

### 5.3–5.10

Ver documento original completo para lógica de turnos, imputaciones automáticas, degradar operaria, background jobs, etc.

---

## 6. Seed inicial

```
Usuario admin: admin@etereo.com / (env var ADMIN_PASSWORD)
               rol=Admin, estado=Activo, auth_provider=Local

Categorías de imputación:
  Ingresos: Peluquería, Depilación Descartable, Depilación Láser, Masajes,
            Lash Lifting, Cejas, Facial, Alquiler de Máquina, Pestañas, Otro Ingreso
  Egresos:  Electricidad, Wi-Fi, Seguro, Alquiler Local, Comisión Operaria,
            Agua Destilada, Gel, Papel Depilación, Espátulas Descartables,
            Community Manager, Publicidad, Otro Egreso

Métodos de pago: Efectivo, Transferencia, Tarjeta débito, Tarjeta crédito
Motivos de bloqueo: Feriado, Fin de semana, Vacaciones, Cierre por evento, Otro
Config email: recordatorio_dias_antes=1, postturno_horas_despues=2, emails_activos=true

Reglas de descuento de sesión:
  Depilación Láser:        zonas_minimas=3, porcentaje_descuento=15.00
  Depilación Descartable:  zonas_minimas=3, porcentaje_descuento=10.00

Servicios y subservicios: ver DatabaseSeeder.cs (precios Marzo 2026 cargados).
```

---

## 7. Variables de entorno

```
DATABASE_URL          → connection string PostgreSQL
JWT_SECRET_KEY        → clave HS256 (mínimo 32 chars)
JWT_ISSUER            → ej: "etereo-api"
JWT_AUDIENCE          → ej: "etereo-app"
RESEND_API_KEY        → clave de Resend para envío de emails
RESEND_FROM_EMAIL     → ej: "Etereo <noreply@etereo.com>"
GOOGLE_CLIENT_ID      → para verificación de idToken de Google OAuth
ADMIN_PASSWORD        → contraseña del usuario admin semilla
CORS_ALLOWED_ORIGINS  → origins permitidos en producción
```

---

## 8. Errores por módulo

Ver ETEREO_CONTRATO_SOT.md sección 8 para lista completa de códigos de error.

---

## 9. Convenciones para agregar un nuevo módulo

1. Enums en `Domain/Enums/`
2. Entidades en `Domain/Entities/{Modulo}/` heredando de `EntityBase`. Sin nav properties.
3. EF Configurations en `Infrastructure/Persistence/Configurations/{Modulo}/`
4. DbSets + implementación de `IXxxDbContext` en `AppDbContext`
5. `IXxxDbContext` en `Application/Interfaces/{Modulo}/`
6. DTOs en `Shared/{Modulo}/`
7. Service en `Application/Services/{Modulo}/`. Devuelve `Result<T>`.
8. Controller en `Api/Controllers/{Modulo}/` con `[RequiereRol(...)]`
9. Registrar en `Program.cs`
10. Seeds catálogo si aplica en `DatabaseSeeder`
11. Migración: `dotnet ef migrations add AddXxxModule ...`

---

**Fin del SOT Backend.**
