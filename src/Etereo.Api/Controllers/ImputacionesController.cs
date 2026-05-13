using System.Security.Claims;
using Etereo.Api.Attributes;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Imputaciones;
using Etereo.Shared.Imputaciones;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/imputaciones")]
public class ImputacionesController : ControllerBase
{
    private readonly IImputacionesService _svc;

    public ImputacionesController(IImputacionesService svc) => _svc = svc;

    // GET /api/v1/imputaciones
    [HttpGet]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> Listar(
        [FromQuery] DateOnly? fechaDesde,
        [FromQuery] DateOnly? fechaHasta,
        [FromQuery] string? tipo,
        [FromQuery] int? operarioId)
    {
        var result = await _svc.ListarAsync(fechaDesde, fechaHasta, tipo, operarioId);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/imputaciones/resumen
    [HttpGet("resumen")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Resumen(
        [FromQuery] DateOnly? fechaDesde,
        [FromQuery] DateOnly? fechaHasta)
    {
        var result = await _svc.ResumenAsync(fechaDesde, fechaHasta);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/imputaciones
    [HttpPost]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> Crear([FromBody] CrearImputacionRequest req)
    {
        var sub = User.FindFirstValue("sub");
        if (sub is null || !int.TryParse(sub, out var userId))
            return Unauthorized();
        var result = await _svc.CrearAsync(req, userId);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // PUT /api/v1/imputaciones/{id}
    [HttpPut("{id:int}")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarImputacionRequest req)
    {
        var result = await _svc.ActualizarAsync(id, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // DELETE /api/v1/imputaciones/{id}
    [HttpDelete("{id:int}")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var result = await _svc.EliminarAsync(id);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    private IActionResult Error<T>(Result<T> result)
    {
        var status = result.ErrorCode switch
        {
            "IMPUTACION_NO_ENCONTRADA" => 404,
            "NO_EDITABLE"              => 409,
            "NO_ELIMINABLE"            => 409,
            _                          => 400
        };
        return StatusCode(status, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
    }
}
