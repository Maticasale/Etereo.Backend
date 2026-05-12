# SOT_Contrato — Etereo

> Fuente de verdad compartida entre Backend (.NET Core) y Frontend Web (React).
> Define el contrato de API, modelos, enums y convenciones de nombrado.
> Última actualización: Mayo 2026 — v2: sexo de cliente, variantes, sesiones, packs, precios reales.

---

## 1. Convenciones generales

### Nombrado por capa

| Capa | Convención |
|---|---|
| Backend C# (clases, props, métodos) | `PascalCase` |
| Backend PostgreSQL (tablas, columnas) | `snake_case` plural para tablas, singular para columnas |
| Backend JSON (respuestas y requests) | `camelCase` |
| Frontend TS (types, interfaces) | `PascalCase` para tipos; `camelCase` para variables/props |
| Frontend archivos | `PascalCase` para componentes (`.tsx`); `camelCase` para hooks/utils (`.ts`) |

### Reglas de IDs
- Todos los IDs son `int` autoincrement. Sin GUIDs. Sin claves compuestas.

### Fechas
- Backend siempre emite fechas en **UTC** (`DateTime.UtcNow`).
- Frontend convierte a hora local Argentina (UTC-3).
- Formato wire: ISO 8601 (`2026-05-07T14:30:00Z`).
- Campos solo fecha: `YYYY-MM-DD`.
- Campos solo hora: `HH:mm`.

### Enums en JSON
- Se serializan como **string** (no int). Ej: `"estado": "Confirmado"`, no `"estado": 2`.

### Soft delete
- No se borran registros de negocio. Se usa `activo` (bool) o `estado` (enum).

---

## 2. Formato de respuestas HTTP

### Éxito
```json
{ "data": { ... } }
```

### Error
```json
{
  "error": {
    "codigo": "CODIGO_ERROR_SCREAMING_SNAKE",
    "mensaje": "Descripción legible del error"
  }
}
```

### Status codes

| Código | Uso |
|---|---|
| `200` | GET, PUT, PATCH exitoso |
| `201` | POST exitoso (creación) |
| `204` | DELETE exitoso (sin body) |
| `400` | Validación de input o error de negocio genérico |
| `401` | JWT inválido o expirado |
| `403` | Autenticado pero sin permiso de rol |
| `404` | Entidad no encontrada |
| `409` | Conflicto de unicidad o regla de negocio que impide la acción |

---

## 3. Autenticación

### Tokens

| Campo | Tipo | TTL |
|---|---|---|
| `accessToken` | JWT HS256 (Bearer) | 15 minutos |
| `refreshToken` | opaque (SHA-256 hashed en DB) | 30 días |

### Claims en el JWT

| Claim | Tipo | Descripción |
|---|---|---|
| `sub` | `string` | ID del usuario |
| `rol` | `string` | "Admin" \| "Operario" \| "Cliente" |
| `email` | `string` | Email del usuario |

### Flujo de refresh
1. Request falla con `401`.
2. Cliente llama `POST /api/v1/auth/refresh` con `{ "refreshToken": "..." }`.
3. Backend responde con nuevos `accessToken` + `refreshToken` (rotación obligatoria).
4. Si el refresh falla: limpiar tokens y redirigir a `/login`.

---

## 4. Endpoints

> Base: `/api/v1/`

### 4.1 Auth

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| POST | `/auth/register` | Anónimo | Crea cliente. Vincula historial por teléfono. |
| POST | `/auth/login` | Anónimo | Email + password → AuthResponse |
| POST | `/auth/google` | Anónimo | idToken Google → AuthResponse |
| POST | `/auth/refresh` | Anónimo | Rota ambos tokens |
| POST | `/auth/logout` | Anónimo | Revoca refresh token |
| GET | `/auth/me` | [Authorize] | UsuarioDto del token actual |
| POST | `/auth/cambiar-password` | [Authorize] | Cambia contraseña |
| POST | `/auth/forgot-password` | Anónimo | Envía email de recuperación. Siempre 200. |
| POST | `/auth/reset-password` | Anónimo | Valida token y cambia password |

