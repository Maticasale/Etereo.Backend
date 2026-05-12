using Etereo.Domain.Base;

namespace Etereo.Domain.Entities.Turnos;

public class ReglaDescuentoSesion : EntityBase
{
    public int ServicioId { get; set; }
    public int ZonasMinimas { get; set; } = 3;
    public decimal PorcentajeDescuento { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
}
