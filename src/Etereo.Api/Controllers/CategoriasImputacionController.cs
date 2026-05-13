using Etereo.Api.Attributes;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Imputaciones;
using Etereo.Shared.Imputaciones;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/categorias-imputacion")]
public class CategoriasImputacionController : ControllerBase
{
    private readonly IImputacionesService _svc;

    public CategoriasImputacionController(IImputacionesService svc) => _svc = svc;

    // GET /api/v1/categorias-imputacion
    [HttpGet]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> Listar()
    {
        var result = await _svc.ListarCategoriasAsync();
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/categorias-imputacion
    [HttpPost]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Crear([FromBody] CrearCategoriaImputacionRequest req)
    {
        var result = await _svc.CrearCategoriaAsync(req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // PUT /api/v1/categorias-imputacion/{id}
    [HttpPut("{id:int}")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarCategoriaImputacionRequest req)
    {
        var result = await _svc.ActualizarCategoriaAsync(id, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // PATCH /api/v1/categorias-imputacion/{id}/estado
    [HttpPatch("{id:int}/estado")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] EstadoImputacionRequest req)
    {
        var result = await _svc.CambiarEstadoCategoriaAsync(id, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    private IActionResult Error<T>(Result<T> result)
    {
        var status = result.ErrorCode == "CATEGORIA_NO_ENCONTRADA" ? 404 : 400;
        return StatusCode(status, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
    }
}
