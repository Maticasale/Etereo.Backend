using System.Security.Claims;
using Etereo.Api.Attributes;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Operarios;
using Etereo.Shared.Operarios;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/operarios")]
public class OperariosController : ControllerBase
{
    private readonly IOperariosService _svc;

    public OperariosController(IOperariosService svc) => _svc = svc;

    // GET /api/v1/operarios
    [HttpGet]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> Listar()
    {
        var result = await _svc.ListarAsync();
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/operarios/{id}
    [HttpGet("{id:int}")]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> Obtener(int id)
    {
        var result = await _svc.ObtenerAsync(id);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/operarios/{id}/subservicios
    [HttpGet("{id:int}/subservicios")]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> ListarSubservicios(int id)
    {
        var result = await _svc.ListarSubserviciosAsync(id);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/operarios/{id}/subservicios
    [HttpPost("{id:int}/subservicios")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> AsignarSubservicio(int id, [FromBody] AsignarSubservicioRequest req)
    {
        var result = await _svc.AsignarSubservicioAsync(id, req);
        return result.IsSuccess ? StatusCode(201, new { data = result.Value }) : Error(result);
    }

    // PUT /api/v1/operarios/{id}/subservicios/{subservicioId}
    [HttpPut("{id:int}/subservicios/{subservicioId:int}")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> ActualizarComision(int id, int subservicioId, [FromBody] ActualizarComisionRequest req)
    {
        var result = await _svc.ActualizarComisionAsync(id, subservicioId, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // DELETE /api/v1/operarios/{id}/subservicios/{subservicioId}
    [HttpDelete("{id:int}/subservicios/{subservicioId:int}")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> QuitarSubservicio(int id, int subservicioId)
    {
        var result = await _svc.QuitarSubservicioAsync(id, subservicioId);
        return result.IsSuccess ? NoContent() : Error(result);
    }

    // GET /api/v1/operarios/{id}/vistas
    [HttpGet("{id:int}/vistas")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> ObtenerVistas(int id)
    {
        var result = await _svc.ObtenerVistasAsync(id);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // PUT /api/v1/operarios/{id}/vistas
    [HttpPut("{id:int}/vistas")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> ActualizarVistas(int id, [FromBody] ActualizarVistasRequest req)
    {
        var result = await _svc.ActualizarVistasAsync(id, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IActionResult Error<T>(Result<T> result)
    {
        var status = result.ErrorCode switch
        {
            "OPERARIO_NO_ENCONTRADO"    => 404,
            "SUBSERVICIO_NO_ENCONTRADO" => 404,
            "ASIGNACION_NO_ENCONTRADA"  => 404,
            "YA_ASIGNADO"               => 409,
            _                           => 400
        };
        return StatusCode(status, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
    }
}
