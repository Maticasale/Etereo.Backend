using Etereo.Application.Common;
using Etereo.Shared.Auth;

namespace Etereo.Application.Interfaces.Auth;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest req);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest req);
    Task<Result<AuthResponse>> GoogleAuthAsync(GoogleAuthRequest req);
    Task<Result<AuthResponse>> RefreshAsync(RefreshRequest req);
    Task<Result<bool>>         LogoutAsync(string refreshToken);
    Task<Result<UsuarioDto>>   GetMeAsync(int userId);
    Task<Result<bool>>         CambiarPasswordAsync(int userId, CambiarPasswordRequest req);
    Task<Result<bool>>         ForgotPasswordAsync(ForgotPasswordRequest req);
    Task<Result<bool>>         ResetPasswordAsync(ResetPasswordRequest req);
}
