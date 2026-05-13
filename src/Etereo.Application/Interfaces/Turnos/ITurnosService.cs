using Etereo.Application.Common;
using Etereo.Shared.Turnos;

namespace Etereo.Application.Interfaces.Turnos;

public interface ITurnosService
{
    // Sesiones
    Task<Result<SesionDto>> CrearSesionAsync(CrearSesionRequest req, int? creadoPorId);
    Task<Result<SesionDto>> ObtenerSesionAsync(int id, int? usuarioId, string? rol);

    // Turnos CRUD
    Task<Result<TurnoDto>>       CrearTurnoAsync(CrearTurnoRequest req, int? creadoPorId);
    Task<Result<List<TurnoDto>>> ListarTurnosAsync(DateOnly? fecha, int? operarioId, string? estado);
    Task<Result<TurnoDto>>       ObtenerTurnoAsync(int id, int? usuarioId, string? rol);
    Task<Result<List<TurnoDto>>> MisTurnosAsync(int clienteId);
    Task<Result<DisponibilidadDto>> ObtenerDisponibilidadAsync(DateOnly fecha, int operarioId, int duracionMin);

    // Transiciones de estado
    Task<Result<TurnoDto>> ConfirmarTurnoAsync(int id);
    Task<Result<TurnoDto>> RechazarTurnoAsync(int id, RechazarTurnoRequest req);
    Task<Result<TurnoDto>> CancelarTurnoAsync(int id, int? usuarioId, string? rol);
    Task<Result<TurnoDto>> MultaTurnoAsync(int id);
    Task<Result<TurnoDto>> AusenteTurnoAsync(int id);
    Task<Result<TurnoDto>> RealizarTurnoAsync(int id, RealizarTurnoRequest req);
    Task<Result<TurnoDto>> ImpagoTurnoAsync(int id);
    Task<Result<TurnoDto>> PublicidadTurnoAsync(int id);
}
