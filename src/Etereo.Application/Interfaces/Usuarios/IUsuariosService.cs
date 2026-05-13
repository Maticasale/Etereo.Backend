using Etereo.Application.Common;
using Etereo.Shared.Auth;
using Etereo.Shared.Usuarios;

namespace Etereo.Application.Interfaces.Usuarios;

public interface IUsuariosService
{
    Task<Result<UsuariosListResponse>>  ListarAsync();
    Task<Result<UsuarioDto>>            ObtenerAsync(int id);
    Task<Result<UsuarioDto>>            ActualizarAsync(int id, ActualizarUsuarioRequest req);
    Task<Result<bool>>                  BloquearAsync(int id, BloquearUsuarioRequest req);
    Task<Result<bool>>                  DesbloquearAsync(int id);
    Task<Result<UsuarioDto>>            PromoverOperarioAsync(int id);
    Task<Result<UsuarioDto>>            DegradarClienteAsync(int id);
    Task<Result<UsuarioDto>>            CrearClienteAsync(CrearClienteRequest req);
    Task<Result<List<UsuarioDto>>>      BuscarClientesAsync(string? q);
}