### 4.2 Usuarios

| Método | Ruta | Acceso |
|---|---|---|
| GET | `/usuarios` | Admin |
| GET | `/usuarios/{id}` | Admin\|Operario |
| PATCH | `/usuarios/{id}` | Admin |
| POST | `/usuarios/{id}/bloquear` | Admin |
| POST | `/usuarios/{id}/desbloquear` | Admin |
| POST | `/usuarios/{id}/promover-operario` | Admin |
| POST | `/usuarios/{id}/degradar-cliente` | Admin |
| POST | `/usuarios/clientes` | Admin\|Operario |
| GET | `/usuarios/clientes/buscar` | Admin\|Operario |

### 4.3 Disponibilidad

| Método | Ruta | Acceso |
|---|---|---|
| GET | `/disponibilidad/salon` | Admin\|Operario |
| POST | `/disponibilidad/salon` | Admin |
| DELETE | `/disponibilidad/salon/{id}` | Admin |
| GET | `/disponibilidad/operario/{id}` | Admin\|Operario |
| POST | `/disponibilidad/operario` | Admin\|Operario |

### 4.4 Servicios, Subservicios & Variantes

| Método | Ruta | Acceso |
|---|---|---|
| GET | `/servicios` | Anónimo — filtra subservicios por sexo del cliente autenticado |
| GET | `/servicios/estado-configuracion` | Admin |
| GET | `/servicios/{id}` | Anónimo |
| POST | `/servicios` | Admin |
| PUT | `/servicios/{id}` | Admin |
| PATCH | `/servicios/{id}/estado` | Admin |
| POST | `/subservicios` | Admin |
| PUT | `/subservicios/{id}` | Admin |
| PATCH | `/subservicios/{id}/estado` | Admin |
| POST | `/subservicios/{id}/variantes` | Admin |
| PUT | `/subservicios/{id}/variantes/{vid}` | Admin |
| PATCH | `/subservicios/{id}/variantes/{vid}/estado` | Admin |
| GET | `/reglas-descuento-sesion` | Admin |
| PUT | `/reglas-descuento-sesion/{id}` | Admin |

### 4.5 Operarios

| Método | Ruta | Acceso |
|---|---|---|
| GET | `/operarios` | Admin\|Operario |
| GET | `/operarios/{id}` | Admin\|Operario |
| GET | `/operarios/{id}/subservicios` | Admin\|Operario |
| POST | `/operarios/{id}/subservicios` | Admin |
| PUT | `/operarios/{id}/subservicios/{subservicioId}` | Admin |
| DELETE | `/operarios/{id}/subservicios/{subservicioId}` | Admin |
| GET | `/operarios/{id}/vistas` | Admin |
| PUT | `/operarios/{id}/vistas` | Admin |

### 4.6 Turnos & Sesiones

| Método | Ruta | Acceso |
|---|---|---|
| POST | `/sesiones` | Anónimo\|[Authorize] |
| GET | `/sesiones/{id}` | Admin\|Operario\|Cliente propio |
| POST | `/turnos` | Anónimo\|[Authorize] |
| GET | `/turnos` | Admin\|Operario |
| GET | `/turnos/{id}` | Admin\|Operario\|Cliente propio |
| GET | `/turnos/mis-turnos` | Cliente |
| GET | `/turnos/disponibilidad` | Anónimo |
| POST | `/turnos/{id}/confirmar` | Admin\|Operario |
| POST | `/turnos/{id}/rechazar` | Admin\|Operario |
| POST | `/turnos/{id}/cancelar` | Admin\|Operario\|Cliente propio |
| POST | `/turnos/{id}/multa` | Admin\|Operario |
| POST | `/turnos/{id}/ausente` | Admin\|Operario |
| POST | `/turnos/{id}/realizar` | Admin\|Operario |
| POST | `/turnos/{id}/impago` | Admin\|Operario |
| POST | `/turnos/{id}/publicidad` | Admin\|Operario |

