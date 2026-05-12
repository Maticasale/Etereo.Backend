using Etereo.Domain.Base;

namespace Etereo.Domain.Entities.Emails;

public class Calificacion : EntityBase
{
    public int TurnoId { get; set; }
    public int ClienteId { get; set; }
    public int OperarioId { get; set; }
    public int Puntuacion { get; set; }
    public string? Comentario { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
