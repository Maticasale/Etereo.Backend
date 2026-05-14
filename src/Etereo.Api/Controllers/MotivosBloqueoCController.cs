using Etereo.Api.Attributes;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Imputaciones;
using Etereo.Shared.Imputaciones;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/motivos-bloqueo-salon")]
public class MotivosBloqueoSalonController : ControllerBase
{
    private readonly IImputacionesService _svc;

    public MotivosBloqueoSalonController(IImputacionesService svc) => _svc = svc;

    // GET /api/v1/motivos-bloqueo-salon
    [HttpGet]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> Listar()
    {
        var result = await _svc.ListarMotivosBloqueoAsync();
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/motivos-bloqueo-salon
    [HttpPost]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Crear([FromBody] CrearMotivoBloqueoRequest req)
    {
        var result = await _svc.CrearMotivoBloqueoAsync(req);
        return result.IsSuccess ? StatusCode(201, new { data = result.Value }) : Error(result);
    }

    // PUT /api/v1/motivos-bloqueo-salon/{id}
    [HttpPut("{id:int}")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarMotivoBloqueoRequest req)
    {
        var result = await _svc.ActualizarMotivoBloqueoAsync(id, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // PATCH /api/v1/motivos-bloqueo-salon/{id}/estado
    [HttpPatch("{id:int}/estado")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] EstadoImputacionRequest req)
    {
        var result = await _svc.CambiarEstadoMotivoBloqueoAsync(id, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    private IActionResult Error<T>(Result<T> result)
    {
        var status = result.ErrorCode switch
        {
            "MOTIVO_NO_ENCONTRADO" => 404,
            _                      => 400
        };
        return StatusCode(status, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
    }
}
