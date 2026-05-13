using Etereo.Api.Attributes;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Estadisticas;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/estadisticas")]
public class EstadisticasController : ControllerBase
{
    private readonly IEstadisticasService _svc;

    public EstadisticasController(IEstadisticasService svc) => _svc = svc;

    // GET /api/v1/estadisticas/resumen
    [HttpGet("resumen")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Resumen()
    {
        var result = await _svc.ResumenAsync();
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/estadisticas/evolucion?fechaDesde=&fechaHasta=&agrupacion=dia|semana|mes
    [HttpGet("evolucion")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Evolucion(
        [FromQuery] DateOnly fechaDesde,
        [FromQuery] DateOnly fechaHasta,
        [FromQuery] string agrupacion = "dia")
    {
        var result = await _svc.EvolucionAsync(fechaDesde, fechaHasta, agrupacion);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/estadisticas/servicios
    [HttpGet("servicios")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> RankingServicios(
        [FromQuery] DateOnly? fechaDesde,
        [FromQuery] DateOnly? fechaHasta)
    {
        var result = await _svc.RankingServiciosAsync(fechaDesde, fechaHasta);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/estadisticas/operarias
    [HttpGet("operarias")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> EstadisticasOperarias(
        [FromQuery] DateOnly? fechaDesde,
        [FromQuery] DateOnly? fechaHasta)
    {
        var result = await _svc.EstadisticasOperariasAsync(fechaDesde, fechaHasta);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/estadisticas/ocupacion?fechaDesde=&fechaHasta=
    [HttpGet("ocupacion")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Ocupacion(
        [FromQuery] DateOnly fechaDesde,
        [FromQuery] DateOnly fechaHasta)
    {
        var result = await _svc.OcupacionAsync(fechaDesde, fechaHasta);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    private IActionResult Error<T>(Result<T> result) =>
        StatusCode(400, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
}
