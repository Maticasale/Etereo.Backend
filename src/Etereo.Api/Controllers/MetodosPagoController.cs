using Etereo.Api.Attributes;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Imputaciones;
using Etereo.Shared.Imputaciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/metodos-pago")]
public class MetodosPagoController : ControllerBase
{
    private readonly IImputacionesService _svc;

    public MetodosPagoController(IImputacionesService svc) => _svc = svc;

    // GET /api/v1/metodos-pago — Anónimo (SOT 4.8)
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Listar()
    {
        var result = await _svc.ListarMetodosPagoAsync();
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/metodos-pago
    [HttpPost]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Crear([FromBody] CrearMetodoPagoRequest req)
    {
        var result = await _svc.CrearMetodoPagoAsync(req);
        return result.IsSuccess ? StatusCode(201, new { data = result.Value }) : Error(result);
    }

    // PUT /api/v1/metodos-pago/{id}
    [HttpPut("{id:int}")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarMetodoPagoRequest req)
    {
        var result = await _svc.ActualizarMetodoPagoAsync(id, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // PATCH /api/v1/metodos-pago/{id}/estado
    [HttpPatch("{id:int}/estado")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] EstadoImputacionRequest req)
    {
        var result = await _svc.CambiarEstadoMetodoPagoAsync(id, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    private IActionResult Error<T>(Result<T> result)
    {
        var status = result.ErrorCode == "METODO_PAGO_NO_ENCONTRADO" ? 404 : 400;
        return StatusCode(status, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
    }
}