### 4.7 Cupones

| Método | Ruta | Acceso |
|---|---|---|
| GET | `/cupones` | Admin |
| POST | `/cupones` | Admin |
| PUT | `/cupones/{id}` | Admin |
| PATCH | `/cupones/{id}/estado` | Admin |
| GET | `/cupones/disponibles` | Cliente |
| GET | `/cupones/validar/{codigo}` | Cliente |

### 4.8 Imputaciones & Catálogos

| Método | Ruta | Acceso |
|---|---|---|
| GET | `/imputaciones` | Admin |
| GET | `/imputaciones/resumen` | Admin |
| POST | `/imputaciones` | Admin |
| PUT | `/imputaciones/{id}` | Admin |
| DELETE | `/imputaciones/{id}` | Admin |
| GET | `/categorias-imputacion` | Admin\|Operario |
| POST | `/categorias-imputacion` | Admin |
| PUT | `/categorias-imputacion/{id}` | Admin |
| PATCH | `/categorias-imputacion/{id}/estado` | Admin |
| GET | `/metodos-pago` | Anónimo |
| POST | `/metodos-pago` | Admin |
| PUT | `/metodos-pago/{id}` | Admin |
| PATCH | `/metodos-pago/{id}/estado` | Admin |
| GET | `/motivos-bloqueo-salon` | Admin\|Operario |
| POST | `/motivos-bloqueo-salon` | Admin |
| PUT | `/motivos-bloqueo-salon/{id}` | Admin |
| PATCH | `/motivos-bloqueo-salon/{id}/estado` | Admin |

### 4.9 Emails & Notificaciones

| Método | Ruta | Acceso |
|---|---|---|
| GET | `/config/email` | Admin |
| PUT | `/config/email` | Admin |
| POST | `/emails/campana` | Admin |
| POST | `/calificaciones` | Anónimo (con token) |
| GET | `/calificaciones` | Admin |
| GET | `/calificaciones/operario/{id}` | Admin\|Operario propio |

### 4.10 Estadísticas & Dashboard

| Método | Ruta | Acceso |
|---|---|---|
| GET | `/dashboard/kpis` | Admin |
| GET | `/dashboard/alertas` | Admin |
| GET | `/dashboard/agenda-hoy` | Admin\|Operario |
| GET | `/estadisticas/ingresos-egresos` | Admin |
| GET | `/estadisticas/servicios` | Admin |
| GET | `/estadisticas/operarias` | Admin |
| GET | `/estadisticas/turnos` | Admin |
| GET | `/estadisticas/calificaciones` | Admin |
| GET | `/comisiones` | Admin |
| GET | `/comisiones/mi-resumen` | Operario |

---

## 5. DTOs y modelos compartidos

### 5.1 Auth

```typescript
LoginRequest         { email: string; password: string }
RegisterRequest      { email: string; password: string; nombre: string; apellido: string; telefono?: string; sexo?: string }
GoogleAuthRequest    { idToken: string }
RefreshRequest       { refreshToken: string }
AuthResponse         { accessToken: string; refreshToken: string; usuario: UsuarioDto }
UsuarioDto           { id: number; email: string; nombre: string; apellido: string; telefono?: string; rol: string; estado: string; motivoBloqueo?: string; debeCambiarPassword: boolean; avatarUrl?: string; creadoEn: string }
CambiarPasswordRequest   { passwordActual: string; passwordNueva: string }
ForgotPasswordRequest    { email: string }
ResetPasswordRequest     { token: string; passwordNueva: string }
BloquearUsuarioRequest   { motivo: string }
```

### 5.2–5.10

Ver documentación completa en versión anterior del contrato (todos los DTOs de Servicios, Operarios, Turnos, Cupones, Imputaciones, Emails, Dashboard y Comisiones).

