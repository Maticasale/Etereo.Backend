using Etereo.Domain.Base;
using Etereo.Domain.Enums;

namespace Etereo.Domain.Entities.Servicios;

public class Servicio : EntityBase
{
    public string Nombre { get; set; } = string.Empty;
    public Salon Salon { get; set; }
    public int? CategoriaImputacionId { get; set; }
    public bool Activo { get; set; } = true;
    public int OrdenDisplay { get; set; } = 0;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
