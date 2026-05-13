namespace Etereo.Shared.Auth;

// ── Requests ──────────────────────────────────────────────────────────────────

public record LoginRequest(string Email, string Password);

public record RegisterRequest(
    string Email,
    string Password,
    string Nombre,
    string Apellido,
    string? Telefono,
    string? Sexo);

public record GoogleAuthRequest(string IdToken);

public record RefreshRequest(string RefreshToken);

public record CambiarPasswordRequest(string PasswordActual, string PasswordNueva);

public record ForgotPasswordRequest(string Email);

public record ResetPasswordRequest(string Token, string PasswordNueva);

// ── Response models ───────────────────────────────────────────────────────────

public class UsuarioDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string Sexo { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? MotivoBloqueo { get; set; }
    public bool DebeCambiarPassword { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreadoEn { get; set; }
}

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UsuarioDto Usuario { get; set; } = null!;
}
