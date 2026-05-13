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

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static int ISOWeek(DateOnly d)
        => System.Globalization.ISOWeek.GetWeekOfYear(d.ToDateTime(TimeOnly.MinValue));

    private static int ISOWeekYear(DateOnly d)
        => System.Globalization.ISOWeek.GetYear(d.ToDateTime(TimeOnly.MinValue));
}
