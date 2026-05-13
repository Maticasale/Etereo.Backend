using System.Security.Claims;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Auth;
using Etereo.Shared.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    // POST /api/v1/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest req)
    {
        var result = await _auth.RegisterAsync(req);
        return result.IsSuccess
            ? StatusCode(201, new { data = result.Value })
            : Error(result);
    }

    // POST /api/v1/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var result = await _auth.LoginAsync(req);
        return result.IsSuccess
            ? Ok(new { data = result.Value })
            : Error(result);
    }

    // POST /api/v1/auth/google
    [HttpPost("google")]
    public async Task<IActionResult> Google([FromBody] GoogleAuthRequest req)
    {
        var result = await _auth.GoogleAuthAsync(req);
        return result.IsSuccess
            ? Ok(new { data = result.Value })
            : Error(result);
    }

    // POST /api/v1/auth/refresh
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest req)
    {
        var result = await _auth.RefreshAsync(req);
        return result.IsSuccess
            ? Ok(new { data = result.Value })
            : Error(result);
    }

    // POST /api/v1/auth/logout
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest req)
    {
        await _auth.LogoutAsync(req.RefreshToken);
        return Ok(new { data = "ok" }); // siempre 200
    }

    // GET /api/v1/auth/me
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me()
    {
        var userId = GetUserId();
        var result = await _auth.GetMeAsync(userId);
        return result.IsSuccess
            ? Ok(new { data = result.Value })
            : Error(result);
    }

    // POST /api/v1/auth/cambiar-password
    [HttpPost("cambiar-password")]
    [Authorize]
    public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordRequest req)
    {
        var userId = GetUserId();
        var result = await _auth.CambiarPasswordAsync(userId, req);
        return result.IsSuccess
            ? Ok(new { data = "ok" })
            : Error(result);
    }

    // POST /api/v1/auth/forgot-password
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        await _auth.ForgotPasswordAsync(req);
        return Ok(new { data = "ok" }); // siempre 200 por seguridad
    }

    // POST /api/v1/auth/reset-password
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        var result = await _auth.ResetPasswordAsync(req);
        return result.IsSuccess
            ? Ok(new { data = "ok" })
            : Error(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private int GetUserId()
    {
        // Con MapInboundClaims = false, el claim sub llega con el nombre original "sub"
        var sub = User.FindFirstValue("sub");
        return int.TryParse(sub, out var id) ? id : 0;
    }

    private IActionResult Error<T>(Result<T> result)
    {
        var status = result.ErrorCode switch
        {
            "CREDENCIALES_EN_USO"        => 409,
            "CREDENCIALES_INVALIDAS"     => 401,
            "CUENTA_BLOQUEADA"           => 403,
            "TOKEN_INVALIDO_O_EXPIRADO"  => 401,
            "TOKEN_GOOGLE_INVALIDO"      => 400,
            "USAR_GOOGLE_AUTH"           => 400,
            "SIN_PASSWORD_LOCAL"         => 400,
            "PASSWORD_ACTUAL_INVALIDA"   => 400,
            "USUARIO_NO_ENCONTRADO"      => 404,
            _                            => 400
        };

        return StatusCode(status, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
    }
}
