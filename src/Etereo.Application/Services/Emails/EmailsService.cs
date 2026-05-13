using Etereo.Application.Common;
using Etereo.Application.Interfaces.Email;
using Etereo.Application.Interfaces.Emails;
using Etereo.Domain.Entities.Emails;
using Etereo.Domain.Enums;
using Etereo.Shared.Emails;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Etereo.Application.Services.Emails;

public class EmailsService : IEmailsService
{
    private readonly IEmailsDbContext _db;
    private readonly IEmailService    _mailer;
    private readonly ILogger<EmailsService> _log;

    public EmailsService(IEmailsDbContext db, IEmailService mailer, ILogger<EmailsService> log)
    {
        _db     = db;
        _mailer = mailer;
        _log    = log;
    }

    // ── Configuración ─────────────────────────────────────────────────────────

    public async Task<Result<ConfiguracionEmailDto>> ObtenerConfiguracionAsync()
    {
        var cfg = await _db.ConfiguracionesEmail.FirstOrDefaultAsync()
                  ?? new ConfiguracionEmail();
        return Result<ConfiguracionEmailDto>.Success(ToCfgDto(cfg));
    }

    public async Task<Result<ConfiguracionEmailDto>> ActualizarConfiguracionAsync(ActualizarConfiguracionEmailRequest req)
    {
        // El registro de configuración se crea en el seeder — si no existe devolvemos error
        var cfg = await _db.ConfiguracionesEmail.FirstOrDefaultAsync();
        if (cfg is null)
            return Result<ConfiguracionEmailDto>.Failure("CONFIG_NO_ENCONTRADA",
                "Configuración no inicializada. Ejecute el seeder.");

        if (req.RecordatorioDiasAntes.HasValue) cfg.RecordatorioDiasAntes = req.RecordatorioDiasAntes.Value;
        if (req.PostturnoHorasDespues.HasValue) cfg.PostturnoHorasDespues = req.PostturnoHorasDespues.Value;
        if (req.EmailsActivos.HasValue)         cfg.EmailsActivos         = req.EmailsActivos.Value;
        cfg.ActualizadoEn = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Result<ConfiguracionEmailDto>.Success(ToCfgDto(cfg));
    }

    // ── Historial ─────────────────────────────────────────────────────────────

    public async Task<Result<List<EmailEnviadoDto>>> ListarHistorialAsync(
        string? tipo, string? estado, DateOnly? fechaDesde, DateOnly? fechaHasta)
    {
        var query = _db.EmailsEnviados.AsQueryable();

        if (!string.IsNullOrEmpty(tipo) && Enum.TryParse<TipoEmail>(tipo, true, out var tipoEnum))
            query = query.Where(e => e.Tipo == tipoEnum);

        if (!string.IsNullOrEmpty(estado) && Enum.TryParse<EstadoEmail>(estado, true, out var estadoEnum))
            query = query.Where(e => e.Estado == estadoEnum);

        if (fechaDesde.HasValue)
            query = query.Where(e => DateOnly.FromDateTime(e.EnviadoEn) >= fechaDesde.Value);
        if (fechaHasta.HasValue)
            query = query.Where(e => DateOnly.FromDateTime(e.EnviadoEn) <= fechaHasta.Value);

        var list = await query.OrderByDescending(e => e.EnviadoEn).ToListAsync();
        return Result<List<EmailEnviadoDto>>.Success(list.Select(ToEnviadoDto).ToList());
    }

    // ── Calificaciones ────────────────────────────────────────────────────────

