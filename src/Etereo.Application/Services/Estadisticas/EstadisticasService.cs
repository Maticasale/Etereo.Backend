using Etereo.Application.Common;
using Etereo.Application.Interfaces.Estadisticas;
using Etereo.Domain.Enums;
using Etereo.Shared.Estadisticas;
using Microsoft.EntityFrameworkCore;

namespace Etereo.Application.Services.Estadisticas;

public class EstadisticasService : IEstadisticasService
{
    private readonly IEstadisticasDbContext _db;

    public EstadisticasService(IEstadisticasDbContext db) => _db = db;

    // ── Resumen general ───────────────────────────────────────────────────────

    public async Task<Result<ResumenEstadisticasDto>> ResumenAsync()
    {
        var ahora    = DateTime.UtcNow;
        var hoy      = DateOnly.FromDateTime(ahora);
        var lunesUtc = hoy.AddDays(-(int)ahora.DayOfWeek == 0 ? 6 : (int)ahora.DayOfWeek - 1);
        var primeroMes = new DateOnly(hoy.Year, hoy.Month, 1);

        var turnos      = await _db.Turnos.ToListAsync();
        var imputaciones = await _db.Imputaciones.ToListAsync();
        var cals        = await _db.Calificaciones.ToListAsync();

        // Turnos por período (solo realizados como "activos")
        var estadosActivos = new[] { EstadoTurno.Confirmado, EstadoTurno.PendienteConfirmacion, EstadoTurno.Realizado };

        int turnosHoy    = turnos.Count(t => DateOnly.FromDateTime(t.FechaHoraInicio) == hoy && estadosActivos.Contains(t.Estado));
        int turnosSemana = turnos.Count(t => DateOnly.FromDateTime(t.FechaHoraInicio) >= lunesUtc && DateOnly.FromDateTime(t.FechaHoraInicio) <= hoy && estadosActivos.Contains(t.Estado));
        int turnosMes    = turnos.Count(t => DateOnly.FromDateTime(t.FechaHoraInicio) >= primeroMes && estadosActivos.Contains(t.Estado));

        // Ingresos/egresos por período
        var ingrHoy    = imputaciones.Where(i => i.Tipo == TipoImputacion.Ingreso && i.Fecha == hoy).Sum(i => i.Monto);
        var ingrSem    = imputaciones.Where(i => i.Tipo == TipoImputacion.Ingreso && i.Fecha >= lunesUtc && i.Fecha <= hoy).Sum(i => i.Monto);
        var ingrMes    = imputaciones.Where(i => i.Tipo == TipoImputacion.Ingreso && i.Fecha >= primeroMes).Sum(i => i.Monto);
        var egrHoy     = imputaciones.Where(i => i.Tipo == TipoImputacion.Egreso  && i.Fecha == hoy).Sum(i => i.Monto);
        var egrSem     = imputaciones.Where(i => i.Tipo == TipoImputacion.Egreso  && i.Fecha >= lunesUtc && i.Fecha <= hoy).Sum(i => i.Monto);
        var egrMes     = imputaciones.Where(i => i.Tipo == TipoImputacion.Egreso  && i.Fecha >= primeroMes).Sum(i => i.Monto);

        // Distribución de estados (todo el mes)
        var turnosMesAll = turnos.Where(t => DateOnly.FromDateTime(t.FechaHoraInicio) >= primeroMes).ToList();
        int totalMes     = turnosMesAll.Count == 0 ? 1 : turnosMesAll.Count;

        var distribucion = turnosMesAll
            .GroupBy(t => t.Estado)
            .Select(g => new DistribucionEstadoDto
            {
                Estado     = g.Key.ToString(),
                Cantidad   = g.Count(),
                Porcentaje = Math.Round((double)g.Count() / totalMes * 100, 1)
            })
            .OrderByDescending(d => d.Cantidad)
            .ToList();

        return Result<ResumenEstadisticasDto>.Success(new ResumenEstadisticasDto
        {
            TurnosHoy    = turnosHoy,
            TurnosSemana = turnosSemana,
            TurnosMes    = turnosMes,
            IngresosHoy    = ingrHoy,
            IngresosSemana = ingrSem,
            IngresosMes    = ingrMes,
            EgresosHoy     = egrHoy,
            EgresosSemana  = egrSem,
            EgresosMes     = egrMes,
            PromedioCalificacionGlobal = cals.Count > 0 ? Math.Round(cals.Average(c => c.Puntuacion), 2) : 0,
            TotalCalificaciones        = cals.Count,
            TurnosPorEstado            = distribucion
        });
    }

