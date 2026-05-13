using Etereo.Application.Common;
using Etereo.Shared.Cupones;

namespace Etereo.Application.Interfaces.Cupones;

public interface ICuponesService
{
    Task<Result<List<CuponDto>>> ListarAsync();
    Task<Result<CuponDto>>       CrearAsync(CrearCuponRequest req);
    Task<Result<CuponDto>>       ActualizarAsync(int id, ActualizarCuponRequest req);
    Task<Result<CuponDto>>       CambiarEstadoAsync(int id, EstadoCuponRequest req);
    Task<Result<List<CuponDto>>> DisponiblesAsync(int clienteId);
    Task<Result<CuponDto>>       ValidarAsync(string codigo, int clienteId);
}
