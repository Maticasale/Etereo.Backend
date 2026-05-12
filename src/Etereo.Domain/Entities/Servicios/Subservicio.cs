using Etereo.Domain.Base;
using Etereo.Domain.Enums;

namespace Etereo.Domain.Entities.Servicios;

public class Subservicio : EntityBase
{
    public int ServicioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public decimal? Precio { get; set; }
    public int? DuracionMin { get; set; }
    public bool RequiereSilencio { get; set; } = false;
    public bool EsPack { get; set; } = false;
    public string? DetallePack { get; set; }
    public SexoSubservicio Sexo { get; set; } = SexoSubservicio.Ambos;
    public bool Activo { get; set; } = true;
    public int OrdenDisplay { get; set; } = 0;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
