using System.Security.Claims;
using Etereo.Api.Attributes;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Servicios;
using Etereo.Domain.Enums;
using Etereo.Shared.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1")]
public class ServiciosController : ControllerBase
{
    private readonly IServiciosService _svc;

    public ServiciosController(IServiciosService svc) => _svc = svc;

    // ── Servicios ─────────────────────────────────────────────────────────────

    // GET /api/v1/servicios  (anónimo, pero filtra por sexo si hay token)
    [HttpGet("servicios")]
    [AllowAnonymous]
    public async Task<IActionResult> Listar()
    {
        var filtro = ResolverFiltroSexo();
        var result = await _svc.ListarAsync(filtro);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/servicios/estado-configuracion
    [HttpGet("servicios/estado-configuracion")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> EstadoConfiguracion()
    {
        var result = await _svc.EstadoConfiguracionAsync();
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/servicios/{id}
    [HttpGet("servicios/{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Obtener(int id)
    {
        var filtro = ResolverFiltroSexo();
        var result = await _svc.ObtenerAsync(id, filtro);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/servicios
    [HttpPost("servicios")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Crear([FromBody] CrearServicioRequest req)
    {
        var result = await _svc.CrearAsync(req);
        return result.IsSuccess ? StatusCode(201, new { data = result.Value }) : Error(result);
    }

    // PUT /api/v1/servicios/{id}
    [HttpPut("servicios/{id:int}")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarServicioRequest req)
    {
        var result = await _svc.ActualizarAsync(id, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // PATCH /api/v1/servicios/{id}/estado
    [HttpPatch("servicios/{id:int}/estado")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] EstadoRequest req)
    {
        var result = await _svc.CambiarEstadoAsync(id, req.Activo);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // ── Subservicios ──────────────────────────────────────────────────────────

    // POST /api/v1/subservicios
    [HttpPost("subservicios")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> CrearSubservicio([FromBody] CrearSubservicioRequest req)
    {
        var result = await _svc.CrearSubservicioAsync(req);
        return result.IsSuccess ? StatusCode(201, new { data = result.Value }) : Error(result);
    }

    // PUT /api/v1/subservicios/{id}
    [HttpPut("subservicios/{id:int}")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> ActualizarSubservicio(int id, [FromBody] ActualizarSubservicioRequest req)
    {
        var result = await _svc.ActualizarSubservicioAsync(id, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // PATCH /api/v1/subservicios/{id}/estado
    [HttpPatch("subservicios/{id:int}/estado")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> CambiarEstadoSubservicio(int id, [FromBody] EstadoRequest req)
    {
        var result = await _svc.CambiarEstadoSubservicioAsync(id, req.Activo);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // ── Variantes ─────────────────────────────────────────────────────────────

    // POST /api/v1/subservicios/{id}/variantes
    [HttpPost("subservicios/{id:int}/variantes")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> CrearVariante(int id, [FromBody] CrearVarianteRequest req)
    {
        var result = await _svc.CrearVarianteAsync(id, req);
        return result.IsSuccess ? StatusCode(201, new { data = result.Value }) : Error(result);
    }

    // PUT /api/v1/subservicios/{id}/variantes/{vid}
    [HttpPut("subservicios/{id:int}/variantes/{vid:int}")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> ActualizarVariante(int id, int vid, [FromBody] ActualizarVarianteRequest req)
    {
        var result = await _svc.ActualizarVarianteAsync(id, vid, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // PATCH /api/v1/subservicios/{id}/variantes/{vid}/estado
    [HttpPatch("subservicios/{id:int}/variantes/{vid:int}/estado")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> CambiarEstadoVariante(int id, int vid, [FromBody] EstadoRequest req)
    {
        var result = await _svc.CambiarEstadoVarianteAsync(id, vid, req.Activo);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // ── Reglas descuento ──────────────────────────────────────────────────────

    // GET /api/v1/reglas-descuento-sesion
    [HttpGet("reglas-descuento-sesion")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> ListarReglas()
    {
        var result = await _svc.ListarReglasAsync();
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // PUT /api/v1/reglas-descuento-sesion/{id}
    [HttpPut("reglas-descuento-sesion/{id:int}")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> ActualizarRegla(int id, [FromBody] ActualizarReglaDescuentoRequest req)
    {
        var result = await _svc.ActualizarReglaAsync(id, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Resuelve el filtro de sexo desde el claim "rol" y "sexo" del usuario autenticado.
    /// Si no hay token o el rol no es Cliente, retorna null (sin filtro).
    /// </summary>
    private SexoSubservicio? ResolverFiltroSexo()
    {
        if (!User.Identity?.IsAuthenticated ?? true) return null;

        var rol = User.FindFirstValue("rol");
        if (rol != "Cliente") return null;

        // El sexo viene del claim "sexo" que se agrega en el token (si se incluye),
        // o se puede agregar como claim adicional en el JWT. Por ahora lo resolvemos
        // con un claim opcional; si no está presente, no se filtra.
        var sexoClaim = User.FindFirstValue("sexo");
        if (sexoClaim is null) return null;

        return Enum.TryParse<SexoSubservicio>(sexoClaim, true, out var sexo) ? sexo : null;
    }

    private IActionResult Error<T>(Result<T> result)
    {
        var status = result.ErrorCode switch
        {
            "SERVICIO_NO_ENCONTRADO"     => 404,
            "SUBSERVICIO_NO_ENCONTRADO"  => 404,
            "VARIANTE_NO_ENCONTRADA"     => 404,
            "REGLA_NO_ENCONTRADA"        => 404,
            _                            => 400
        };
        return StatusCode(status, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
    }
}
