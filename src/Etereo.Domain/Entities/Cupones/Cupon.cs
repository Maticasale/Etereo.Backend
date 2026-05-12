using Etereo.Domain.Base;
using Etereo.Domain.Enums;

namespace Etereo.Domain.Entities.Cupones;

public class Cupon : EntityBase
{
    public string Codigo { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public TipoDescuento TipoDescuento { get; set; }
    public decimal Valor { get; set; }
    public int[]? ServiciosIds { get; set; }
    public DateOnly FechaDesde { get; set; }
    public DateOnly FechaHasta { get; set; }
    public int? UsosMaximos { get; set; }
    public int UsosActuales { get; set; } = 0;
    public bool UnUsoPorCliente { get; set; } = true;
    public bool Activo { get; set; } = true;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
