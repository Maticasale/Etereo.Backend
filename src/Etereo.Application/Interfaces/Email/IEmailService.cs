namespace Etereo.Application.Interfaces.Email;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string nombre, string resetLink);
    Task SendWelcomeEmailAsync(string toEmail, string nombre);
}
