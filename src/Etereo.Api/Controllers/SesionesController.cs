using System.Security.Claims;
using Etereo.Api.Attributes;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Turnos;
using Etereo.Shared.Turnos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/sesiones")]
public class SesionesController : ControllerBase
{
    private readonly ITurnosService _svc;

    public SesionesController(ITurnosService svc) => _svc = svc;

    // POST /api/v1/sesiones — Anónimo | [Authorize]
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Crear([FromBody] CrearSesionRequest req)
    {
        var userId = ObtenerUserId();
        var result = await _svc.CrearSesionAsync(req, userId);
        return result.IsSuccess ? StatusCode(201, new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/sesiones/{id} — Admin | Operario propio | Cliente propio
    [HttpGet("{id:int}")]
    [RequiereRol("Admin", "Operario", "Cliente")]
    public async Task<IActionResult> Obtener(int id)
    {
        var userId = ObtenerUserId();
        var rol    = User.FindFirstValue("rol");
        var result = await _svc.ObtenerSesionAsync(id, userId, rol);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private int? ObtenerUserId()
    {
        var sub = User.FindFirstValue("sub");
        return sub is not null && int.TryParse(sub, out var id) ? id : null;
    }

    private IActionResult Error<T>(Result<T> result)
    {
        var status = result.ErrorCode switch
        {
            "SESION_NO_ENCONTRADA"    => 404,
            "OPERARIO_NO_ENCONTRADO"  => 404,
            "CLIENTE_NO_ENCONTRADO"   => 404,
            "SUBSERVICIO_NO_ENCONTRADO" => 404,
            "SIN_PERMISO"             => 403,
            "SALON_INVALIDO"          => 422,
            _                         => 400
        };
        return StatusCode(status, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
    }
}
