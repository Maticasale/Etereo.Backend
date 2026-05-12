using Etereo.Domain.Base;
using Etereo.Domain.Enums;

namespace Etereo.Domain.Entities.Auth;

public class DisponibilidadSalon : EntityBase
{
    public DateOnly Fecha { get; set; }
    public Salon Salon { get; set; }
    public int MotivoId { get; set; }
    public string? Descripcion { get; set; }
    public int CreadoPorId { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