    // ── Evolución ─────────────────────────────────────────────────────────────

    public async Task<Result<List<PuntoEvolucionDto>>> EvolucionAsync(
        DateOnly fechaDesde, DateOnly fechaHasta, string agrupacion)
    {
        agrupacion = agrupacion.ToLower();
        if (agrupacion is not ("dia" or "semana" or "mes"))
            return Result<List<PuntoEvolucionDto>>.Failure("AGRUPACION_INVALIDA",
                "Agrupación inválida. Use: dia, semana o mes.");

        var imps = await _db.Imputaciones
            .Where(i => i.Fecha >= fechaDesde && i.Fecha <= fechaHasta)
            .ToListAsync();

        var puntos = agrupacion switch
        {
            "dia" => imps
                .GroupBy(i => i.Fecha.ToString("yyyy-MM-dd"))
                .Select(g => new PuntoEvolucionDto
                {
                    Periodo  = g.Key,
                    Ingresos = g.Where(i => i.Tipo == TipoImputacion.Ingreso).Sum(i => i.Monto),
                    Egresos  = g.Where(i => i.Tipo == TipoImputacion.Egreso).Sum(i => i.Monto)
                }),
            "semana" => imps
                .GroupBy(i => $"{ISOWeekYear(i.Fecha)}-W{ISOWeek(i.Fecha):D2}")
                .Select(g => new PuntoEvolucionDto
                {
                    Periodo  = g.Key,
                    Ingresos = g.Where(i => i.Tipo == TipoImputacion.Ingreso).Sum(i => i.Monto),
                    Egresos  = g.Where(i => i.Tipo == TipoImputacion.Egreso).Sum(i => i.Monto)
                }),
            _ => imps
                .GroupBy(i => i.Fecha.ToString("yyyy-MM"))
                .Select(g => new PuntoEvolucionDto
                {
                    Periodo  = g.Key,
                    Ingresos = g.Where(i => i.Tipo == TipoImputacion.Ingreso).Sum(i => i.Monto),
                    Egresos  = g.Where(i => i.Tipo == TipoImputacion.Egreso).Sum(i => i.Monto)
                })
        };

        return Result<List<PuntoEvolucionDto>>.Success(puntos.OrderBy(p => p.Periodo).ToList());
    }

    // ── Ranking servicios ─────────────────────────────────────────────────────

    public async Task<Result<List<ServicioRankingDto>>> RankingServiciosAsync(
        DateOnly? fechaDesde, DateOnly? fechaHasta)
    {
        var query = _db.Turnos
            .Where(t => t.Estado == EstadoTurno.Realizado);

        if (fechaDesde.HasValue)
            query = query.Where(t => DateOnly.FromDateTime(t.FechaHoraInicio) >= fechaDesde.Value);
        if (fechaHasta.HasValue)
            query = query.Where(t => DateOnly.FromDateTime(t.FechaHoraInicio) <= fechaHasta.Value);

        var turnos      = await query.ToListAsync();
        var subservicios = await _db.Subservicios.ToListAsync();
        var servicios   = await _db.Servicios.ToListAsync();

        var ranking = turnos
            .GroupBy(t => t.SubservicioId)
            .Select(g =>
            {
                var sub = subservicios.FirstOrDefault(s => s.Id == g.Key);
                var svc = sub is not null ? servicios.FirstOrDefault(s => s.Id == sub.ServicioId) : null;
                return new ServicioRankingDto
                {
                    SubservicioId   = g.Key,
                    NombreServicio  = sub is not null ? $"{svc?.Nombre} — {sub.Nombre}" : $"Subservicio #{g.Key}",
                    NombreCategoria = svc?.Nombre ?? string.Empty,
                    CantidadTurnos  = g.Count(),
                    IngresoTotal    = g.Sum(t => t.PrecioFinal ?? 0)
                };
            })
            .OrderByDescending(r => r.CantidadTurnos)
            .ToList();

        return Result<List<ServicioRankingDto>>.Success(ranking);
    }

