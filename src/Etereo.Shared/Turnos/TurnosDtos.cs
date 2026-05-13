namespace Etereo.Shared.Turnos;

// ── Sesiones ──────────────────────────────────────────────────────────────────

public record CrearTurnoEnSesionRequest(
    int  SubservicioId,
    int? VarianteId
);

public record CrearSesionRequest(
    int?   ClienteId,
    string? NombreAnonimo,
    string? TelefonoAnonimo,
    int    OperarioId,
    string Salon,
    DateTime FechaHoraInicio,
    List<CrearTurnoEnSesionRequest> Zonas
);

public class SesionDto
{
    public int       Id                { get; set; }
    public int?      ClienteId         { get; set; }
    public string?   NombreCliente     { get; set; }
    public string?   NombreAnonimo     { get; set; }
    public string?   TelefonoAnonimo   { get; set; }
    public int       OperarioId        { get; set; }
    public string    NombreOperario    { get; set; } = string.Empty;
    public string    Salon             { get; set; } = string.Empty;
    public DateTime  FechaHoraInicio   { get; set; }
    public string    Estado            { get; set; } = string.Empty;
    public decimal?  DescuentoAutoPct  { get; set; }
    public List<TurnoDto> Turnos       { get; set; } = [];
    public DateTime  CreadoEn         { get; set; }
}

// ── Turnos ────────────────────────────────────────────────────────────────────

public record CrearTurnoRequest(
    int?    ClienteId,
    string? NombreAnonimo,
    string? TelefonoAnonimo,
    int     OperarioId,
    int     SubservicioId,
    int?    VarianteId,
    DateTime FechaHoraInicio,
    string? Notas,
    int?    CuponId
);

public class TurnoDto
{
    public int      Id                  { get; set; }
    public string   Salon               { get; set; } = string.Empty;
    public int?     ClienteId           { get; set; }
    public string?  NombreCliente       { get; set; }
    public string?  NombreAnonimo       { get; set; }
    public string?  TelefonoAnonimo     { get; set; }
    public int      OperarioId          { get; set; }
    public string   NombreOperario      { get; set; } = string.Empty;
    public int      SubservicioId       { get; set; }
    public string   NombreSubservicio   { get; set; } = string.Empty;
    public string   NombreServicio      { get; set; } = string.Empty;
    public int?     VarianteId          { get; set; }
    public string?  NombreVariante      { get; set; }
    public int?     SesionId            { get; set; }
    public DateTime FechaHoraInicio     { get; set; }
    public int      DuracionMin         { get; set; }
    public string   Estado              { get; set; } = string.Empty;
    public string?  MotivoRechazo       { get; set; }
    public decimal  PrecioBase          { get; set; }
    public decimal? PorcentajeDescuento { get; set; }
    public int?     CuponId             { get; set; }
    public decimal? PrecioFinal         { get; set; }
    public int?     MetodoPagoId        { get; set; }
    public string?  NombreMetodoPago    { get; set; }
    public decimal? ComisionCalculada   { get; set; }
    public string?  Notas               { get; set; }
    public DateTime CreadoEn            { get; set; }
    public DateTime ActualizadoEn       { get; set; }
}

// ── Cambios de estado ─────────────────────────────────────────────────────────

public record RechazarTurnoRequest(string MotivoRechazo);

public record RealizarTurnoRequest(int MetodoPagoId, decimal PrecioFinal);

// ── Disponibilidad ────────────────────────────────────────────────────────────

public class SlotOcupadoDto
{
    public DateTime Inicio { get; set; }
    public DateTime Fin    { get; set; }
    public string   Estado { get; set; } = string.Empty;
}

public class DisponibilidadDto
{
    public bool               Disponible         { get; set; }
    public string?            MotivoNoDisponible { get; set; }
    public List<SlotOcupadoDto> SlotsOcupados    { get; set; } = [];
    public List<DateTime>     HorariosDisponibles { get; set; } = [];
}
