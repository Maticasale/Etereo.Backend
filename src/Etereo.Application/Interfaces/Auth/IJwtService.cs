using Etereo.Domain.Entities.Auth;

namespace Etereo.Application.Interfaces.Auth;

public interface IJwtService
{
    /// <summary>Genera el JWT access token (TTL 15 min).</summary>
    string GenerateAccessToken(Usuario usuario);

    /// <summary>Genera un token opaco seguro y su hash SHA-256. El raw va al cliente; el hash va a la DB.</summary>
    (string rawToken, string tokenHash) GenerateSecureToken();

    /// <summary>Hashea un token con SHA-256 para comparación contra la DB.</summary>
    string HashToken(string token);
}
