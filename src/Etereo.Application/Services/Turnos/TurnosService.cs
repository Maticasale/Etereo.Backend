using Etereo.Application.Common;
using Etereo.Application.Interfaces.Emails;
using Etereo.Application.Interfaces.Turnos;
using Etereo.Domain.Entities.Cupones;
using Etereo.Domain.Entities.Imputaciones;
using Etereo.Domain.Entities.Turnos;
using Etereo.Domain.Enums;
using Etereo.Shared.Turnos;
using Microsoft.EntityFrameworkCore;

namespace Etereo.Application.Services.Turnos;

public class TurnosService : ITurnosService
{
    private readonly ITurnosDbContext _db;
    private readonly IEmailsService   _emails;

    // Horario de atención Argentina (UTC-3). Business hours → UTC: +3h
    private static readonly TimeSpan ArgOffset     = TimeSpan.FromHours(3);
    private const int HoraAperturaArg = 9;
    private const int HoraCierreArg   = 21;
    private const int SlotMinutos     = 15;

    public TurnosService(ITurnosDbContext db, IEmailsService emails)
    {
        _db     = db;
        _emails = emails;
    }

    // ── Crear sesión ──────────────────────────────────────────────────────────

    public async Task<Result<SesionDto>> CrearSesionAsync(CrearSesionRequest req, int? creadoPorId)
    {
        if (!Enum.TryParse<Salon>(req.Salon, true, out var salon))
            return Result<SesionDto>.Failure("SALON_INVALIDO", "Valor de salón inválido. Use: Salon1, Salon2 o Ambos.");

        if (!req.Zonas.Any())
            return Result<SesionDto>.Failure("ZONAS_REQUERIDAS", "Debe incluir al menos una zona.");

        var operario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == req.OperarioId && u.Rol == Rol.Operario);
        if (operario is null)
            return Result<SesionDto>.Failure("OPERARIO_NO_ENCONTRADO", "Operario no encontrado.");

        if (req.ClienteId.HasValue)
        {
            var clienteExiste = await _db.Usuarios.AnyAsync(u => u.Id == req.ClienteId.Value && u.Rol == Rol.Cliente);
            if (!clienteExiste)
                return Result<SesionDto>.Failure("CLIENTE_NO_ENCONTRADO", "Cliente no encontrado.");
        }

        var subIds = req.Zonas.Select(z => z.SubservicioId).ToList();
        var subservicios = await _db.Subservicios.Where(s => subIds.Contains(s.Id)).ToListAsync();
        if (subservicios.Count != subIds.Distinct().Count())
            return Result<SesionDto>.Failure("SUBSERVICIO_NO_ENCONTRADO", "Uno o más subservicios no existen.");

        var servicioId = subservicios.Select(s => s.ServicioId).Distinct().FirstOrDefault();

        var regla = await _db.ReglasDescuentoSesion
            .FirstOrDefaultAsync(r => r.ServicioId == servicioId && r.Activo);
        decimal? descuentoAutoPct = regla != null && req.Zonas.Count >= regla.ZonasMinimas
            ? regla.PorcentajeDescuento
            : null;

        var creadoPorRol = await GetRolAsync(creadoPorId);
        var estadoInicial = EsAdminOOperario(creadoPorRol)
            ? EstadoTurno.Confirmado
            : EstadoTurno.PendienteConfirmacion;

        var sesion = new Sesion
        {
            ClienteId        = req.ClienteId,
            NombreAnonimo    = req.NombreAnonimo?.Trim(),
            TelefonoAnonimo  = req.TelefonoAnonimo?.Trim(),
            OperarioId       = req.OperarioId,
            Salon            = salon,
            FechaHoraInicio  = req.FechaHoraInicio,
            Estado           = estadoInicial,
            DescuentoAutoPct = descuentoAutoPct,
            CreadoEn         = DateTime.UtcNow
        };
        _db.AddSesion(sesion);
        await _db.SaveChangesAsync();  // para obtener sesion.Id

        var varianteIds = req.Zonas.Where(z => z.VarianteId.HasValue).Select(z => z.VarianteId!.Value).ToList();
        var variantes = varianteIds.Any()
            ? await _db.VariantesSubservicio.Where(v => varianteIds.Contains(v.Id)).ToListAsync()
            : new List<Etereo.Domain.Entities.Servicios.VarianteSubservicio>();

