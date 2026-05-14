using System.Security.Claims;
using Etereo.Api.Attributes;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Estadisticas;
using Etereo.Shared.Estadisticas;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/comisiones")]
public class ComisionesController : ControllerBase
{
    private readonly IEstadisticasService _svc;

    public ComisionesController(IEstadisticasService svc) => _svc = svc;

    // GET /api/v1/comisiones — Admin: todas las comisiones
    [HttpGet]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Listar(
        [FromQuery] DateOnly? fechaDesde,
        [FromQuery] DateOnly? fechaHasta,
        [FromQuery] int? operarioId)
    {
        var result = await _svc.ListarComisionesAsync(fechaDesde, fechaHasta, operarioId);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/comisiones/mi-resumen — Operario: su propio resumen
    [HttpGet("mi-resumen")]
    [RequiereRol("Operario")]
    public async Task<IActionResult> MiResumen(
        [FromQuery] DateOnly? fechaDesde,
        [FromQuery] DateOnly? fechaHasta)
    {
        var sub = User.FindFirstValue("sub");
        if (sub is null || !int.TryParse(sub, out var operarioId))
            return Unauthorized();

        var result = await _svc.ResumenComisionesAsync(operarioId, fechaDesde, fechaHasta);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    private IActionResult Error<T>(Result<T> result)
    {
        var status = result.ErrorCode == "OPERARIO_NO_ENCONTRADO" ? 404 : 400;
        return StatusCode(status, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
    }
}
