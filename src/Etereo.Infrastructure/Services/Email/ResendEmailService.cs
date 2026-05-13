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
