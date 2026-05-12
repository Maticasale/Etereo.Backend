using Etereo.Domain.Base;
using Etereo.Domain.Enums;

namespace Etereo.Domain.Entities.Turnos;

public class Sesion : EntityBase
{
    public int? ClienteId { get; set; }
    public string? NombreAnonimo { get; set; }
    public string? TelefonoAnonimo { get; set; }
    public int OperarioId { get; set; }
    public Salon Salon { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public EstadoTurno Estado { get; set; } = EstadoTurno.PendienteConfirmacion;
    public decimal? DescuentoAutoPct { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