        var turnosCreados = new List<Turno>();
        foreach (var zona in req.Zonas)
        {
            var sub      = subservicios.First(s => s.Id == zona.SubservicioId);
            var variante = zona.VarianteId.HasValue ? variantes.FirstOrDefault(v => v.Id == zona.VarianteId.Value) : null;

            var precioBase  = variante?.Precio ?? sub.Precio ?? 0m;
            var duracion    = variante?.DuracionMin ?? sub.DuracionMin ?? 30;
            var precioFinal = descuentoAutoPct.HasValue
                ? Math.Round(precioBase * (1 - descuentoAutoPct.Value / 100m), 2)
                : precioBase;

            var turno = new Turno
            {
                Salon               = salon,
                ClienteId           = req.ClienteId,
                NombreAnonimo       = req.NombreAnonimo?.Trim(),
                TelefonoAnonimo     = req.TelefonoAnonimo?.Trim(),
                OperarioId          = req.OperarioId,
                SubservicioId       = zona.SubservicioId,
                VarianteId          = zona.VarianteId,
                SesionId            = sesion.Id,
                FechaHoraInicio     = req.FechaHoraInicio,
                DuracionMin         = duracion,
                Estado              = estadoInicial,
                PrecioBase          = precioBase,
                PorcentajeDescuento = descuentoAutoPct,
                PrecioFinal         = precioFinal,
                CreadoPorId         = creadoPorId,
                CreadoEn            = DateTime.UtcNow,
                ActualizadoEn       = DateTime.UtcNow
            };
            _db.AddTurno(turno);
            turnosCreados.Add(turno);
        }
        await _db.SaveChangesAsync();

