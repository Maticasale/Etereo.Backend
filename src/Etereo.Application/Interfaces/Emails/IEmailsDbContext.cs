using Etereo.Domain.Entities.Emails;

namespace Etereo.Application.Interfaces.Emails;

public interface IEmailsDbContext
{
    IQueryable<ConfiguracionEmail> ConfiguracionesEmail { get; }
    IQueryable<EmailEnviado> EmailsEnviados { get; }
    IQueryable<Calificacion> Calificaciones { get; }

    void AddEmailEnviado(EmailEnviado e);
    void AddCalificacion(Calificacion c);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
