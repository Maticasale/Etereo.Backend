using System.Security.Claims;
using Etereo.Api.Attributes;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Emails;
using Etereo.Shared.Emails;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/calificaciones")]
public class CalificacionesController : ControllerBase
{
    private readonly IEmailsService _svc;

    public CalificacionesController(IEmailsService svc) => _svc = svc;

    // POST /api/v1/calificaciones
    [HttpPost]
    [RequiereRol("Cliente")]
    public async Task<IActionResult> Crear([FromBody] CrearCalificacionRequest req)
    {
        var sub = User.FindFirstValue("sub");
        if (sub is null || !int.TryParse(sub, out var clienteId))
            return Unauthorized();

        var result = await _svc.CrearCalificacionAsync(req, clienteId);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/calificaciones
    [HttpGet]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> Listar([FromQuery] int? operarioId)
    {
        var result = await _svc.ListarCalificacionesAsync(operarioId);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/calificaciones/promedio/{operarioId}
    [HttpGet("promedio/{operarioId:int}")]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> Promedio(int operarioId)
    {
        var result = await _svc.PromedioOperarioAsync(operarioId);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    private IActionResult Error<T>(Result<T> result)
    {
        var status = result.ErrorCode switch
        {
            "TURNO_NO_ENCONTRADO"  => 404,
            "OPERARIO_NO_ENCONTRADO" => 404,
            "SIN_PERMISO"          => 403,
            _                      => 400
        };
        return StatusCode(status, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
    }
}
