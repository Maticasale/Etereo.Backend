namespace Etereo.Shared.Emails;

// ── Configuración ─────────────────────────────────────────────────────────────

public class ConfiguracionEmailDto
{
    public int  Id                     { get; set; }
    public int  RecordatorioDiasAntes  { get; set; }
    public int  PostturnoHorasDespues  { get; set; }
    public bool EmailsActivos          { get; set; }
    public DateTime ActualizadoEn      { get; set; }
}

public record ActualizarConfiguracionEmailRequest(
    int?  RecordatorioDiasAntes,
    int?  PostturnoHorasDespues,
    bool? EmailsActivos
);

// ── Historial ─────────────────────────────────────────────────────────────────

public class EmailEnviadoDto
{
    public int      Id           { get; set; }
    public string   Tipo         { get; set; } = string.Empty;
    public string   Destinatario { get; set; } = string.Empty;
    public int?     TurnoId      { get; set; }
    public int?     UsuarioId    { get; set; }
    public string   Estado       { get; set; } = string.Empty;
    public string?  ErrorDetalle { get; set; }
    public DateTime EnviadoEn    { get; set; }
}

// ── Calificaciones ────────────────────────────────────────────────────────────

public record CrearCalificacionRequest(int TurnoId, int Puntuacion, string? Comentario);

public class CalificacionDto
{
    public int      Id              { get; set; }
    public int      TurnoId         { get; set; }
    public int      ClienteId       { get; set; }
    public string   NombreCliente   { get; set; } = string.Empty;
    public int      OperarioId      { get; set; }
    public string   NombreOperario  { get; set; } = string.Empty;
    public int      Puntuacion      { get; set; }
    public string?  Comentario      { get; set; }
    public DateTime CreadoEn        { get; set; }
}

public class PromedioCalificacionDto
{
    public int     OperarioId          { get; set; }
    public string  NombreOperario      { get; set; } = string.Empty;
    public double  Promedio            { get; set; }
    public int     TotalCalificaciones { get; set; }
}

// ── Campaña ───────────────────────────────────────────────────────────────────

public record EnviarCampanaRequest(string[] Emails, string Asunto, string Contenido);
