using Etereo.Domain.Entities.Auth;

namespace Etereo.Application.Interfaces.Usuarios;

public interface IUsuariosDbContext
{
    IQueryable<Usuario> Usuarios { get; }

    void AddUsuario(Usuario usuario);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
