using Etereo.Domain.Base;

namespace Etereo.Domain.Entities.Auth;

public class DisponibilidadOperario : EntityBase
{
    public int OperarioId { get; set; }
    public DateOnly Fecha { get; set; }
    public bool Trabaja { get; set; } = true;
    public string? MotivoAusencia { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
