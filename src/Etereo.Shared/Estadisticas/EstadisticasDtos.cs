namespace Etereo.Shared.Estadisticas;

// ── Resumen general ───────────────────────────────────────────────────────────

public class ResumenEstadisticasDto
{
    // Turnos
    public int TurnosHoy     { get; set; }
    public int TurnosSemana  { get; set; }
    public int TurnosMes     { get; set; }

    // Financiero
    public decimal IngresosHoy     { get; set; }
    public decimal IngresosSemana  { get; set; }
    public decimal IngresosMes     { get; set; }
    public decimal EgresosHoy      { get; set; }
    public decimal EgresosSemana   { get; set; }
    public decimal EgresosMes      { get; set; }
    public decimal BalanceHoy      => IngresosHoy    - EgresosHoy;
    public decimal BalanceSemana   => IngresosSemana - EgresosSemana;
    public decimal BalanceMes      => IngresosMes    - EgresosMes;

    // Calificaciones
    public double PromedioCalificacionGlobal { get; set; }
    public int    TotalCalificaciones         { get; set; }

    // Distribución de estados (hoy)
    public List<DistribucionEstadoDto> TurnosPorEstado { get; set; } = [];
}

public class DistribucionEstadoDto
{
    public string  Estado     { get; set; } = string.Empty;
    public int     Cantidad   { get; set; }
    public double  Porcentaje { get; set; }
}

// ── Evolución de ingresos/egresos ─────────────────────────────────────────────

public class PuntoEvolucionDto
{
    public string  Periodo  { get; set; } = string.Empty;   // "2026-05-13" o "2026-05" etc.
    public decimal Ingresos { get; set; }
    public decimal Egresos  { get; set; }
    public decimal Balance  => Ingresos - Egresos;
}

// ── Ranking servicios ─────────────────────────────────────────────────────────

public class ServicioRankingDto
{
    public int     SubservicioId   { get; set; }
    public string  NombreServicio  { get; set; } = string.Empty;
    public string  NombreCategoria { get; set; } = string.Empty;
    public int     CantidadTurnos  { get; set; }
    public decimal IngresoTotal    { get; set; }
}

// ── Estadísticas por operaria ─────────────────────────────────────────────────

public class OperariaEstadisticasDto
{
    public int     OperarioId          { get; set; }
    public string  Nombre              { get; set; } = string.Empty;
    public int     TurnosMes           { get; set; }
    public int     TurnosRealizados    { get; set; }
    public decimal IngresosMes         { get; set; }
    public decimal ComisionesMes       { get; set; }
    public double  PromedioCalificacion { get; set; }
    public int     TotalCalificaciones  { get; set; }
}

// ── Ocupación diaria ──────────────────────────────────────────────────────────

public class OcupacionDiariaDto
{
    public DateOnly Fecha            { get; set; }
    public int      TotalTurnos      { get; set; }
    public int      TurnosRealizados { get; set; }
    public int      TurnosCancelados { get; set; }
    public int      TurnosPendientes { get; set; }
}