        return Result<SesionDto>.Success(await BuildSesionDtoAsync(sesion, turnosCreados));
    }

    // ── Obtener sesión ────────────────────────────────────────────────────────

    public async Task<Result<SesionDto>> ObtenerSesionAsync(int id, int? usuarioId, string? rol)
    {
        var sesion = await _db.Sesiones.FirstOrDefaultAsync(s => s.Id == id);
        if (sesion is null)
            return Result<SesionDto>.Failure("SESION_NO_ENCONTRADA", "Sesión no encontrada.");

        if (!PuedeVerTurno(sesion.ClienteId, sesion.OperarioId, usuarioId, rol))
            return Result<SesionDto>.Failure("SIN_PERMISO", "No tiene permiso para ver esta sesión.");

        var turnos = await _db.Turnos.Where(t => t.SesionId == id).ToListAsync();
        return Result<SesionDto>.Success(await BuildSesionDtoAsync(sesion, turnos));
    }

    // ── Crear turno individual ────────────────────────────────────────────────

    public async Task<Result<TurnoDto>> CrearTurnoAsync(CrearTurnoRequest req, int? creadoPorId)
    {
        var operario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == req.OperarioId && u.Rol == Rol.Operario);
        if (operario is null)
            return Result<TurnoDto>.Failure("OPERARIO_NO_ENCONTRADO", "Operario no encontrado.");

        if (req.ClienteId.HasValue)
        {
            var clienteExiste = await _db.Usuarios.AnyAsync(u => u.Id == req.ClienteId.Value && u.Rol == Rol.Cliente);
            if (!clienteExiste)
                return Result<TurnoDto>.Failure("CLIENTE_NO_ENCONTRADO", "Cliente no encontrado.");
        }

        var sub = await _db.Subservicios.FirstOrDefaultAsync(s => s.Id == req.SubservicioId);
        if (sub is null)
            return Result<TurnoDto>.Failure("SUBSERVICIO_NO_ENCONTRADO", "Subservicio no encontrado.");

        Etereo.Domain.Entities.Servicios.VarianteSubservicio? variante = null;
        if (req.VarianteId.HasValue)
        {
            variante = await _db.VariantesSubservicio
                .FirstOrDefaultAsync(v => v.Id == req.VarianteId.Value && v.SubservicioId == req.SubservicioId);
            if (variante is null)
                return Result<TurnoDto>.Failure("VARIANTE_NO_ENCONTRADA", "Variante no encontrada.");
        }

        var precioBase = variante?.Precio ?? sub.Precio ?? 0m;
        var duracion   = variante?.DuracionMin ?? sub.DuracionMin ?? 30;
        var svc        = await _db.Servicios.FirstOrDefaultAsync(s => s.Id == sub.ServicioId);
        var salon      = svc?.Salon ?? Salon.Salon1;

        // Cupón
        decimal? porcentajeDescuento = null;
        decimal? precioFinal         = null;
        if (req.CuponId.HasValue)
        {
            var cuponResult = await ValidarCuponAsync(req.CuponId.Value, req.ClienteId, sub.ServicioId, precioBase);
            if (!cuponResult.IsSuccess)
                return Result<TurnoDto>.Failure(cuponResult.ErrorCode!, cuponResult.ErrorMessage!);
            porcentajeDescuento = cuponResult.Value.Pct;
            precioFinal         = cuponResult.Value.PrecioFinal;
        }

        var creadoPorRol  = await GetRolAsync(creadoPorId);
        var estadoInicial = EsAdminOOperario(creadoPorRol)
            ? EstadoTurno.Confirmado
            : EstadoTurno.PendienteConfirmacion;

        var turno = new Turno
        {
            Salon               = salon,
            ClienteId           = req.ClienteId,
            NombreAnonimo       = req.NombreAnonimo?.Trim(),
            TelefonoAnonimo     = req.TelefonoAnonimo?.Trim(),
            OperarioId          = req.OperarioId,
            SubservicioId       = req.SubservicioId,
            VarianteId          = req.VarianteId,
            FechaHoraInicio     = req.FechaHoraInicio,
            DuracionMin         = duracion,
            Estado              = estadoInicial,
            PrecioBase          = precioBase,
            PorcentajeDescuento = porcentajeDescuento,
            CuponId             = req.CuponId,
            PrecioFinal         = precioFinal,
            Notas               = req.Notas?.Trim(),
            CreadoPorId         = creadoPorId,
            CreadoEn            = DateTime.UtcNow,
            ActualizadoEn       = DateTime.UtcNow
        };
        _db.AddTurno(turno);
        await _db.SaveChangesAsync();   // turno.Id se asigna aquí

        // Registrar uso del cupón
        if (req.CuponId.HasValue && req.ClienteId.HasValue)
        {
            var cupon = await _db.Cupones.FirstAsync(c => c.Id == req.CuponId.Value);
            cupon.UsosActuales++;
            _db.AddCuponUso(new CuponUso
            {
                CuponId   = req.CuponId.Value,
                ClienteId = req.ClienteId.Value,
                TurnoId   = turno.Id,
                UsadoEn   = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }

        return Result<TurnoDto>.Success(await BuildTurnoDtoAsync(turno));
    }

    // ── Listar turnos ─────────────────────────────────────────────────────────

    public async Task<Result<List<TurnoDto>>> ListarTurnosAsync(DateOnly? fecha, int? operarioId, string? estado)
    {
        var query = _db.Turnos.AsQueryable();

        if (fecha.HasValue)
        {
            var desde = fecha.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            var hasta = desde.AddDays(1);
            query = query.Where(t => t.FechaHoraInicio >= desde && t.FechaHoraInicio < hasta);
        }
        if (operarioId.HasValue)
            query = query.Where(t => t.OperarioId == operarioId.Value);

        if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoTurno>(estado, true, out var estadoEnum))
            query = query.Where(t => t.Estado == estadoEnum);

        var turnos = await query.OrderBy(t => t.FechaHoraInicio).ToListAsync();
        var dtos   = new List<TurnoDto>();
        foreach (var t in turnos)
            dtos.Add(await BuildTurnoDtoAsync(t));

        return Result<List<TurnoDto>>.Success(dtos);
    }

    // ── Obtener turno ─────────────────────────────────────────────────────────

    public async Task<Result<TurnoDto>> ObtenerTurnoAsync(int id, int? usuarioId, string? rol)
    {
        var turno = await _db.Turnos.FirstOrDefaultAsync(t => t.Id == id);
        if (turno is null)
            return Result<TurnoDto>.Failure("TURNO_NO_ENCONTRADO", "Turno no encontrado.");

        if (!PuedeVerTurno(turno.ClienteId, turno.OperarioId, usuarioId, rol))
            return Result<TurnoDto>.Failure("SIN_PERMISO", "No tiene permiso para ver este turno.");

        return Result<TurnoDto>.Success(await BuildTurnoDtoAsync(turno));
    }

    // ── Mis turnos ────────────────────────────────────────────────────────────

    public async Task<Result<List<TurnoDto>>> MisTurnosAsync(int clienteId)
    {
        var turnos = await _db.Turnos
            .Where(t => t.ClienteId == clienteId)
            .OrderByDescending(t => t.FechaHoraInicio)
            .ToListAsync();

        var dtos = new List<TurnoDto>();
        foreach (var t in turnos)
            dtos.Add(await BuildTurnoDtoAsync(t));

        return Result<List<TurnoDto>>.Success(dtos);
    }

    // ── Disponibilidad ────────────────────────────────────────────────────────

    public async Task<Result<DisponibilidadDto>> ObtenerDisponibilidadAsync(DateOnly fecha, int operarioId, int duracionMin)
    {
        var existeOperario = await _db.Usuarios.AnyAsync(u => u.Id == operarioId && u.Rol == Rol.Operario);
        if (!existeOperario)
            return Result<DisponibilidadDto>.Failure("OPERARIO_NO_ENCONTRADO", "Operario no encontrado.");

        var bloqueado = await _db.DisponibilidadesSalon.AnyAsync(d => d.Fecha == fecha);
        if (bloqueado)
            return Result<DisponibilidadDto>.Success(new DisponibilidadDto
            {
                Disponible = false,
                MotivoNoDisponible = "El salón está bloqueado ese día."
            });

        var ausente = await _db.DisponibilidadesOperario
            .AnyAsync(d => d.OperarioId == operarioId && d.Fecha == fecha && !d.Trabaja);
        if (ausente)
            return Result<DisponibilidadDto>.Success(new DisponibilidadDto
            {
                Disponible = false,
                MotivoNoDisponible = "El operario no trabaja ese día."
            });

        // Argentina UTC-3: 9:00 ARG = 12:00 UTC
        var diaInicioUtc = new DateTime(fecha.Year, fecha.Month, fecha.Day, 0, 0, 0, DateTimeKind.Utc).Add(ArgOffset);
        var diaFinUtc    = diaInicioUtc.AddDays(1);

        var turnosDelDia = await _db.Turnos
            .Where(t => t.OperarioId == operarioId
                     && t.FechaHoraInicio >= diaInicioUtc
                     && t.FechaHoraInicio < diaFinUtc
                     && (t.Estado == EstadoTurno.Confirmado || t.Estado == EstadoTurno.PendienteConfirmacion))
            .ToListAsync();

        var slotsOcupados = turnosDelDia.Select(t => new SlotOcupadoDto
        {
            Inicio = t.FechaHoraInicio,
            Fin    = t.FechaHoraInicio.AddMinutes(t.DuracionMin),
            Estado = t.Estado.ToString()
        }).ToList();

        var horariosDisponibles = new List<DateTime>();
        var cursor = new DateTime(fecha.Year, fecha.Month, fecha.Day, HoraAperturaArg, 0, 0, DateTimeKind.Utc).Add(ArgOffset);
        var cierreUtc = new DateTime(fecha.Year, fecha.Month, fecha.Day, HoraCierreArg, 0, 0, DateTimeKind.Utc).Add(ArgOffset);

        while (cursor.AddMinutes(duracionMin) <= cierreUtc)
        {
            var slotFin = cursor.AddMinutes(duracionMin);
            bool solapado = slotsOcupados.Any(s => cursor < s.Fin && slotFin > s.Inicio);
            if (!solapado)
                horariosDisponibles.Add(cursor);
            cursor = cursor.AddMinutes(SlotMinutos);
        }

        return Result<DisponibilidadDto>.Success(new DisponibilidadDto
        {
            Disponible          = true,
            SlotsOcupados       = slotsOcupados,
            HorariosDisponibles = horariosDisponibles
        });
    }

    // ── Transiciones de estado ────────────────────────────────────────────────

    public async Task<Result<TurnoDto>> ConfirmarTurnoAsync(int id)
    {
        var t = await _db.Turnos.FirstOrDefaultAsync(x => x.Id == id);
        if (t is null) return NotFound();
        if (t.Estado != EstadoTurno.PendienteConfirmacion)
            return TransicionInvalida();

        t.Estado = EstadoTurno.Confirmado;
        t.ActualizadoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _ = _emails.NotificarConfirmacionAsync(t.Id);
        return Result<TurnoDto>.Success(await BuildTurnoDtoAsync(t));
    }

    public async Task<Result<TurnoDto>> RechazarTurnoAsync(int id, RechazarTurnoRequest req)
    {
        var t = await _db.Turnos.FirstOrDefaultAsync(x => x.Id == id);
        if (t is null) return NotFound();
        if (t.Estado != EstadoTurno.PendienteConfirmacion)
            return TransicionInvalida();

        t.Estado        = EstadoTurno.Rechazado;
        t.MotivoRechazo = req.MotivoRechazo;
        t.ActualizadoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _ = _emails.NotificarRechazoAsync(t.Id, req.MotivoRechazo);
        return Result<TurnoDto>.Success(await BuildTurnoDtoAsync(t));
    }

    public async Task<Result<TurnoDto>> CancelarTurnoAsync(int id, int? usuarioId, string? rol)
    {
        var t = await _db.Turnos.FirstOrDefaultAsync(x => x.Id == id);
        if (t is null) return NotFound();

        if (rol == "Cliente" && t.ClienteId != usuarioId)
            return Result<TurnoDto>.Failure("SIN_PERMISO", "Solo puede cancelar sus propios turnos.");

        if (t.Estado != EstadoTurno.Confirmado && t.Estado != EstadoTurno.PendienteConfirmacion)
            return TransicionInvalida();

        t.Estado = EstadoTurno.Cancelado;
        t.ActualizadoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        _ = _emails.NotificarCancelacionAsync(t.Id);
        return Result<TurnoDto>.Success(await BuildTurnoDtoAsync(t));
    }

    public async Task<Result<TurnoDto>> MultaTurnoAsync(int id)
        => await CambiarEstadoDesdeConfirmadoAsync(id, EstadoTurno.Multa);

    public async Task<Result<TurnoDto>> AusenteTurnoAsync(int id)
        => await CambiarEstadoDesdeConfirmadoAsync(id, EstadoTurno.Ausente);

    public async Task<Result<TurnoDto>> ImpagoTurnoAsync(int id)
        => await CambiarEstadoDesdeConfirmadoAsync(id, EstadoTurno.Impago);

    public async Task<Result<TurnoDto>> PublicidadTurnoAsync(int id)
        => await CambiarEstadoDesdeConfirmadoAsync(id, EstadoTurno.Publicidad);

    public async Task<Result<TurnoDto>> RealizarTurnoAsync(int id, RealizarTurnoRequest req)
    {
        var t = await _db.Turnos.FirstOrDefaultAsync(x => x.Id == id);
        if (t is null) return NotFound();
        if (t.Estado != EstadoTurno.Confirmado)
            return TransicionInvalida();

        var metodoPagoExiste = await _db.MetodosPago.AnyAsync(m => m.Id == req.MetodoPagoId && m.Activo);
        if (!metodoPagoExiste)
            return Result<TurnoDto>.Failure("METODO_PAGO_NO_ENCONTRADO", "Método de pago no encontrado.");

        t.Estado        = EstadoTurno.Realizado;
        t.MetodoPagoId  = req.MetodoPagoId;
        t.PrecioFinal   = req.PrecioFinal;
        t.ActualizadoEn = DateTime.UtcNow;

        // Calcular comisión
        var asignacion = await _db.OperarioSubservicios
            .FirstOrDefaultAsync(os => os.OperarioId == t.OperarioId && os.SubservicioId == t.SubservicioId);
        if (asignacion != null)
            t.ComisionCalculada = Math.Round(req.PrecioFinal * (asignacion.PorcentajeComision / 100m), 2);

        // Imputación ingreso automática
        var sub = await _db.Subservicios.FirstOrDefaultAsync(s => s.Id == t.SubservicioId);
        var svc = sub is not null ? await _db.Servicios.FirstOrDefaultAsync(s => s.Id == sub.ServicioId) : null;

        if (svc?.CategoriaImputacionId is not null)
        {
            _db.AddImputacion(new Imputacion
            {
                Fecha        = DateOnly.FromDateTime(t.FechaHoraInicio),
                Tipo         = TipoImputacion.Ingreso,
                CategoriaId  = svc.CategoriaImputacionId!.Value,
                Descripcion  = $"Turno #{t.Id} — {sub?.Nombre}",
                Monto        = req.PrecioFinal,
                TurnoId      = t.Id,
                OperarioId   = t.OperarioId,
                CargadoPorId = t.OperarioId,
                Origen       = OrigenImputacion.Automatico,
                CreadoEn     = DateTime.UtcNow
            });
        }

        // Imputación egreso comisión automática
        if (t.ComisionCalculada is > 0)
        {
            var catComision = await _db.CategoriasImputacion
                .FirstOrDefaultAsync(c => c.Nombre == "Comisión Operaria");
            if (catComision != null)
            {
                _db.AddImputacion(new Imputacion
                {
                    Fecha        = DateOnly.FromDateTime(t.FechaHoraInicio),
                    Tipo         = TipoImputacion.Egreso,
                    CategoriaId  = catComision.Id,
                    Descripcion  = $"Comisión turno #{t.Id} — {sub?.Nombre}",
                    Monto        = t.ComisionCalculada.Value,
                    TurnoId      = t.Id,
                    OperarioId   = t.OperarioId,
                    CargadoPorId = t.OperarioId,
                    Origen       = OrigenImputacion.Automatico,
                    CreadoEn     = DateTime.UtcNow
                });
            }
        }

        await _db.SaveChangesAsync();
        _ = _emails.NotificarPostTurnoAsync(t.Id);
        return Result<TurnoDto>.Success(await BuildTurnoDtoAsync(t));
    }

    // ── Helpers privados ──────────────────────────────────────────────────────

    private async Task<Result<TurnoDto>> CambiarEstadoDesdeConfirmadoAsync(int id, EstadoTurno nuevo)
    {
        var t = await _db.Turnos.FirstOrDefaultAsync(x => x.Id == id);
        if (t is null) return NotFound();
        if (t.Estado != EstadoTurno.Confirmado)
            return TransicionInvalida();

        t.Estado        = nuevo;
        t.ActualizadoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Result<TurnoDto>.Success(await BuildTurnoDtoAsync(t));
    }

    // Validar cupón — devuelve (Pct, PrecioFinal) o Failure
    private record CuponAplicado(decimal? Pct, decimal PrecioFinal);

    private async Task<Result<CuponAplicado>> ValidarCuponAsync(
        int cuponId, int? clienteId, int servicioId, decimal precioBase)
    {
        var cupon = await _db.Cupones.FirstOrDefaultAsync(c => c.Id == cuponId && c.Activo);
        if (cupon is null)
            return Result<CuponAplicado>.Failure("CUPON_NO_ENCONTRADO", "Cupón no encontrado.");

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        if (hoy < cupon.FechaDesde || hoy > cupon.FechaHasta)
            return Result<CuponAplicado>.Failure("CUPON_EXPIRADO", "El cupón no está vigente.");

        if (cupon.UsosMaximos.HasValue && cupon.UsosActuales >= cupon.UsosMaximos.Value)
            return Result<CuponAplicado>.Failure("CUPON_AGOTADO", "El cupón ha alcanzado el máximo de usos.");

        if (cupon.UnUsoPorCliente && clienteId.HasValue)
        {
            var yaUsado = await _db.CuponUsos.AnyAsync(u => u.CuponId == cuponId && u.ClienteId == clienteId.Value);
            if (yaUsado)
                return Result<CuponAplicado>.Failure("CUPON_YA_USADO", "Ya usaste este cupón.");
        }

        if (cupon.ServiciosIds is { Length: > 0 } && !cupon.ServiciosIds.Contains(servicioId))
            return Result<CuponAplicado>.Failure("CUPON_NO_APLICA", "El cupón no aplica a este servicio.");

        decimal precioFinal;
        decimal? pct = null;
        if (cupon.TipoDescuento == TipoDescuento.Porcentaje)
        {
            pct         = cupon.Valor;
            precioFinal = Math.Round(precioBase * (1 - cupon.Valor / 100m), 2);
        }
        else
        {
            precioFinal = Math.Max(0, precioBase - cupon.Valor);
        }

        return Result<CuponAplicado>.Success(new CuponAplicado(pct, precioFinal));
    }

    private static bool PuedeVerTurno(int? clienteIdTurno, int operarioIdTurno, int? usuarioId, string? rol)
    {
        if (rol == "Admin") return true;
        if (rol == "Operario" && operarioIdTurno == usuarioId) return true;
        if (rol == "Cliente" && clienteIdTurno == usuarioId) return true;
        return false;
    }

    private static bool EsAdminOOperario(string? rol) => rol == "Admin" || rol == "Operario";

    private async Task<string?> GetRolAsync(int? userId)
    {
        if (!userId.HasValue) return null;
        var u = await _db.Usuarios.FirstOrDefaultAsync(x => x.Id == userId.Value);
        return u?.Rol.ToString();
    }

    private static Result<TurnoDto> NotFound() =>
        Result<TurnoDto>.Failure("TURNO_NO_ENCONTRADO", "Turno no encontrado.");

    private static Result<TurnoDto> TransicionInvalida() =>
        Result<TurnoDto>.Failure("TRANSICION_INVALIDA", "Transición de estado no permitida desde el estado actual.");

    private async Task<TurnoDto> BuildTurnoDtoAsync(Turno t)
    {
        var sub      = await _db.Subservicios.FirstOrDefaultAsync(s => s.Id == t.SubservicioId);
        var svc      = sub is not null ? await _db.Servicios.FirstOrDefaultAsync(s => s.Id == sub.ServicioId) : null;
        var variante = t.VarianteId.HasValue
            ? await _db.VariantesSubservicio.FirstOrDefaultAsync(v => v.Id == t.VarianteId.Value)
            : null;
        var operario   = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == t.OperarioId);
        var cliente    = t.ClienteId.HasValue ? await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == t.ClienteId.Value) : null;
        var metodoPago = t.MetodoPagoId.HasValue ? await _db.MetodosPago.FirstOrDefaultAsync(m => m.Id == t.MetodoPagoId.Value) : null;

        return new TurnoDto
        {
            Id                  = t.Id,
            Salon               = t.Salon.ToString(),
            ClienteId           = t.ClienteId,
            NombreCliente       = cliente is not null ? $"{cliente.Nombre} {cliente.Apellido}" : null,
            NombreAnonimo       = t.NombreAnonimo,
            TelefonoAnonimo     = t.TelefonoAnonimo,
            OperarioId          = t.OperarioId,
            NombreOperario      = operario is not null ? $"{operario.Nombre} {operario.Apellido}" : string.Empty,
            SubservicioId       = t.SubservicioId,
            NombreSubservicio   = sub?.Nombre ?? string.Empty,
            NombreServicio      = svc?.Nombre ?? string.Empty,
            VarianteId          = t.VarianteId,
            NombreVariante      = variante?.Nombre,
            SesionId            = t.SesionId,
            FechaHoraInicio     = t.FechaHoraInicio,
            DuracionMin         = t.DuracionMin,
            Estado              = t.Estado.ToString(),
            MotivoRechazo       = t.MotivoRechazo,
            PrecioBase          = t.PrecioBase,
            PorcentajeDescuento = t.PorcentajeDescuento,
            CuponId             = t.CuponId,
            PrecioFinal         = t.PrecioFinal,
            MetodoPagoId        = t.MetodoPagoId,
            NombreMetodoPago    = metodoPago?.Nombre,
            ComisionCalculada   = t.ComisionCalculada,
            Notas               = t.Notas,
            CreadoEn            = t.CreadoEn,
            ActualizadoEn       = t.ActualizadoEn
        };
    }

    private async Task<SesionDto> BuildSesionDtoAsync(Sesion s, List<Turno> turnos)
    {
        var operario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == s.OperarioId);
        var cliente  = s.ClienteId.HasValue ? await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == s.ClienteId.Value) : null;

        var dtos = new List<TurnoDto>();
        foreach (var t in turnos)
            dtos.Add(await BuildTurnoDtoAsync(t));

        return new SesionDto
        {
            Id               = s.Id,
            ClienteId        = s.ClienteId,
            NombreCliente    = cliente is not null ? $"{cliente.Nombre} {cliente.Apellido}" : null,
            NombreAnonimo    = s.NombreAnonimo,
            TelefonoAnonimo  = s.TelefonoAnonimo,
            OperarioId       = s.OperarioId,
            NombreOperario   = operario is not null ? $"{operario.Nombre} {operario.Apellido}" : string.Empty,
            Salon            = s.Salon.ToString(),
            FechaHoraInicio  = s.FechaHoraInicio,
            Estado           = s.Estado.ToString(),
            DescuentoAutoPct = s.DescuentoAutoPct,
            Turnos           = dtos,
            CreadoEn         = s.CreadoEn
        };
    }
}
