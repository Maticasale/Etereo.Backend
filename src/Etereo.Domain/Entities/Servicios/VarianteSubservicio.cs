using Etereo.Domain.Base;
using Etereo.Domain.Enums;

namespace Etereo.Domain.Entities.Servicios;

public class VarianteSubservicio : EntityBase
{
    public int SubservicioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int DuracionMin { get; set; }
    public SexoSubservicio Sexo { get; set; } = SexoSubservicio.Ambos;
    public bool Activo { get; set; } = true;
    public int OrdenDisplay { get; set; } = 0;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
