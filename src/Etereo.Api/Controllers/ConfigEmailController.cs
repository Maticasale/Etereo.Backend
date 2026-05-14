using Etereo.Api.Attributes;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Emails;
using Etereo.Shared.Emails;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/config/email")]
public class ConfigEmailController : ControllerBase
{
    private readonly IEmailsService _svc;

    public ConfigEmailController(IEmailsService svc) => _svc = svc;

    // GET /api/v1/config/email
    [HttpGet]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Obtener()
    {
        var result = await _svc.ObtenerConfiguracionAsync();
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // PUT /api/v1/config/email
    [HttpPut]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Actualizar([FromBody] ActualizarConfiguracionEmailRequest req)
    {
        var result = await _svc.ActualizarConfiguracionAsync(req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    private IActionResult Error<T>(Result<T> result)
    {
        var status = result.ErrorCode == "CONFIG_NO_ENCONTRADA" ? 404 : 400;
        return StatusCode(status, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
    }
}
