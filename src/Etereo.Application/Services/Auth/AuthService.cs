using Etereo.Application.Common;
using Etereo.Application.Interfaces.Auth;
using Etereo.Application.Interfaces.Email;
using Etereo.Domain.Entities.Auth;
using Etereo.Domain.Enums;
using Etereo.Shared.Auth;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Etereo.Application.Services.Auth;

public class AuthService : IAuthService
{
    private readonly IAuthDbContext  _db;
    private readonly IJwtService     _jwt;
    private readonly IEmailService   _email;
    private readonly IConfiguration  _config;

    public AuthService(IAuthDbContext db, IJwtService jwt, IEmailService email, IConfiguration config)
    {
        _db     = db;
        _jwt    = jwt;
        _email  = email;
        _config = config;
    }

    // ── Register ──────────────────────────────────────────────────────────────

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest req)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        if (await _db.Usuarios.AnyAsync(u => u.Email == email))
            return Result<AuthResponse>.Failure("CREDENCIALES_EN_USO", "El email ya está registrado.");

        var sexo = req.Sexo != null && Enum.TryParse<Sexo>(req.Sexo, true, out var s)
            ? s
            : Sexo.NoEspecifica;

        var usuario = new Usuario
        {
            Email         = email,
            PasswordHash  = BCrypt.Net.BCrypt.HashPassword(req.Password, workFactor: 12),
            Nombre        = req.Nombre.Trim(),
            Apellido      = req.Apellido.Trim(),
            Telefono      = req.Telefono?.Trim(),
            Sexo          = sexo,
            Rol           = Rol.Cliente,
            AuthProvider  = AuthProvider.Local,
            Estado        = EstadoUsuario.Activo,
            CreadoEn      = DateTime.UtcNow,
            ActualizadoEn = DateTime.UtcNow
        };

        _db.AddUsuario(usuario);
        await _db.SaveChangesAsync(); // necesitamos el Id antes de crear el refresh token

        return Result<AuthResponse>.Success(await BuildAuthResponseAsync(usuario));
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest req)
    {
        var email   = req.Email.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        if (usuario is null || usuario.PasswordHash is null)
            return Result<AuthResponse>.Failure("CREDENCIALES_INVALIDAS", "Email o contraseña incorrectos.");

        if (usuario.AuthProvider == AuthProvider.Google)
            return Result<AuthResponse>.Failure("USAR_GOOGLE_AUTH", "Esta cuenta usa Google para iniciar sesión.");

        if (!BCrypt.Net.BCrypt.Verify(req.Password, usuario.PasswordHash))
            return Result<AuthResponse>.Failure("CREDENCIALES_INVALIDAS", "Email o contraseña incorrectos.");

        if (usuario.Estado == EstadoUsuario.Bloqueado)
            return Result<AuthResponse>.Failure("CUENTA_BLOQUEADA", $"Tu cuenta está bloqueada. {usuario.MotivoBloqueo}");

        return Result<AuthResponse>.Success(await BuildAuthResponseAsync(usuario));
    }

    // ── Google OAuth ──────────────────────────────────────────────────────────

    public async Task<Result<AuthResponse>> GoogleAuthAsync(GoogleAuthRequest req)
    {
        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_config["GOOGLE_CLIENT_ID"]!]
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(req.IdToken, settings);
        }
        catch
        {
            return Result<AuthResponse>.Failure("TOKEN_GOOGLE_INVALIDO", "Token de Google inválido o expirado.");
        }

        // Buscar primero por GoogleId, luego por email
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.GoogleId == payload.Subject)
                   ?? await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == payload.Email);

        if (usuario is null)
        {
            usuario = new Usuario
            {
                Email         = payload.Email,
                Nombre        = payload.GivenName ?? payload.Name ?? "Usuario",
                Apellido      = payload.FamilyName ?? string.Empty,
                GoogleId      = payload.Subject,
                AvatarUrl     = payload.Picture,
                Rol           = Rol.Cliente,
                AuthProvider  = AuthProvider.Google,
                Estado        = EstadoUsuario.Activo,
                CreadoEn      = DateTime.UtcNow,
                ActualizadoEn = DateTime.UtcNow
            };
            _db.AddUsuario(usuario);
        }
        else
        {
            // Vincular GoogleId si todavía no estaba vinculado
            if (usuario.GoogleId != payload.Subject)
                usuario.GoogleId = payload.Subject;
            if (usuario.AvatarUrl != payload.Picture)
                usuario.AvatarUrl = payload.Picture;
            usuario.ActualizadoEn = DateTime.UtcNow;
        }

        if (usuario.Estado == EstadoUsuario.Bloqueado)
            return Result<AuthResponse>.Failure("CUENTA_BLOQUEADA", $"Tu cuenta está bloqueada. {usuario.MotivoBloqueo}");

        await _db.SaveChangesAsync(); // guarda usuario (nuevo o actualizado) y obtiene Id
        return Result<AuthResponse>.Success(await BuildAuthResponseAsync(usuario));
    }

    // ── Refresh ───────────────────────────────────────────────────────────────

    public async Task<Result<AuthResponse>> RefreshAsync(RefreshRequest req)
    {
        var tokenHash    = _jwt.HashToken(req.RefreshToken);
        var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (refreshToken is null || refreshToken.Revocado || refreshToken.ExpiraEn < DateTime.UtcNow)
            return Result<AuthResponse>.Failure("TOKEN_INVALIDO_O_EXPIRADO", "Refresh token inválido o expirado.");

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == refreshToken.UsuarioId);

        if (usuario is null || usuario.Estado == EstadoUsuario.Bloqueado)
            return Result<AuthResponse>.Failure("TOKEN_INVALIDO_O_EXPIRADO", "Token inválido.");

        // Rotación obligatoria: revocar el token actual
        refreshToken.Revocado = true;

        // BuildAuthResponseAsync guardará la revocación y el nuevo token en un solo SaveChanges
        return Result<AuthResponse>.Success(await BuildAuthResponseAsync(usuario));
    }

    // ── Logout ────────────────────────────────────────────────────────────────

    public async Task<Result<bool>> LogoutAsync(string refreshToken)
    {
        var tokenHash = _jwt.HashToken(refreshToken);
        var token     = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (token is not null)
        {
            token.Revocado = true;
            await _db.SaveChangesAsync();
        }

        return Result<bool>.Success(true); // siempre 200
    }

    // ── Me ────────────────────────────────────────────────────────────────────

    public async Task<Result<UsuarioDto>> GetMeAsync(int userId)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == userId);

        if (usuario is null)
            return Result<UsuarioDto>.Failure("USUARIO_NO_ENCONTRADO", "Usuario no encontrado.");

        return Result<UsuarioDto>.Success(ToDto(usuario));
    }

    // ── Cambiar password ──────────────────────────────────────────────────────

    public async Task<Result<bool>> CambiarPasswordAsync(int userId, CambiarPasswordRequest req)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == userId);

        if (usuario is null)
            return Result<bool>.Failure("USUARIO_NO_ENCONTRADO", "Usuario no encontrado.");

        if (usuario.AuthProvider == AuthProvider.Google || usuario.PasswordHash is null)
            return Result<bool>.Failure("SIN_PASSWORD_LOCAL", "Esta cuenta usa Google para iniciar sesión. No tiene contraseña local.");

        if (!BCrypt.Net.BCrypt.Verify(req.PasswordActual, usuario.PasswordHash))
            return Result<bool>.Failure("PASSWORD_ACTUAL_INVALIDA", "La contraseña actual es incorrecta.");

        usuario.PasswordHash        = BCrypt.Net.BCrypt.HashPassword(req.PasswordNueva, workFactor: 12);
        usuario.DebeCambiarPassword = false;
        usuario.ActualizadoEn       = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    // ── Forgot password ───────────────────────────────────────────────────────

    public async Task<Result<bool>> ForgotPasswordAsync(ForgotPasswordRequest req)
    {
        var email   = req.Email.Trim().ToLowerInvariant();
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

        // Siempre 200 — no revelamos si el email existe o no
        if (usuario is null || usuario.AuthProvider == AuthProvider.Google || usuario.Estado == EstadoUsuario.Bloqueado)
            return Result<bool>.Success(true);

        var (rawToken, tokenHash) = _jwt.GenerateSecureToken();

        _db.AddPasswordResetToken(new PasswordResetToken
        {
            UsuarioId = usuario.Id,
            TokenHash = tokenHash,
            ExpiraEn  = DateTime.UtcNow.AddHours(1),
            Usado     = false,
            CreadoEn  = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        // Construir el link usando el primer origin de CORS como base del frontend
        var frontendUrl = _config["CORS_ALLOWED_ORIGINS"]
            ?.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()
            ?.Trim()
            ?? "http://localhost:5173";

        var resetLink = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(rawToken)}";

        // Fire-and-forget — si falla el email, el endpoint igual devuelve 200
        _ = _email.SendPasswordResetEmailAsync(usuario.Email, usuario.Nombre, resetLink);

        return Result<bool>.Success(true);
    }

    // ── Reset password ────────────────────────────────────────────────────────

    public async Task<Result<bool>> ResetPasswordAsync(ResetPasswordRequest req)
    {
        var tokenHash  = _jwt.HashToken(req.Token);
        var resetToken = await _db.PasswordResetTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (resetToken is null || resetToken.Usado || resetToken.ExpiraEn < DateTime.UtcNow)
            return Result<bool>.Failure("TOKEN_INVALIDO_O_EXPIRADO", "El token de recuperación es inválido o ya expiró.");

        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == resetToken.UsuarioId);

        if (usuario is null)
            return Result<bool>.Failure("TOKEN_INVALIDO_O_EXPIRADO", "Token inválido.");

        usuario.PasswordHash        = BCrypt.Net.BCrypt.HashPassword(req.PasswordNueva, workFactor: 12);
        usuario.DebeCambiarPassword = false;
        usuario.ActualizadoEn       = DateTime.UtcNow;
        resetToken.Usado            = true;

        await _db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<AuthResponse> BuildAuthResponseAsync(Usuario usuario)
    {
        var (rawRefresh, refreshHash) = _jwt.GenerateSecureToken();

        _db.AddRefreshToken(new RefreshToken
        {
            UsuarioId = usuario.Id,
            TokenHash = refreshHash,
            ExpiraEn  = DateTime.UtcNow.AddDays(30),
            CreadoEn  = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return new AuthResponse
        {
            AccessToken  = _jwt.GenerateAccessToken(usuario),
            RefreshToken = rawRefresh,
            Usuario      = ToDto(usuario)
        };
    }

    private static UsuarioDto ToDto(Usuario u) => new()
    {
        Id                  = u.Id,
        Email               = u.Email,
        Nombre              = u.Nombre,
        Apellido            = u.Apellido,
        Telefono            = u.Telefono,
        Rol                 = u.Rol.ToString(),
        Estado              = u.Estado.ToString(),
        MotivoBloqueo       = u.MotivoBloqueo,
        DebeCambiarPassword = u.DebeCambiarPassword,
        AvatarUrl           = u.AvatarUrl,
        CreadoEn            = u.CreadoEn
    };
}
