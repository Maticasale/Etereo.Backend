using Etereo.Domain.Base;

namespace Etereo.Domain.Entities.Emails;

public class ConfiguracionEmail : EntityBase
{
    public int RecordatorioDiasAntes { get; set; } = 1;
    public int PostturnoHorasDespues { get; set; } = 2;
    public bool EmailsActivos { get; set; } = true;
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
}
