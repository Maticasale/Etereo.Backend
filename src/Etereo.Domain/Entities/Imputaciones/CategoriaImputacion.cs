using Etereo.Domain.Base;
using Etereo.Domain.Enums;

namespace Etereo.Domain.Entities.Imputaciones;

public class CategoriaImputacion : EntityBase
{
    public string Nombre { get; set; } = string.Empty;
    public TipoCategoriaImp Tipo { get; set; }
    public string? Descripcion { get; set; }
    public bool Activo { get; set; } = true;
    public int OrdenDisplay { get; set; } = 0;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
