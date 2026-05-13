using System.Security.Claims;
using Etereo.Api.Attributes;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Cupones;
using Etereo.Shared.Cupones;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/cupones")]
public class CuponesController : ControllerBase
{
    private readonly ICuponesService _svc;

    public CuponesController(ICuponesService svc) => _svc = svc;

    // GET /api/v1/cupones — Admin
    [HttpGet]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Listar()
    {
        var result = await _svc.ListarAsync();
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/cupones — Admin
    [HttpPost]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Crear([FromBody] CrearCuponRequest req)
    {
        var result = await _svc.CrearAsync(req);
        return result.IsSuccess ? StatusCode(201, new { data = result.Value }) : Error(result);
    }

    // PUT /api/v1/cupones/{id} — Admin
    [HttpPut("{id:int}")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarCuponRequest req)
    {
        var result = await _svc.ActualizarAsync(id, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // PATCH /api/v1/cupones/{id}/estado — Admin
    [HttpPatch("{id:int}/estado")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] EstadoCuponRequest req)
    {
        var result = await _svc.CambiarEstadoAsync(id, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/cupones/disponibles — Cliente
    [HttpGet("disponibles")]
    [RequiereRol("Cliente")]
    public async Task<IActionResult> Disponibles()
    {
        var clienteId = int.Parse(User.FindFirstValue("sub")!);
        var result = await _svc.DisponiblesAsync(clienteId);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/cupones/validar/{codigo} — Cliente
    [HttpGet("validar/{codigo}")]
    [RequiereRol("Cliente")]
    public async Task<IActionResult> Validar(string codigo)
    {
        var clienteId = int.Parse(User.FindFirstValue("sub")!);
        var result = await _svc.ValidarAsync(codigo, clienteId);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IActionResult Error<T>(Result<T> result)
    {
        var status = result.ErrorCode switch
        {
            "CUPON_NO_ENCONTRADO"     => 404,
            "CODIGO_EN_USO"           => 409,
            "CUPON_YA_USADO"          => 409,
            "CUPON_AGOTADO"           => 409,
            "CUPON_EXPIRADO"          => 400,
            "TIPO_DESCUENTO_INVALIDO" => 422,
            _                         => 400
        };
        return StatusCode(status, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
    }
}
