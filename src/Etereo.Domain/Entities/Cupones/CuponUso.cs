using Etereo.Domain.Base;

namespace Etereo.Domain.Entities.Cupones;

public class CuponUso : EntityBase
{
    public int CuponId { get; set; }
    public int ClienteId { get; set; }
    public int TurnoId { get; set; }
    public DateTime UsadoEn { get; set; } = DateTime.UtcNow;
}
