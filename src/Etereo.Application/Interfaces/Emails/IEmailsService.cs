using Etereo.Application.Common;
using Etereo.Shared.Emails;

namespace Etereo.Application.Interfaces.Emails;

public interface IEmailsService
{
    // Configuración
    Task<Result<ConfiguracionEmailDto>> ObtenerConfiguracionAsync();
    Task<Result<ConfiguracionEmailDto>> ActualizarConfiguracionAsync(ActualizarConfiguracionEmailRequest req);

    // Historial
    Task<Result<List<EmailEnviadoDto>>> ListarHistorialAsync(string? tipo, string? estado, DateOnly? fechaDesde, DateOnly? fechaHasta);

    // Calificaciones
    Task<Result<CalificacionDto>>             CrearCalificacionAsync(CrearCalificacionRequest req, int clienteId);
    Task<Result<List<CalificacionDto>>>       ListarCalificacionesAsync(int? operarioId);
    Task<Result<PromedioCalificacionDto>>     PromedioOperarioAsync(int operarioId);

    // Campaña
    Task<Result<bool>> EnviarCampanaAsync(EnviarCampanaRequest req);

    // Notificaciones internas (llamadas desde TurnosService)
    Task NotificarConfirmacionAsync(int turnoId);
    Task NotificarRechazoAsync(int turnoId, string motivo);
    Task NotificarCancelacionAsync(int turnoId);
    Task NotificarPostTurnoAsync(int turnoId);
}
