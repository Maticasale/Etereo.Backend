using System.Net.Http.Json;
using Etereo.Application.Interfaces.Email;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Etereo.Infrastructure.Services.Email;

public class ResendEmailService : IEmailService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(HttpClient http, IConfiguration config, ILogger<ResendEmailService> logger)
    {
        _http   = http;
        _config = config;
        _logger = logger;
    }

    private string From => _config["RESEND_FROM_EMAIL"] ?? "noreply@example.com";

    public async Task SendPasswordResetEmailAsync(string toEmail, string nombre, string resetLink)
    {
        var html = $"""
            <div style="font-family:sans-serif;max-width:500px;margin:auto">
              <h2 style="color:#111">Recuperación de contraseña</h2>
              <p>Hola <strong>{nombre}</strong>,</p>
              <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta en <strong>Etéreo Salón</strong>.</p>
              <p style="margin:24px 0">
                <a href="{resetLink}"
                   style="background:#111;color:#fff;padding:12px 24px;text-decoration:none;border-radius:6px;font-weight:bold">
                  Restablecer contraseña
                </a>
              </p>
              <p style="color:#666;font-size:14px">Este link es válido por <strong>1 hora</strong>. Si no solicitaste esto, podés ignorar este email.</p>
              <hr style="border:none;border-top:1px solid #eee;margin-top:32px"/>
              <p style="color:#999;font-size:12px">Etéreo Salón</p>
            </div>
            """;

        await SendAsync(toEmail, "Restablecer tu contraseña — Etéreo", html);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string nombre)
    {
        var html = $"""
            <div style="font-family:sans-serif;max-width:500px;margin:auto">
              <h2 style="color:#111">¡Bienvenida a Etéreo!</h2>
              <p>Hola <strong>{nombre}</strong>, tu cuenta fue creada exitosamente.</p>
              <p>Ya podés iniciar sesión y reservar tus turnos.</p>
              <hr style="border:none;border-top:1px solid #eee;margin-top:32px"/>
              <p style="color:#999;font-size:12px">Etéreo Salón</p>
            </div>
            """;

        await SendAsync(toEmail, "¡Bienvenida a Etéreo!", html);
    }

    public async Task SendConfirmacionTurnoAsync(string toEmail, string nombre, DateTime fechaHoraInicio, string servicio, string operario)
    {
        var fecha = fechaHoraInicio.ToString("dddd d 'de' MMMM", new System.Globalization.CultureInfo("es-AR"));
        var hora  = fechaHoraInicio.ToString("HH:mm");
        var html = $"""
            <div style="font-family:sans-serif;max-width:500px;margin:auto">
              <h2 style="color:#111">¡Turno confirmado!</h2>
              <p>Hola <strong>{nombre}</strong>, tu turno fue confirmado.</p>
              <table style="width:100%;margin:16px 0;border-collapse:collapse">
                <tr><td style="color:#666;padding:4px 0">Servicio:</td><td><strong>{servicio}</strong></td></tr>
                <tr><td style="color:#666;padding:4px 0">Operaria:</td><td><strong>{operario}</strong></td></tr>
                <tr><td style="color:#666;padding:4px 0">Fecha:</td><td><strong>{fecha}</strong></td></tr>
                <tr><td style="color:#666;padding:4px 0">Hora:</td><td><strong>{hora}</strong></td></tr>
              </table>
              <hr style="border:none;border-top:1px solid #eee;margin-top:32px"/>
              <p style="color:#999;font-size:12px">Etéreo Salón</p>
            </div>
            """;
        await SendAsync(toEmail, "Tu turno fue confirmado — Etéreo", html);
    }

    public async Task SendRechazoTurnoAsync(string toEmail, string nombre, DateTime fechaHoraInicio, string motivo)
    {
        var fecha = fechaHoraInicio.ToString("dddd d 'de' MMMM 'a las' HH:mm", new System.Globalization.CultureInfo("es-AR"));
        var html = $"""
            <div style="font-family:sans-serif;max-width:500px;margin:auto">
              <h2 style="color:#111">Turno rechazado</h2>
              <p>Hola <strong>{nombre}</strong>, lamentablemente tu turno del <strong>{fecha}</strong> no pudo confirmarse.</p>
              <p><strong>Motivo:</strong> {motivo}</p>
              <p>Podés reservar un nuevo turno cuando quieras.</p>
              <hr style="border:none;border-top:1px solid #eee;margin-top:32px"/>
              <p style="color:#999;font-size:12px">Etéreo Salón</p>
            </div>
            """;
        await SendAsync(toEmail, "Tu turno no pudo confirmarse — Etéreo", html);
    }

    public async Task SendCancelacionTurnoAsync(string toEmail, string nombre, DateTime fechaHoraInicio)
    {
        var fecha = fechaHoraInicio.ToString("dddd d 'de' MMMM 'a las' HH:mm", new System.Globalization.CultureInfo("es-AR"));
        var html = $"""
            <div style="font-family:sans-serif;max-width:500px;margin:auto">
              <h2 style="color:#111">Turno cancelado</h2>
              <p>Hola <strong>{nombre}</strong>, tu turno del <strong>{fecha}</strong> fue cancelado.</p>
              <p>Si querés, podés reservar un nuevo turno.</p>
              <hr style="border:none;border-top:1px solid #eee;margin-top:32px"/>
              <p style="color:#999;font-size:12px">Etéreo Salón</p>
            </div>
            """;
        await SendAsync(toEmail, "Tu turno fue cancelado — Etéreo", html);
    }

    public async Task SendPostTurnoCalificacionAsync(string toEmail, string nombre, int turnoId)
    {
        var html = $"""
            <div style="font-family:sans-serif;max-width:500px;margin:auto">
              <h2 style="color:#111">¿Cómo estuvo tu turno?</h2>
              <p>Hola <strong>{nombre}</strong>, gracias por visitarnos.</p>
              <p>Tu opinión nos ayuda a mejorar.</p>
              <hr style="border:none;border-top:1px solid #eee;margin-top:32px"/>
              <p style="color:#999;font-size:12px">Etéreo Salón — turno #{turnoId}</p>
            </div>
            """;
        await SendAsync(toEmail, "¿Cómo estuvo tu turno? — Etéreo", html);
    }

    public async Task SendCampanaAsync(string toEmail, string nombre, string asunto, string contenido)
    {
        var html = $"""
            <div style="font-family:sans-serif;max-width:500px;margin:auto">
              <p>Hola <strong>{nombre}</strong>,</p>
              <div>{contenido}</div>
              <hr style="border:none;border-top:1px solid #eee;margin-top:32px"/>
              <p style="color:#999;font-size:12px">Etéreo Salón</p>
            </div>
            """;
        await SendAsync(toEmail, asunto, html);
    }

    // ── Interno ───────────────────────────────────────────────────────────────

    private async Task SendAsync(string toEmail, string subject, string html)
    {
        var payload = new
        {
            from    = From,
            to      = new[] { toEmail },
            subject = subject,
            html    = html
        };

        try
        {
            var resp = await _http.PostAsJsonAsync("https://api.resend.com/emails", payload);
            if (!resp.IsSuccessStatusCode)
            {
                var body = await resp.Content.ReadAsStringAsync();
                _logger.LogError("Resend error {Status}: {Body}", (int)resp.StatusCode, body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando email a {Email}", toEmail);
        }
    }
}
