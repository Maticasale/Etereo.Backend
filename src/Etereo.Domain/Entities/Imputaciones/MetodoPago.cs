using Etereo.Domain.Base;

namespace Etereo.Domain.Entities.Imputaciones;

public class MetodoPago : EntityBase
{
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public int OrdenDisplay { get; set; } = 0;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
