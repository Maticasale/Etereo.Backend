namespace Etereo.Shared.Operarios;

// ── Subservicios del operario ──────────────────────────────────────────────────

public record AsignarSubservicioRequest(int SubservicioId, decimal PorcentajeComision);

public record ActualizarComisionRequest(decimal PorcentajeComision);

public class OperarioSubservicioDto
{
    public int     Id                  { get; set; }
    public int     OperarioId          { get; set; }
    public int     SubservicioId       { get; set; }
    public string  NombreSubservicio   { get; set; } = string.Empty;
    public string  NombreServicio      { get; set; } = string.Empty;
    public decimal PorcentajeComision  { get; set; }
}

// ── Vistas del operario ────────────────────────────────────────────────────────

public record ActualizarVistasRequest(
    bool VerMisTurnos,
    bool VerMisComisiones,
    bool VerMiCalificacion,
    bool VerMisEstadisticas
);

public class OperarioVistasDto
{
    public int  Id                  { get; set; }
    public int  OperarioId          { get; set; }
    public bool VerMisTurnos        { get; set; }
    public bool VerMisComisiones    { get; set; }
    public bool VerMiCalificacion   { get; set; }
    public bool VerMisEstadisticas  { get; set; }
}

// ── Disponibilidad Salón ───────────────────────────────────────────────────────

public record CrearDisponibilidadSalonRequest(
    DateOnly Fecha,
    string   Salon,
    int      MotivoId,
    string?  Descripcion
);

public class DisponibilidadSalonDto
{
    public int      Id           { get; set; }
    public DateOnly Fecha        { get; set; }
    public string   Salon        { get; set; } = string.Empty;
    public int      MotivoId     { get; set; }
    public string   NombreMotivo { get; set; } = string.Empty;
    public string?  Descripcion  { get; set; }
    public int      CreadoPorId  { get; set; }
}

// ── Disponibilidad Operario ────────────────────────────────────────────────────

public record CrearDisponibilidadOperarioRequest(
    int      OperarioId,
    DateOnly Fecha,
    bool     Trabaja,
    string?  MotivoAusencia
);

public class DisponibilidadOperarioDto
{
    public int      Id              { get; set; }
    public int      OperarioId      { get; set; }
    public DateOnly Fecha           { get; set; }
    public bool     Trabaja         { get; set; }
    public string?  MotivoAusencia  { get; set; }
}
