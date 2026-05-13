using System.Security.Claims;
using Etereo.Api.Attributes;
using Etereo.Application.Common;
using Etereo.Application.Interfaces.Turnos;
using Etereo.Shared.Turnos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Etereo.Api.Controllers;

[ApiController]
[Route("api/v1/turnos")]
public class TurnosController : ControllerBase
{
    private readonly ITurnosService _svc;

    public TurnosController(ITurnosService svc) => _svc = svc;

    // POST /api/v1/turnos — Anónimo | [Authorize]
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Crear([FromBody] CrearTurnoRequest req)
    {
        var userId = ObtenerUserId();
        var result = await _svc.CrearTurnoAsync(req, userId);
        return result.IsSuccess ? StatusCode(201, new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/turnos — Admin | Operario
    [HttpGet]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> Listar(
        [FromQuery] DateOnly? fecha,
        [FromQuery] int?      operarioId,
        [FromQuery] string?   estado)
    {
        var result = await _svc.ListarTurnosAsync(fecha, operarioId, estado);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/turnos/disponibilidad — Anónimo
    [HttpGet("disponibilidad")]
    [AllowAnonymous]
    public async Task<IActionResult> Disponibilidad(
        [FromQuery] DateOnly fecha,
        [FromQuery] int operarioId,
        [FromQuery] int duracionMin = 30)
    {
        var result = await _svc.ObtenerDisponibilidadAsync(fecha, operarioId, duracionMin);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/turnos/mis-turnos — Cliente
    [HttpGet("mis-turnos")]
    [RequiereRol("Cliente")]
    public async Task<IActionResult> MisTurnos()
    {
        var userId = ObtenerUserId()!.Value;
        var result = await _svc.MisTurnosAsync(userId);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // GET /api/v1/turnos/{id} — Admin | Operario | Cliente propio
    [HttpGet("{id:int}")]
    [RequiereRol("Admin", "Operario", "Cliente")]
    public async Task<IActionResult> Obtener(int id)
    {
        var userId = ObtenerUserId();
        var rol    = User.FindFirstValue("rol");
        var result = await _svc.ObtenerTurnoAsync(id, userId, rol);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/turnos/{id}/confirmar — Admin | Operario
    [HttpPost("{id:int}/confirmar")]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> Confirmar(int id)
    {
        var result = await _svc.ConfirmarTurnoAsync(id);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/turnos/{id}/rechazar — Admin | Operario
    [HttpPost("{id:int}/rechazar")]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> Rechazar(int id, [FromBody] RechazarTurnoRequest req)
    {
        var result = await _svc.RechazarTurnoAsync(id, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/turnos/{id}/cancelar — Admin | Operario | Cliente propio
    [HttpPost("{id:int}/cancelar")]
    [RequiereRol("Admin", "Operario", "Cliente")]
    public async Task<IActionResult> Cancelar(int id)
    {
        var userId = ObtenerUserId();
        var rol    = User.FindFirstValue("rol");
        var result = await _svc.CancelarTurnoAsync(id, userId, rol);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/turnos/{id}/multa — Admin | Operario
    [HttpPost("{id:int}/multa")]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> Multa(int id)
    {
        var result = await _svc.MultaTurnoAsync(id);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/turnos/{id}/ausente — Admin | Operario
    [HttpPost("{id:int}/ausente")]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> Ausente(int id)
    {
        var result = await _svc.AusenteTurnoAsync(id);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/turnos/{id}/realizar — Admin | Operario
    [HttpPost("{id:int}/realizar")]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> Realizar(int id, [FromBody] RealizarTurnoRequest req)
    {
        var result = await _svc.RealizarTurnoAsync(id, req);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/turnos/{id}/impago — Admin | Operario
    [HttpPost("{id:int}/impago")]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> Impago(int id)
    {
        var result = await _svc.ImpagoTurnoAsync(id);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // POST /api/v1/turnos/{id}/publicidad — Admin | Operario
    [HttpPost("{id:int}/publicidad")]
    [RequiereRol("Admin", "Operario")]
    public async Task<IActionResult> Publicidad(int id)
    {
        var result = await _svc.PublicidadTurnoAsync(id);
        return result.IsSuccess ? Ok(new { data = result.Value }) : Error(result);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private int? ObtenerUserId()
    {
        var sub = User.FindFirstValue("sub");
        return sub is not null && int.TryParse(sub, out var id) ? id : null;
    }

    private IActionResult Error<T>(Result<T> result)
    {
        var status = result.ErrorCode switch
        {
            "TURNO_NO_ENCONTRADO"       => 404,
            "SESION_NO_ENCONTRADA"      => 404,
            "OPERARIO_NO_ENCONTRADO"    => 404,
            "CLIENTE_NO_ENCONTRADO"     => 404,
            "SUBSERVICIO_NO_ENCONTRADO" => 404,
            "VARIANTE_NO_ENCONTRADA"    => 404,
            "METODO_PAGO_NO_ENCONTRADO" => 404,
            "SIN_PERMISO"              => 403,
            "TRANSICION_INVALIDA"      => 409,
            "CUPON_YA_USADO"           => 409,
            "CUPON_AGOTADO"            => 409,
            "SALON_INVALIDO"           => 422,
            _                          => 400
        };
        return StatusCode(status, new { error = new { codigo = result.ErrorCode, mensaje = result.ErrorMessage } });
    }
}
