using Etereo.Api.Attributes;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Usuarios;
using Etereo.Shared.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuariosService _svc;

    public UsuariosController(IUsuariosService svc) => _svc = svc;

    // GET /api/v1/usuarios
    [HttpGet]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Listar()
    {
        var result = await _svc.ListarAsync();
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/usuarios/{id}
    [HttpGet("{id:int}")]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> Obtener(int id)
    {
        var result = await _svc.ObtenerAsync(id);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // PATCH /api/v1/usuarios/{id}
    [HttpPatch("{id:int}")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarUsuarioRequest req)
    {
        var result = await _svc.ActualizarAsync(id, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/usuarios/{id}/bloquear
    [HttpPost("{id:int}/bloquear")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Bloquear(int id, [FromBody] BloquearUsuarioRequest req)
    {
        var result = await _svc.BloquearAsync(id, req);
        return result.IsSuccess ? Ok(new { data = "ok" }) : Error(result);
    }

    // POST /api/v1/usuarios/{id}/desbloquear
    [HttpPost("{id:int}/desbloquear")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> Desbloquear(int id)
    {
        var result = await _svc.DesbloquearAsync(id);
        return result.IsSuccess ? Ok(new { data = "ok" }) : Error(result);
    }

    // POST /api/v1/usuarios/{id}/promover-operario
    [HttpPost("{id:int}/promover-operario")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> PromoverOperario(int id)
    {
        var result = await _svc.PromoverOperarioAsync(id);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/usuarios/{id}/degradar-cliente
    [HttpPost("{id:int}/degradar-cliente")]
    [RequiereRol("Admin")]
    public async Task<IActionResult> DegradarCliente(int id)
    {
        var result = await _svc.DegradarClienteAsync(id);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/usuarios/clientes
    [HttpPost("clientes")]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> CrearCliente([FromBody] CrearClienteRequest req)
    {
        var result = await _svc.CrearClienteAsync(req);
        return result.IsSuccess ? StatusCode(201, new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/usuarios/clientes/buscar?q=texto
    [HttpGet("clientes/buscar")]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> BuscarClientes([FromQuery] string? q)
    {
        var result = await _svc.BuscarClientesAsync(q);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private IActionResult Error<T>(Result<T> result)
    {
        var status = result.ErrorCode switch
        {
            "USUARIO_NO_ENCONTRADO" => 404,
            "CREDENCIALES_EN_USO"   => 409,
            "NO_PERMITIDO"          => 403,
            "YA_ES_OPERARIO"        => 409,
            "YA_ES_CLIENTE"         => 409,
            _                       => 400
        };
        return StatusCode(status, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
    }
}
