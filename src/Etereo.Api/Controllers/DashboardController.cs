using System.Security.Claims;
using Etereo.Api.Attributes;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Estadisticas;
using Etereo.Shared.Estadisticas;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IEstadisticasService _svc;

    public DashboardController(IEstadisticasService svc) => _svc = svc;

    // GET /api/v1/dashboard/kpis
    [HttpGet("kpis")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Kpis()
    {
        var result = await _svc.KpisAsync();
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/dashboard/alertas
    [HttpGet("alertas")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Alertas()
    {
        var result = await _svc.AlertasAsync();
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/dashboard/agenda-hoy
    [HttpGet("agenda-hoy")]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> AgendaHoy()
    {
        // Si es Operario, filtra sólo sus turnos
        var rol = User.FindFirstValue("rol");
        int? operarioId = null;
        if (rol == "Operario")
        {
            var sub = User.FindFirstValue("sub");
            if (sub is not null && int.TryParse(sub, out var id))
                operarioId = id;
        }

        var result = await _svc.AgendaHoyAsync(operarioId);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    private IActionResult Error<T>(Result<T> result) =>
        StatusCode(400, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
}
