# SOT_Contrato — Etereo

> Fuente de verdad compartida entre Backend (.NET Core) y Frontend Web (React).
> Define el contrato de API, modelos, enums y convenciones de nombrado.
> Última actualización: Mayo 2026 — v4: claims JWT corregidos (sexo+jti), accesos imputaciones corregidos, rutas email canonizadas, secciones 5.2-5.10 completadas con DTOs reales.

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

Generados por `JwtService.GenerateAccessToken(Usuario)`:

| Claim | Tipo | Descripción |
|---|---|---|
| `sub` | `string` | ID del usuario (int serializado como string) |
| `rol` | `string` | `"Admin"` \| `"Operario"` \| `"Cliente"` |
| `sexo` | `string` | `"Masculino"` \| `"Femenino"` \| `"NoEspecifica"` — usado para filtrar subservicios en `GET /servicios` |
| `email` | `string` | Email del usuario |
| `jti` | `string` | GUID único del token (para trazabilidad) |

**Nota importante:** Con `opts.MapInboundClaims = false`, los claims llegan con sus nombres originales. Leerlos en controllers con `User.FindFirstValue("sub")`, `User.FindFirstValue("rol")`, `User.FindFirstValue("sexo")`.

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
| GET | `/imputaciones` | Admin\|Operario |
| GET | `/imputaciones/resumen` | Admin |
| POST | `/imputaciones` | Admin\|Operario |
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

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| GET | `/config/email` | Admin | Config global — **ruta canónica** (ConfigEmailController) |
| PUT | `/config/email` | Admin | Actualizar config — **ruta canónica** (ConfigEmailController) |
| GET | `/emails/configuracion` | Admin | Alias duplicado de `/config/email` (EmailsController) |
| PUT | `/emails/configuracion` | Admin | Alias duplicado de `/config/email` (EmailsController) |
| GET | `/emails/historial` | Admin | Historial (filtros: `tipo`, `estado`, `fechaDesde`, `fechaHasta`) |
| POST | `/emails/campana` | Admin | Enviar campaña masiva |
| POST | `/calificaciones` | `[Authorize]` (cualquier rol) | Crear calificación post-turno. El cliente **debe estar logueado**. El link se envía por WhatsApp al frontend (con el token JWT como query param); el frontend lo usa como Bearer en el header. |
| GET | `/calificaciones` | Admin | Listar calificaciones (query: `?operarioId=`) |
| GET | `/calificaciones/operario/{id}` | Admin\|Operario | Promedio de calificaciones de un operario |

### 4.10 Estadísticas & Dashboard

| Método | Ruta | Acceso | Descripción |
|---|---|---|---|
| GET | `/dashboard/kpis` | Admin | KPIs del día: turnos, ingresos, balance, calificaciones |
| GET | `/dashboard/alertas` | Admin | Alertas activas: turnos sin confirmar, balance negativo, etc. |
| GET | `/dashboard/agenda-hoy` | Admin\|Operario | Turnos de hoy; Operario ve sólo los suyos |
| GET | `/estadisticas/resumen` | Admin | Resumen general del mes (turnos, financiero, distribución estados) |
| GET | `/estadisticas/evolucion` | Admin | Evolución ingresos/egresos: `?fechaDesde&fechaHasta&agrupacion=dia\|semana\|mes` |
| GET | `/estadisticas/ingresos-egresos` | Admin | Alias de evolucion con agrupacion=dia por defecto |
| GET | `/estadisticas/servicios` | Admin | Ranking de subservicios por cantidad de turnos realizados |
| GET | `/estadisticas/operarias` | Admin | Estadísticas por operaria: turnos, ingresos, calificaciones |
| GET | `/estadisticas/turnos` | Admin | Distribución de turnos por estado en un rango de fechas |
| GET | `/estadisticas/calificaciones` | Admin | Promedios de calificaciones globales y por operaria |
| GET | `/estadisticas/ocupacion` | Admin | Ocupación diaria: turnos por día en un rango |
| GET | `/comisiones` | Admin | Listado de comisiones (egresos vinculados a operarias) |
| GET | `/comisiones/mi-resumen` | Operario | Resumen de comisiones propias |

---

## 5. DTOs y modelos compartidos

### 5.1 Auth

