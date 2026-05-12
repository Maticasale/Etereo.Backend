using Etereo.Domain.Base;
using Etereo.Domain.Enums;

namespace Etereo.Domain.Entities.Turnos;

public class Turno : EntityBase
{
    public Salon Salon { get; set; }
    public int? ClienteId { get; set; }
    public string? NombreAnonimo { get; set; }
    public string? TelefonoAnonimo { get; set; }
    public int OperarioId { get; set; }
    public int SubservicioId { get; set; }
    public int? VarianteId { get; set; }
    public int? SesionId { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public int DuracionMin { get; set; }
    public EstadoTurno Estado { get; set; } = EstadoTurno.PendienteConfirmacion;
    public string? MotivoRechazo { get; set; }
    public decimal PrecioBase { get; set; }
    public decimal? PorcentajeDescuento { get; set; }
    public int? CuponId { get; set; }
    public decimal? PrecioFinal { get; set; }
    public int? MetodoPagoId { get; set; }
    public decimal? ComisionCalculada { get; set; }
    public string? Notas { get; set; }
    public string? IpOrigen { get; set; }
    public int? CreadoPorId { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
}