---

## 6. Enums (valores de string en JSON)

```
Rol:              Admin | Operario | Cliente
AuthProvider:     Local | Google
EstadoUsuario:    Activo | Inactivo | Bloqueado
Sexo:             Masculino | Femenino | NoEspecifica
SexoSubservicio:  Masculino | Femenino | Ambos
Salon:            Salon1 | Salon2 | Ambos
EstadoTurno:      PendienteConfirmacion | Confirmado | Rechazado | Cancelado |
                  Multa | Ausente | Realizado | Impago | Publicidad
TipoDescuento:    Porcentaje | MontoFijo
TipoImputacion:   Ingreso | Egreso
TipoCategoria:    Ingreso | Egreso | Ambos
OrigenImput:      Manual | Automatico
TipoEmail:        ConfirmacionRegistro | ConfirmacionTurno | RechazoTurno |
                  RecordatorioTurno | PostTurnoCalificacion |
                  RecuperacionPassword | CambioPassword | Campana
```

---

## 7. Reglas de negocio documentadas en el contrato

### Filtrado por sexo del cliente
```
Cliente Masculino   → subservicios WHERE sexo IN ('Masculino', 'Ambos')
Cliente Femenino    → subservicios WHERE sexo IN ('Femenino', 'Ambos')
Cliente NoEspecifica o anónimo → todos, agrupados con separador visual en frontend
```

### Variantes de subservicio
```
Si subservicio.variantes.length > 0 → el cliente debe elegir una variante
El precio y duracion_min del turno vienen de la variante, no del subservicio
varianteId es obligatorio en CrearTurnoRequest y CrearSesionRequest cuando aplica
```

### Sesiones y descuento automático
```
POST /sesiones con N zonas del mismo servicio (Láser o Descartable):
  Si N >= reglas_descuento_sesion.zonas_minimas → se aplica descuento automático
  Láser: default 3 zonas → 15% | Descartable: default 3 zonas → 10%
Los packs (esPack=true) NO usan sesiones — precio fijo ya con descuento
```

### Turnos — estado inicial según creador
```
creado_por = cliente propio o anónimo  →  PendienteConfirmacion
creado_por = Admin o Operario          →  Confirmado (directo)
```

### Turnos — transiciones válidas
```
PendienteConfirmacion → Confirmado | Rechazado
Confirmado → Cancelado | Multa | Ausente | Realizado | Impago | Publicidad
Multa → Confirmado (cuando reagenda y paga el 50%)
```

### Imputaciones automáticas
```
Al marcar turno como Realizado → se crean automáticamente:
  1 Ingreso (origen=Automatico, turno_id=X)
  1 Egreso de comisión si operario != Admin (origen=Automatico, turno_id=X)
Las imputaciones Automatico NO son editables ni eliminables.
```

---

## 8. Seed inicial del sistema

```
Usuario admin:        admin@etereo.com
Métodos de pago:      Efectivo, Transferencia, Tarjeta débito, Tarjeta crédito
Motivos bloqueo:      Feriado, Fin de semana, Vacaciones, Cierre por evento, Otro
Categorías ingreso:   Peluquería, Depilación Descartable, Depilación Láser, Masajes,
                      Lash Lifting, Cejas, Facial, Alquiler de Máquina, Pestañas, Otro Ingreso
Categorías egreso:    Electricidad, Wi-Fi, Seguro, Alquiler Local, Comisión Operaria,
                      Agua Destilada, Gel, Papel Depilación, Espátulas Descartables,
                      Community Manager, Publicidad, Otro Egreso
Config email:         recordatorio_dias_antes=1, postturno_horas_despues=2, activo=true
Reglas descuento:     Depilación Láser → 3 zonas → 15%
                      Depilación Descartable → 3 zonas → 10%
Servicios/precios:    Ver DatabaseSeeder.cs (precios Marzo 2026)
```

---

**Fin del Contrato SOT.**
