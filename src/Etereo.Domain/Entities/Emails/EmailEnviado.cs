using Etereo.Domain.Base;
using Etereo.Domain.Enums;

namespace Etereo.Domain.Entities.Emails;

public class EmailEnviado : EntityBase
{
    public TipoEmail Tipo { get; set; }
    public string Destinatario { get; set; } = string.Empty;
    public int? TurnoId { get; set; }
    public int? UsuarioId { get; set; }
    public EstadoEmail Estado { get; set; }
    public string? ErrorDetalle { get; set; }
    public DateTime EnviadoEn { get; set; } = DateTime.UtcNow;
}