    public async Task<Result<CalificacionDto>> CrearCalificacionAsync(CrearCalificacionRequest req, int clienteId)
    {
        if (req.Puntuacion < 1 || req.Puntuacion > 5)
            return Result<CalificacionDto>.Failure("PUNTUACION_INVALIDA", "La puntuación debe ser entre 1 y 5.");

        var turno = await _db.Turnos.FirstOrDefaultAsync(t => t.Id == req.TurnoId);
        if (turno is null)
            return Result<CalificacionDto>.Failure("TURNO_NO_ENCONTRADO", "Turno no encontrado.");

        if (turno.ClienteId != clienteId)
            return Result<CalificacionDto>.Failure("SIN_PERMISO", "Solo podés calificar tus propios turnos.");

        if (turno.Estado != EstadoTurno.Realizado)
            return Result<CalificacionDto>.Failure("TURNO_NO_REALIZADO", "Solo se pueden calificar turnos realizados.");

        var yaCalificado = await _db.Calificaciones.AnyAsync(c => c.TurnoId == req.TurnoId);
        if (yaCalificado)
            return Result<CalificacionDto>.Failure("YA_CALIFICADO", "Este turno ya fue calificado.");

        var cal = new Calificacion
        {
            TurnoId    = req.TurnoId,
            ClienteId  = clienteId,
            OperarioId = turno.OperarioId,
            Puntuacion = req.Puntuacion,
            Comentario = req.Comentario?.Trim(),
            CreadoEn   = DateTime.UtcNow
        };
        _db.AddCalificacion(cal);
        await _db.SaveChangesAsync();

        var cliente  = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == clienteId);
        var operario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == turno.OperarioId);

        return Result<CalificacionDto>.Success(ToCalDto(cal, cliente, operario));
    }

    public async Task<Result<List<CalificacionDto>>> ListarCalificacionesAsync(int? operarioId)
    {
        var query = _db.Calificaciones.AsQueryable();
        if (operarioId.HasValue)
            query = query.Where(c => c.OperarioId == operarioId.Value);

        var cals     = await query.OrderByDescending(c => c.CreadoEn).ToListAsync();
        var usuarios = await _db.Usuarios.ToListAsync();

        var dtos = cals.Select(c =>
        {
            var cliente  = usuarios.FirstOrDefault(u => u.Id == c.ClienteId);
            var operario = usuarios.FirstOrDefault(u => u.Id == c.OperarioId);
            return ToCalDto(c, cliente, operario);
        }).ToList();

        return Result<List<CalificacionDto>>.Success(dtos);
    }

    public async Task<Result<PromedioCalificacionDto>> PromedioOperarioAsync(int operarioId)
    {
        var operario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == operarioId);
        if (operario is null)
            return Result<PromedioCalificacionDto>.Failure("OPERARIO_NO_ENCONTRADO", "Operario no encontrado.");

        var cals = await _db.Calificaciones.Where(c => c.OperarioId == operarioId).ToListAsync();

        return Result<PromedioCalificacionDto>.Success(new PromedioCalificacionDto
        {
            OperarioId          = operarioId,
            NombreOperario      = $"{operario.Nombre} {operario.Apellido}",
            Promedio            = cals.Count > 0 ? Math.Round(cals.Average(c => c.Puntuacion), 2) : 0,
            TotalCalificaciones = cals.Count
        });
    }

    // ── Campaña ───────────────────────────────────────────────────────────────

    public async Task<Result<bool>> EnviarCampanaAsync(EnviarCampanaRequest req)
    {
        if (req.Emails.Length == 0)
            return Result<bool>.Failure("SIN_DESTINATARIOS", "Debe especificar al menos un destinatario.");

        foreach (var email in req.Emails)
        {
            try
            {
                await _mailer.SendCampanaAsync(email, email, req.Asunto, req.Contenido);
                _db.AddEmailEnviado(new EmailEnviado
                {
                    Tipo         = TipoEmail.Campana,
                    Destinatario = email,
                    Estado       = EstadoEmail.Enviado,
                    EnviadoEn    = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Error enviando campaña a {Email}", email);
                _db.AddEmailEnviado(new EmailEnviado
                {
                    Tipo         = TipoEmail.Campana,
                    Destinatario = email,
                    Estado       = EstadoEmail.Fallido,
                    ErrorDetalle = ex.Message,
                    EnviadoEn    = DateTime.UtcNow
                });
            }
        }
        await _db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    // ── Notificaciones internas ───────────────────────────────────────────────

    public async Task NotificarConfirmacionAsync(int turnoId)
    {
        try
        {
            if (!await EmailsActivosAsync()) return;
            var (turno, cliente, sub) = await CargarContextoTurnoAsync(turnoId);
            if (turno is null || cliente?.Email is null) return;

            var operario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == turno.OperarioId);
            await _mailer.SendConfirmacionTurnoAsync(
                cliente.Email, cliente.Nombre,
                turno.FechaHoraInicio,
                sub?.Nombre ?? "Servicio",
                operario is not null ? $"{operario.Nombre} {operario.Apellido}" : "Operaria");

            await LogEmailAsync(TipoEmail.ConfirmacionTurno, cliente.Email, turnoId, cliente.Id, null);
        }
        catch (Exception ex) { _log.LogError(ex, "Error notificando confirmación turno {Id}", turnoId); }
    }

    public async Task NotificarRechazoAsync(int turnoId, string motivo)
    {
        try
        {
            if (!await EmailsActivosAsync()) return;
            var (turno, cliente, _) = await CargarContextoTurnoAsync(turnoId);
            if (turno is null || cliente?.Email is null) return;

            await _mailer.SendRechazoTurnoAsync(cliente.Email, cliente.Nombre, turno.FechaHoraInicio, motivo);
            await LogEmailAsync(TipoEmail.RechazoTurno, cliente.Email, turnoId, cliente.Id, null);
        }
        catch (Exception ex) { _log.LogError(ex, "Error notificando rechazo turno {Id}", turnoId); }
    }

    public async Task NotificarCancelacionAsync(int turnoId)
    {
        try
        {
            if (!await EmailsActivosAsync()) return;
            var (turno, cliente, _) = await CargarContextoTurnoAsync(turnoId);
            if (turno is null || cliente?.Email is null) return;

            await _mailer.SendCancelacionTurnoAsync(cliente.Email, cliente.Nombre, turno.FechaHoraInicio);
            await LogEmailAsync(TipoEmail.ConfirmacionTurno, cliente.Email, turnoId, cliente.Id, null);
        }
        catch (Exception ex) { _log.LogError(ex, "Error notificando cancelación turno {Id}", turnoId); }
    }

    public async Task NotificarPostTurnoAsync(int turnoId)
    {
        try
        {
            if (!await EmailsActivosAsync()) return;
            var (turno, cliente, _) = await CargarContextoTurnoAsync(turnoId);
            if (turno is null || cliente?.Email is null) return;

            await _mailer.SendPostTurnoCalificacionAsync(cliente.Email, cliente.Nombre, turnoId);
            await LogEmailAsync(TipoEmail.PostTurnoCalificacion, cliente.Email, turnoId, cliente.Id, null);
        }
        catch (Exception ex) { _log.LogError(ex, "Error notificando post-turno {Id}", turnoId); }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<bool> EmailsActivosAsync()
    {
        var cfg = await _db.ConfiguracionesEmail.FirstOrDefaultAsync();
        return cfg?.EmailsActivos ?? true;
    }

    private async Task<(Turno? turno, Domain.Entities.Auth.Usuario? cliente, Subservicio? sub)>
        CargarContextoTurnoAsync(int turnoId)
    {
        var turno = await _db.Turnos.FirstOrDefaultAsync(t => t.Id == turnoId);
        if (turno is null) return (null, null, null);

        var cliente = turno.ClienteId.HasValue
            ? await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == turno.ClienteId.Value)
            : null;
        var sub = await _db.Subservicios.FirstOrDefaultAsync(s => s.Id == turno.SubservicioId);
        return (turno, cliente, sub);
    }

    private async Task LogEmailAsync(TipoEmail tipo, string destinatario, int? turnoId, int? usuarioId, string? error)
    {
        _db.AddEmailEnviado(new EmailEnviado
        {
            Tipo         = tipo,
            Destinatario = destinatario,
            TurnoId      = turnoId,
            UsuarioId    = usuarioId,
            Estado       = error is null ? EstadoEmail.Enviado : EstadoEmail.Fallido,
            ErrorDetalle = error,
            EnviadoEn    = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();
    }

    private static ConfiguracionEmailDto ToCfgDto(ConfiguracionEmail c) => new()
    {
        Id                    = c.Id,
        RecordatorioDiasAntes = c.RecordatorioDiasAntes,
        PostturnoHorasDespues = c.PostturnoHorasDespues,
        EmailsActivos         = c.EmailsActivos,
        ActualizadoEn         = c.ActualizadoEn
    };

    private static EmailEnviadoDto ToEnviadoDto(EmailEnviado e) => new()
    {
        Id           = e.Id,
        Tipo         = e.Tipo.ToString(),
        Destinatario = e.Destinatario,
        TurnoId      = e.TurnoId,
        UsuarioId    = e.UsuarioId,
        Estado       = e.Estado.ToString(),
        ErrorDetalle = e.ErrorDetalle,
        EnviadoEn    = e.EnviadoEn
    };

    private static CalificacionDto ToCalDto(
        Calificacion c,
        Domain.Entities.Auth.Usuario? cliente,
        Domain.Entities.Auth.Usuario? operario) => new()
    {
        Id             = c.Id,
        TurnoId        = c.TurnoId,
        ClienteId      = c.ClienteId,
        NombreCliente  = cliente  is not null ? $"{cliente.Nombre} {cliente.Apellido}"   : string.Empty,
        OperarioId     = c.OperarioId,
        NombreOperario = operario is not null ? $"{operario.Nombre} {operario.Apellido}" : string.Empty,
        Puntuacion     = c.Puntuacion,
        Comentario     = c.Comentario,
        CreadoEn       = c.CreadoEn
    };
}
