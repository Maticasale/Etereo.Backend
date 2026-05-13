using Etereo.Application.Common;
using Etereo.Shared.Auth;
using Etereo.Shared.Operarios;

namespace Etereo.Application.Interfaces.Operarios;

public interface IOperariosService
{
    // Operarios
    Task<Result<List<UsuarioDto>>>           ListarAsync();
    Task<Result<UsuarioDto>>                 ObtenerAsync(int id);

    // Subservicios del operario
    Task<Result<List<OperarioSubservicioDto>>> ListarSubserviciosAsync(int operarioId);
    Task<Result<OperarioSubservicioDto>>       AsignarSubservicioAsync(int operarioId, AsignarSubservicioRequest req);
    Task<Result<OperarioSubservicioDto>>       ActualizarComisionAsync(int operarioId, int subservicioId, ActualizarComisionRequest req);
    Task<Result<bool>>                         QuitarSubservicioAsync(int operarioId, int subservicioId);

    // Vistas
    Task<Result<OperarioVistasDto>>  ObtenerVistasAsync(int operarioId);
    Task<Result<OperarioVistasDto>>  ActualizarVistasAsync(int operarioId, ActualizarVistasRequest req);

    // Disponibilidad salón
    Task<Result<List<DisponibilidadSalonDto>>> ListarDisponibilidadSalonAsync();
    Task<Result<DisponibilidadSalonDto>>       CrearDisponibilidadSalonAsync(CrearDisponibilidadSalonRequest req, int creadoPorId);
    Task<Result<bool>>                         EliminarDisponibilidadSalonAsync(int id);

    // Disponibilidad operario
    Task<Result<List<DisponibilidadOperarioDto>>> ListarDisponibilidadOperarioAsync(int operarioId);
    Task<Result<DisponibilidadOperarioDto>>       CrearDisponibilidadOperarioAsync(CrearDisponibilidadOperarioRequest req);
}