```typescript
LoginRequest         { email: string; password: string }
RegisterRequest      { email: string; password: string; nombre: string; apellido: string; telefono?: string; sexo?: string }
GoogleAuthRequest    { idToken: string }
RefreshRequest       { refreshToken: string }
AuthResponse         { accessToken: string; refreshToken: string; usuario: UsuarioDto }
UsuarioDto           { id: number; email: string; nombre: string; apellido: string; telefono?: string; sexo: string; rol: string; estado: string; motivoBloqueo?: string; debeCambiarPassword: boolean; avatarUrl?: string; creadoEn: string }
CambiarPasswordRequest   { passwordActual: string; passwordNueva: string }
ForgotPasswordRequest    { email: string }
ResetPasswordRequest     { token: string; passwordNueva: string }
BloquearUsuarioRequest   { motivo: string }
```

### 5.2 Usuarios

```typescript
// Requests
ActualizarUsuarioRequest { nombre?: string; apellido?: string; telefono?: string; sexo?: string }
BloquearUsuarioRequest   { motivo: string }
CrearClienteRequest      { nombre: string; apellido: string; email?: string; telefono?: string; sexo?: string }

// Responses
UsuariosListResponse     { items: UsuarioDto[]; total: number }
// UsuarioDto — definido en sección 5.1
```

### 5.3 Servicios, Subservicios y Variantes

```typescript
// Requests
CrearServicioRequest        { nombre: string; salon: string; categoriaImputacionId?: number; ordenDisplay?: number }
ActualizarServicioRequest   { nombre?: string; salon?: string; categoriaImputacionId?: number; ordenDisplay?: number }
EstadoRequest               { activo: boolean }

CrearSubservicioRequest     { servicioId: number; nombre: string; descripcion?: string; precio?: number;
                              duracionMin?: number; requiereSilencio?: boolean; esPack?: boolean;
                              detallePack?: string; sexo?: string; ordenDisplay?: number }
ActualizarSubservicioRequest { nombre?: string; descripcion?: string; precio?: number; duracionMin?: number;
                               requiereSilencio?: boolean; esPack?: boolean; detallePack?: string;
                               sexo?: string; ordenDisplay?: number }

CrearVarianteRequest        { nombre: string; precio: number; duracionMin: number; sexo?: string; ordenDisplay?: number }
ActualizarVarianteRequest   { nombre?: string; precio?: number; duracionMin?: number; sexo?: string; ordenDisplay?: number }

ActualizarReglaDescuentoRequest { zonasMinimas: number; porcentajeDescuento: number; activo: boolean }

// Responses
VarianteDto     { id: number; nombre: string; precio: number; duracionMin: number; sexo: string; activo: boolean; ordenDisplay: number }
SubservicioDto  { id: number; servicioId: number; nombre: string; descripcion?: string; precio?: number;
                  duracionMin?: number; requiereSilencio: boolean; esPack: boolean; detallePack?: string;
                  sexo: string; activo: boolean; ordenDisplay: number; variantes: VarianteDto[] }
ServicioDto     { id: number; nombre: string; salon: string; categoriaImputacionId?: number;
                  activo: boolean; ordenDisplay: number; subservicios: SubservicioDto[] }
EstadoConfiguracionDto { configurado: boolean; mensaje: string }
ReglaDescuentoDto { id: number; servicioId: number; nombreServicio: string;
                    zonasMinimas: number; porcentajeDescuento: number; activo: boolean }
```

### 5.4 Operarios y Disponibilidad

```typescript
// Requests
AsignarSubservicioRequest       { subservicioId: number; porcentajeComision: number }
ActualizarComisionRequest       { porcentajeComision: number }
ActualizarVistasRequest         { verMisTurnos: boolean; verMisComisiones: boolean; verMiCalificacion: boolean; verMisEstadisticas: boolean }
CrearDisponibilidadSalonRequest    { fecha: string; salon: string; motivoId: number; descripcion?: string }
CrearDisponibilidadOperarioRequest { operarioId: number; fecha: string; trabaja: boolean; motivoAusencia?: string }

// Responses
OperarioSubservicioDto  { id: number; operarioId: number; subservicioId: number;
                          nombreSubservicio: string; nombreServicio: string; porcentajeComision: number }
OperarioVistasDto       { id: number; operarioId: number; verMisTurnos: boolean; verMisComisiones: boolean;
                          verMiCalificacion: boolean; verMisEstadisticas: boolean }
DisponibilidadSalonDto    { id: number; fecha: string; salon: string; motivoId: number;
                            nombreMotivo: string; descripcion?: string; creadoPorId: number }
DisponibilidadOperarioDto { id: number; operarioId: number; fecha: string; trabaja: boolean; motivoAusencia?: string }
```

### 5.5 Turnos y Sesiones

