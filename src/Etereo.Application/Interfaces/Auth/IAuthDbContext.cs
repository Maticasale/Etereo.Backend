using Etereo.Domain.Entities.Auth;

namespace Etereo.Application.Interfaces.Auth;

public interface IAuthDbContext
{
    IQueryable<Usuario> Usuarios { get; }
    IQueryable<RefreshToken> RefreshTokens { get; }
    IQueryable<PasswordResetToken> PasswordResetTokens { get; }
    IQueryable<DisponibilidadSalon> DisponibilidadesSalon { get; }
    IQueryable<DisponibilidadOperario> DisponibilidadesOperario { get; }
    IQueryable<OperarioVistas> OperarioVistas { get; }

    void AddUsuario(Usuario usuario);
    void AddRefreshToken(RefreshToken token);
    void RemoveRefreshToken(RefreshToken token);
    void AddPasswordResetToken(PasswordResetToken token);
    void AddDisponibilidadSalon(DisponibilidadSalon d);
    void RemoveDisponibilidadSalon(DisponibilidadSalon d);
    void AddDisponibilidadOperario(DisponibilidadOperario d);
    void AddOperarioVistas(OperarioVistas v);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