    // ── Estadísticas por operaria ─────────────────────────────────────────────

    public async Task<Result<List<OperariaEstadisticasDto>>> EstadisticasOperariasAsync(
        DateOnly? fechaDesde, DateOnly? fechaHasta)
    {
        var hoy        = DateOnly.FromDateTime(DateTime.UtcNow);
        var desde      = fechaDesde ?? new DateOnly(hoy.Year, hoy.Month, 1);
        var hasta      = fechaHasta ?? hoy;

        var turnos      = await _db.Turnos
            .Where(t => DateOnly.FromDateTime(t.FechaHoraInicio) >= desde
                     && DateOnly.FromDateTime(t.FechaHoraInicio) <= hasta
                     && t.OperarioId != null)
            .ToListAsync();

        var imputaciones = await _db.Imputaciones
            .Where(i => i.Fecha >= desde && i.Fecha <= hasta && i.OperarioId != null)
            .ToListAsync();

        var cals      = await _db.Calificaciones.ToListAsync();
        var operarios = await _db.Usuarios
            .Where(u => u.Rol == Rol.Operario)
            .ToListAsync();

        var stats = operarios.Select(op =>
        {
            var tOp     = turnos.Where(t => t.OperarioId == op.Id).ToList();
            var iOp     = imputaciones.Where(i => i.OperarioId == op.Id).ToList();
            var cOp     = cals.Where(c => c.OperarioId == op.Id).ToList();

            return new OperariaEstadisticasDto
            {
                OperarioId           = op.Id,
                Nombre               = $"{op.Nombre} {op.Apellido}",
                TurnosMes            = tOp.Count,
                TurnosRealizados     = tOp.Count(t => t.Estado == EstadoTurno.Realizado),
                IngresosMes          = iOp.Where(i => i.Tipo == TipoImputacion.Ingreso).Sum(i => i.Monto),
                ComisionesMes        = iOp.Where(i => i.Tipo == TipoImputacion.Egreso).Sum(i => i.Monto),
                PromedioCalificacion = cOp.Count > 0 ? Math.Round(cOp.Average(c => c.Puntuacion), 2) : 0,
                TotalCalificaciones  = cOp.Count
            };
        })
        .OrderByDescending(s => s.TurnosMes)
        .ToList();

        return Result<List<OperariaEstadisticasDto>>.Success(stats);
    }

    // ── Ocupación diaria ──────────────────────────────────────────────────────

    public async Task<Result<List<OcupacionDiariaDto>>> OcupacionAsync(
        DateOnly fechaDesde, DateOnly fechaHasta)
    {
        var turnos = await _db.Turnos
            .Where(t => DateOnly.FromDateTime(t.FechaHoraInicio) >= fechaDesde
                     && DateOnly.FromDateTime(t.FechaHoraInicio) <= fechaHasta)
            .ToListAsync();

        var ocupacion = turnos
            .GroupBy(t => DateOnly.FromDateTime(t.FechaHoraInicio))
            .Select(g => new OcupacionDiariaDto
            {
                Fecha            = g.Key,
                TotalTurnos      = g.Count(),
                TurnosRealizados = g.Count(t => t.Estado == EstadoTurno.Realizado),
                TurnosCancelados = g.Count(t => t.Estado is EstadoTurno.Cancelado or EstadoTurno.Rechazado),
                TurnosPendientes = g.Count(t => t.Estado is EstadoTurno.PendienteConfirmacion or EstadoTurno.Confirmado)
            })
            .OrderBy(o => o.Fecha)
            .ToList();

        return Result<List<OcupacionDiariaDto>>.Success(ocupacion);
    }

