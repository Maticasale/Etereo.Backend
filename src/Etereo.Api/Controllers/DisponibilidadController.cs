using System.Security.Claims;
using Etereo.Api.Attributes;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Operarios;
using Etereo.Shared.Operarios;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/disponibilidad")]
public class DisponibilidadController : ControllerBase
{
    private readonly IOperariosService _svc;

    public DisponibilidadController(IOperariosService svc) => _svc = svc;

    // GET /api/v1/disponibilidad/salon
    [HttpGet("salon")]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> ListarSalon()
    {
        var result = await _svc.ListarDisponibilidadSalonAsync();
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/disponibilidad/salon
    [HttpPost("salon")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> CrearSalon([FromBody] CrearDisponibilidadSalonRequest req)
    {
        var userId = int.Parse(User.FindFirstValue("sub") ?? "0");
        var result = await _svc.CrearDisponibilidadSalonAsync(req, userId);
        return result.IsSuccess ? StatusCode(201, new { data = result.Value }) : Error(result);
    }

    // DELETE /api/v1/disponibilidad/salon/{id}
    [HttpDelete("salon/{id:int}")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> EliminarSalon(int id)
    {
        var result = await _svc.EliminarDisponibilidadSalonAsync(id);
        return result.IsSuccess ? NoContent() : Error(result);
    }

    // GET /api/v1/disponibilidad/operario/{id}
    [HttpGet("operario/{id:int}")]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> ListarOperario(int id)
    {
        var result = await _svc.ListarDisponibilidadOperarioAsync(id);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/disponibilidad/operario
    [HttpPost("operario")]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> CrearOperario([FromBody] CrearDisponibilidadOperarioRequest req)
    {
        var result = await _svc.CrearDisponibilidadOperarioAsync(req);
        return result.IsSuccess ? StatusCode(201, new { data = result.Value }) : Error(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IActionResult Error<T>(Result<T> result)
    {
        var status = result.ErrorCode switch
        {
            "OPERARIO_NO_ENCONTRADO"      => 404,
            "DISPONIBILIDAD_NO_ENCONTRADA" => 404,
            "MOTIVO_NO_ENCONTRADO"        => 404,
            "SALON_INVALIDO"              => 422,
            _                             => 400
        };
        return StatusCode(status, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
    }
}
