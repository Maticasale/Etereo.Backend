using Etereo.Api.Attributes;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Emails;
using Etereo.Shared.Emails;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/emails")]
public class EmailsController : ControllerBase
{
    private readonly IEmailsService _svc;

    public EmailsController(IEmailsService svc) => _svc = svc;

    // GET /api/v1/emails/configuracion
    [HttpGet("configuracion")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> ObtenerConfiguracion()
    {
        var result = await _svc.ObtenerConfiguracionAsync();
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // PUT /api/v1/emails/configuracion
    [HttpPut("configuracion")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> ActualizarConfiguracion([FromBody] ActualizarConfiguracionEmailRequest req)
    {
        var result = await _svc.ActualizarConfiguracionAsync(req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/emails/historial
    [HttpGet("historial")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Historial(
        [FromQuery] string? tipo,
        [FromQuery] string? estado,
        [FromQuery] DateOnly? fechaDesde,
        [FromQuery] DateOnly? fechaHasta)
    {
        var result = await _svc.ListarHistorialAsync(tipo, estado, fechaDesde, fechaHasta);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/emails/campana
    [HttpPost("campana")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> EnviarCampana([FromBody] EnviarCampanaRequest req)
    {
        var result = await _svc.EnviarCampanaAsync(req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    private IActionResult Error<T>(Result<T> result)
    {
        var status = result.ErrorCode == "CONFIG_NO_ENCONTRADA" ? 404 : 400;
        return StatusCode(status, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
    }
}
