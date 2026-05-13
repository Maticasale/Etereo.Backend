namespace Etereo.Application.Interfaces.Email;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string nombre, string resetLink);
    Task SendWelcomeEmailAsync(string toEmail, string nombre);
    Task SendConfirmacionTurnoAsync(string toEmail, string nombre, DateTime fechaHoraInicio, string servicio, string operario);
    Task SendRechazoTurnoAsync(string toEmail, string nombre, DateTime fechaHoraInicio, string motivo);
    Task SendCancelacionTurnoAsync(string toEmail, string nombre, DateTime fechaHoraInicio);
    Task SendPostTurnoCalificacionAsync(string toEmail, string nombre, int turnoId);
    Task SendCampanaAsync(string toEmail, string nombre, string asunto, string contenido);
}