    // ── Ingresos / Egresos (alias de Evolución) ───────────────────────────────

    public async Task<Result<List<PuntoEvolucionDto>>> IngresosEgresosAsync(
        DateOnly? fechaDesde, DateOnly? fechaHasta, string agrupacion)
    {
        var hoy   = DateOnly.FromDateTime(DateTime.UtcNow);
        var desde = fechaDesde ?? new DateOnly(hoy.Year, hoy.Month, 1);
        var hasta = fechaHasta ?? hoy;
        return await EvolucionAsync(desde, hasta, agrupacion);
    }

    // ── Estadísticas de turnos ────────────────────────────────────────────────

    public async Task<Result<TurnosEstadisticasDto>> TurnosAsync(
        DateOnly? fechaDesde, DateOnly? fechaHasta)
    {
        var hoy   = DateOnly.FromDateTime(DateTime.UtcNow);
        var desde = fechaDesde ?? new DateOnly(hoy.Year, hoy.Month, 1);
        var hasta = fechaHasta ?? hoy;

        var turnos = await _db.Turnos
            .Where(t => DateOnly.FromDateTime(t.FechaHoraInicio) >= desde
                     && DateOnly.FromDateTime(t.FechaHoraInicio) <= hasta)
            .ToListAsync();

        int total    = turnos.Count == 0 ? 1 : turnos.Count;
        var porEstado = turnos
            .GroupBy(t => t.Estado)
            .Select(g => new DistribucionEstadoDto
            {
                Estado     = g.Key.ToString(),
                Cantidad   = g.Count(),
                Porcentaje = Math.Round((double)g.Count() / total * 100, 1)
            })
            .OrderByDescending(d => d.Cantidad)
            .ToList();

        return Result<TurnosEstadisticasDto>.Success(new TurnosEstadisticasDto
        {
            Total     = turnos.Count,
            PorEstado = porEstado
        });
    }

    // ── Estadísticas de calificaciones ───────────────────────────────────────

    public async Task<Result<CalificacionesEstadisticasDto>> CalificacionesAsync(
        DateOnly? fechaDesde, DateOnly? fechaHasta)
    {
        var cals = await _db.Calificaciones.ToListAsync();
        if (fechaDesde.HasValue)
            cals = cals.Where(c => DateOnly.FromDateTime(c.CreadoEn) >= fechaDesde.Value).ToList();
        if (fechaHasta.HasValue)
            cals = cals.Where(c => DateOnly.FromDateTime(c.CreadoEn) <= fechaHasta.Value).ToList();

        var operarios = await _db.Usuarios
            .Where(u => u.Rol == Rol.Operario)
            .ToListAsync();

        var porOperario = operarios
            .Select(op =>
            {
                var c = cals.Where(x => x.OperarioId == op.Id).ToList();
                return new PromedioOperarioCalDto
                {
                    OperarioId = op.Id,
                    Nombre     = $"{op.Nombre} {op.Apellido}",
                    Promedio   = c.Count > 0 ? Math.Round(c.Average(x => x.Puntuacion), 2) : 0,
                    Total      = c.Count
                };
            })
            .Where(x => x.Total > 0)
            .OrderByDescending(x => x.Promedio)
            .ToList();

        return Result<CalificacionesEstadisticasDto>.Success(new CalificacionesEstadisticasDto
        {
            PromedioGlobal = cals.Count > 0 ? Math.Round(cals.Average(c => c.Puntuacion), 2) : 0,
            Total          = cals.Count,
            PorOperario    = porOperario
        });
    }

    // ── Dashboard KPIs ────────────────────────────────────────────────────────

