using Etereo.Domain.Base;
using Etereo.Domain.Enums;

namespace Etereo.Domain.Entities.Imputaciones;

public class Imputacion : EntityBase
{
    public DateOnly Fecha { get; set; }
    public TipoImputacion Tipo { get; set; }
    public int CategoriaId { get; set; }
    public string? Descripcion { get; set; }
    public decimal Monto { get; set; }
    public int? TurnoId { get; set; }
    public int? OperarioId { get; set; }
    public int CargadoPorId { get; set; }
    public OrigenImputacion Origen { get; set; } = OrigenImputacion.Manual;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