```typescript
// Requests
CrearTurnoRequest         { clienteId?: number; nombreAnonimo?: string; telefonoAnonimo?: string;
                            operarioId: number; subservicioId: number; varianteId?: number;
                            fechaHoraInicio: string; notas?: string; cuponId?: number }
CrearSesionRequest        { clienteId?: number; nombreAnonimo?: string; telefonoAnonimo?: string;
                            operarioId: number; salon: string; fechaHoraInicio: string;
                            zonas: { subservicioId: number; varianteId?: number }[] }
RechazarTurnoRequest      { motivoRechazo: string }
RealizarTurnoRequest      { metodoPagoId: number; precioFinal: number }

// Responses
TurnoDto {
  id: number; salon: string; clienteId?: number; nombreCliente?: string; nombreAnonimo?: string; telefonoAnonimo?: string;
  operarioId: number; nombreOperario: string; subservicioId: number; nombreSubservicio: string; nombreServicio: string;
  varianteId?: number; nombreVariante?: string; sesionId?: number; fechaHoraInicio: string; duracionMin: number;
  estado: string; motivoRechazo?: string; precioBase: number; porcentajeDescuento?: number;
  cuponId?: number; precioFinal?: number; metodoPagoId?: number; nombreMetodoPago?: string;
  comisionCalculada?: number; notas?: string; creadoEn: string; actualizadoEn: string
}
SesionDto {
  id: number; clienteId?: number; nombreCliente?: string; nombreAnonimo?: string; telefonoAnonimo?: string;
  operarioId: number; nombreOperario: string; salon: string; fechaHoraInicio: string; estado: string;
  descuentoAutoPct?: number; turnos: TurnoDto[]; creadoEn: string
}
SlotOcupadoDto    { inicio: string; fin: string; estado: string }
DisponibilidadDto { disponible: boolean; motivoNoDisponible?: string;
                    slotsOcupados: SlotOcupadoDto[]; horariosDisponibles: string[] }
```

### 5.6 Cupones

```typescript
// Requests
CrearCuponRequest    { codigo: string; descripcion?: string; tipoDescuento: string; valor: number;
                       serviciosIds?: number[]; fechaDesde: string; fechaHasta: string;
                       usosMaximos?: number; unUsoPorCliente: boolean }
ActualizarCuponRequest { descripcion?: string; valor?: number; serviciosIds?: number[];
                          fechaDesde?: string; fechaHasta?: string; usosMaximos?: number; unUsoPorCliente?: boolean }
EstadoCuponRequest   { activo: boolean }

// Response
CuponDto { id: number; codigo: string; descripcion?: string; tipoDescuento: string; valor: number;
           serviciosIds?: number[]; fechaDesde: string; fechaHasta: string; usosMaximos?: number;
           usosActuales: number; unUsoPorCliente: boolean; activo: boolean; creadoEn: string }
```

### 5.7 Imputaciones y Catálogos

```typescript
// Requests
CrearImputacionRequest    { fecha: string; tipo: string; categoriaId: number; descripcion?: string;
                            monto: number; turnoId?: number; operarioId?: number }
ActualizarImputacionRequest { fecha?: string; categoriaId?: number; descripcion?: string; monto?: number; operarioId?: number }
CrearCategoriaImputacionRequest  { nombre: string; tipo: string; descripcion?: string }
ActualizarCategoriaImputacionRequest { nombre?: string; descripcion?: string }
EstadoImputacionRequest  { activo: boolean }
CrearMetodoPagoRequest   { nombre: string }
ActualizarMetodoPagoRequest { nombre: string }
CrearMotivoBloqueoRequest   { nombre: string }
ActualizarMotivoBloqueoRequest { nombre: string }

// Responses
ImputacionDto { id: number; fecha: string; tipo: string; categoriaId: number; nombreCategoria: string;
                descripcion?: string; monto: number; turnoId?: number; operarioId?: number;
                nombreOperario?: string; cargadoPorId: number; origen: string; creadoEn: string }
ResumenImputacionesDto { totalIngresos: number; totalEgresos: number; balance: number;
                          porCategoria: { nombreCategoria: string; tipo: string; total: number }[] }
CategoriaImputacionDto { id: number; nombre: string; tipo: string; descripcion?: string; activo: boolean; ordenDisplay: number }
MetodoPagoDto          { id: number; nombre: string; activo: boolean; ordenDisplay: number }
MotivoBloqueoSalonDto  { id: number; nombre: string; activo: boolean; ordenDisplay: number }
```

### 5.8 Emails, Calificaciones y Configuración