    public async Task<Result<ResumenEstadisticasDto>> KpisAsync() => await ResumenAsync();

    // ── Dashboard Alertas ─────────────────────────────────────────────────────

    public async Task<Result<List<AlertaDashboardDto>>> AlertasAsync()
    {
        var ahora    = DateTime.UtcNow;
        var hoy      = DateOnly.FromDateTime(ahora);
        var manana   = hoy.AddDays(1);
        var primeroMes = new DateOnly(hoy.Year, hoy.Month, 1);

        var turnos      = await _db.Turnos.ToListAsync();
        var imputaciones = await _db.Imputaciones.ToListAsync();

        var alertas = new List<AlertaDashboardDto>();

        // Turnos sin confirmar para hoy/mañana
        var sinConfirmar = turnos.Count(t =>
            t.Estado == EstadoTurno.PendienteConfirmacion &&
            (DateOnly.FromDateTime(t.FechaHoraInicio) == hoy ||
             DateOnly.FromDateTime(t.FechaHoraInicio) == manana));
        if (sinConfirmar > 0)
            alertas.Add(new AlertaDashboardDto
            {
                Tipo      = "TURNOS_SIN_CONFIRMAR",
                Mensaje   = $"{sinConfirmar} turno(s) pendiente(s) de confirmación para hoy/mañana.",
                Prioridad = sinConfirmar >= 3 ? "Alta" : "Normal",
                Cantidad  = sinConfirmar
            });

        // Balance del mes negativo
        var ingrMes = imputaciones.Where(i => i.Tipo == TipoImputacion.Ingreso && i.Fecha >= primeroMes).Sum(i => i.Monto);
        var egrMes  = imputaciones.Where(i => i.Tipo == TipoImputacion.Egreso  && i.Fecha >= primeroMes).Sum(i => i.Monto);
        if (ingrMes - egrMes < 0)
            alertas.Add(new AlertaDashboardDto
            {
                Tipo      = "BALANCE_MES_NEGATIVO",
                Mensaje   = $"El balance del mes es negativo: ${ingrMes - egrMes:F2}.",
                Prioridad = "Alta"
            });

        // Turnos en PendienteConfirmacion de días anteriores (vencidos)
        var vencidos = turnos.Count(t =>
            t.Estado == EstadoTurno.PendienteConfirmacion &&
            DateOnly.FromDateTime(t.FechaHoraInicio) < hoy);
        if (vencidos > 0)
            alertas.Add(new AlertaDashboardDto
            {
                Tipo      = "TURNOS_VENCIDOS_SIN_RESOLVER",
                Mensaje   = $"{vencidos} turno(s) de días anteriores aún en estado Pendiente.",
                Prioridad = "Baja",
                Cantidad  = vencidos
            });

        return Result<List<AlertaDashboardDto>>.Success(alertas);
    }

    // ── Dashboard Agenda Hoy ──────────────────────────────────────────────────

    private static readonly TimeSpan ArgOffset = TimeSpan.FromHours(3);

    public async Task<Result<List<AgendaHoyItemDto>>> AgendaHoyAsync(int? operarioId)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = _db.Turnos
            .Where(t => DateOnly.FromDateTime(t.FechaHoraInicio) == hoy);

        if (operarioId.HasValue)
            query = query.Where(t => t.OperarioId == operarioId.Value);

        var turnos       = await query.OrderBy(t => t.FechaHoraInicio).ToListAsync();
        var usuarios     = await _db.Usuarios.ToListAsync();
        var subservicios = await _db.Subservicios.ToListAsync();
        var servicios    = await _db.Servicios.ToListAsync();

