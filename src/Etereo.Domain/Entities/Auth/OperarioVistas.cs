using Etereo.Domain.Base;

namespace Etereo.Domain.Entities.Auth;

public class OperarioVistas : EntityBase
{
    public int OperarioId { get; set; }
    public bool VerMisTurnos { get; set; } = true;
    public bool VerMisComisiones { get; set; } = true;
    public bool VerMiCalificacion { get; set; } = false;
    public bool VerMisEstadisticas { get; set; } = false;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
}