```typescript
// Requests
ActualizarConfiguracionEmailRequest { recordatorioDiasAntes?: number; postturnoHorasDespues?: number; emailsActivos?: boolean }
CrearCalificacionRequest            { turnoId: number; puntuacion: number; comentario?: string }
EnviarCampanaRequest                { emails: string[]; asunto: string; contenido: string }

// Responses
ConfiguracionEmailDto { id: number; recordatorioDiasAntes: number; postturnoHorasDespues: number;
                        emailsActivos: boolean; actualizadoEn: string }
EmailEnviadoDto       { id: number; tipo: string; destinatario: string; turnoId?: number; usuarioId?: number;
                        estado: string; errorDetalle?: string; enviadoEn: string }
CalificacionDto       { id: number; turnoId: number; clienteId: number; nombreCliente: string;
                        operarioId: number; nombreOperario: string; puntuacion: number; comentario?: string; creadoEn: string }
PromedioCalificacionDto { operarioId: number; nombreOperario: string; promedio: number; totalCalificaciones: number }
```

### 5.9 Estadísticas

```typescript
// Todos los endpoints de /estadisticas son GET con parámetros de query
// Responses:
ResumenEstadisticasDto {
  // Turnos
  turnosHoy: number; turnosSemana: number; turnosMes: number;
  // Financiero
  ingresosHoy: number; ingresosSemana: number; ingresosMes: number;
  egresosHoy: number; egresosSemana: number; egresosMes: number;
  balanceHoy: number; balanceSemana: number; balanceMes: number;  // calculados
  // Calificaciones
  promedioCalificacionGlobal: number; totalCalificaciones: number;
  // Distribución
  turnosPorEstado: { estado: string; cantidad: number; porcentaje: number }[]
}

// GET /estadisticas/evolucion e /estadisticas/ingresos-egresos
PuntoEvolucionDto { periodo: string; ingresos: number; egresos: number; balance: number }

// GET /estadisticas/servicios
ServicioRankingDto { subservicioId: number; nombreServicio: string; nombreCategoria: string;
                     cantidadTurnos: number; ingresoTotal: number }

// GET /estadisticas/operarias
OperariaEstadisticasDto { operarioId: number; nombre: string; turnosMes: number; turnosRealizados: number;
                           ingresosMes: number; comisionesMes: number; promedioCalificacion: number; totalCalificaciones: number }

// GET /estadisticas/ocupacion
OcupacionDiariaDto { fecha: string; totalTurnos: number; turnosRealizados: number; turnosCancelados: number; turnosPendientes: number }

// GET /estadisticas/turnos
TurnosEstadisticasDto { total: number; porEstado: { estado: string; cantidad: number; porcentaje: number }[] }

// GET /estadisticas/calificaciones
CalificacionesEstadisticasDto {
  promedioGlobal: number; total: number;
  porOperario: { operarioId: number; nombre: string; promedio: number; total: number }[]
}
```

### 5.10 Dashboard y Comisiones

```typescript
// GET /dashboard/kpis → ResumenEstadisticasDto (mismo tipo que /estadisticas/resumen pero filtrado a hoy)

// GET /dashboard/alertas → AlertaDashboardDto[]
AlertaDashboardDto { tipo: string; mensaje: string; prioridad: string; cantidad?: number }
// tipo puede ser: "TURNOS_SIN_CONFIRMAR", "OPERARIO_SIN_TURNO", "BALANCE_NEGATIVO", etc.

// GET /dashboard/agenda-hoy → AgendaHoyItemDto[]
AgendaHoyItemDto { turnoId: number; horaInicio: string; horaFin: string; cliente: string;
                   operario: string; servicio: string; estado: string; precioFinal?: number }

// GET /comisiones → ComisionDto[]
ComisionDto { id: number; operarioId: number; nombreOperario: string; turnoId?: number;
              monto: number; fecha: string; concepto: string }

// GET /comisiones/mi-resumen → ResumenComisionesDto
ResumenComisionesDto { operarioId: number; nombre: string; totalComisiones: number; comisiones: ComisionDto[] }
```

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

## 9. Códigos de error

Ver `ETEREO_BACKEND_SOT.md` sección 8 para la tabla completa de `ErrorCode` por módulo y sus HTTP status codes.

Regla general de mapeo status:
- `4xx` error de negocio → `400` si no hay código más específico
- `_NOT_FOUND` → `404`
- `_EN_USO` / `_INVALIDA` (conflicto) → `409`
- `_SIN_PERMISO` → `403`
- `TOKEN_*` → `401`
- `SALON_INVALIDO` / `TIPO_*_INVALIDO` → `422`

---

**Fin del Contrato SOT.**