        var agenda = turnos.Select(t =>
        {
            var cliente = t.ClienteId.HasValue
                ? usuarios.FirstOrDefault(u => u.Id == t.ClienteId.Value)
                : null;
            var nombreCliente = cliente is not null
                ? $"{cliente.Nombre} {cliente.Apellido}"
                : t.NombreAnonimo ?? "Anónimo";

            var operario = usuarios.FirstOrDefault(u => u.Id == t.OperarioId);
            var sub      = subservicios.FirstOrDefault(s => s.Id == t.SubservicioId);
            var svc      = sub is not null ? servicios.FirstOrDefault(s => s.Id == sub.ServicioId) : null;

            var inicio = t.FechaHoraInicio - ArgOffset;
            var fin    = inicio.AddMinutes(t.DuracionMin);

            return new AgendaHoyItemDto
            {
                TurnoId    = t.Id,
                HoraInicio = inicio.ToString("HH:mm"),
                HoraFin    = fin.ToString("HH:mm"),
                Cliente    = nombreCliente,
                Operario   = operario is not null ? $"{operario.Nombre} {operario.Apellido}" : $"Operario #{t.OperarioId}",
                Servicio   = sub is not null ? $"{svc?.Nombre} — {sub.Nombre}" : $"Subservicio #{t.SubservicioId}",
                Estado     = t.Estado.ToString(),
                PrecioFinal = t.PrecioFinal
            };
        }).ToList();

        return Result<List<AgendaHoyItemDto>>.Success(agenda);
    }

    // ── Comisiones ────────────────────────────────────────────────────────────

    public async Task<Result<List<ComisionDto>>> ListarComisionesAsync(
        DateOnly? fechaDesde, DateOnly? fechaHasta, int? operarioId)
    {
        var query = _db.Imputaciones
            .Where(i => i.Tipo == TipoImputacion.Egreso && i.OperarioId != null);

        if (fechaDesde.HasValue)
            query = query.Where(i => i.Fecha >= fechaDesde.Value);
        if (fechaHasta.HasValue)
            query = query.Where(i => i.Fecha <= fechaHasta.Value);
        if (operarioId.HasValue)
            query = query.Where(i => i.OperarioId == operarioId.Value);

        var imputaciones = await query.OrderByDescending(i => i.Fecha).ToListAsync();
        var operarios    = await _db.Usuarios.Where(u => u.Rol == Rol.Operario).ToListAsync();

        var comisiones = imputaciones.Select(i =>
        {
            var op = operarios.FirstOrDefault(u => u.Id == i.OperarioId);
            return new ComisionDto
            {
                Id             = i.Id,
                OperarioId     = i.OperarioId!.Value,
                NombreOperario = op is not null ? $"{op.Nombre} {op.Apellido}" : $"Operario #{i.OperarioId}",
                TurnoId        = i.TurnoId,
                Monto          = i.Monto,
                Fecha          = i.Fecha,
                Concepto       = i.Descripcion ?? string.Empty
            };
        }).ToList();

        return Result<List<ComisionDto>>.Success(comisiones);
    }

    public async Task<Result<ResumenComisionesDto>> ResumenComisionesAsync(
        int operarioId, DateOnly? fechaDesde, DateOnly? fechaHasta)
    {
        var operario = await _db.Usuarios
            .Where(u => u.Id == operarioId && u.Rol == Rol.Operario)
            .FirstOrDefaultAsync();

        if (operario is null)
            return Result<ResumenComisionesDto>.Failure("OPERARIO_NO_ENCONTRADO", "Operario no encontrado.");

        var result = await ListarComisionesAsync(fechaDesde, fechaHasta, operarioId);
        var lista  = result.Value ?? [];

        return Result<ResumenComisionesDto>.Success(new ResumenComisionesDto
        {
            OperarioId      = operarioId,
            Nombre          = $"{operario.Nombre} {operario.Apellido}",
            TotalComisiones = lista.Sum(c => c.Monto),
            Comisiones      = lista
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static int ISOWeek(DateOnly d)
        => System.Globalization.ISOWeek.GetWeekOfYear(d.ToDateTime(TimeOnly.MinValue));

    private static int ISOWeekYear(DateOnly d)
        => System.Globalization.ISOWeek.GetYear(d.ToDateTime(TimeOnly.MinValue));
}
