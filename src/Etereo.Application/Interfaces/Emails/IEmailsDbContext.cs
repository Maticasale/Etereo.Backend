using Etereo.Domain.Entities.Auth;
using Etereo.Domain.Entities.Emails;
using Etereo.Domain.Entities.Servicios;
using Etereo.Domain.Entities.Turnos;

namespace Etereo.Application.Interfaces.Emails;

public interface IEmailsDbContext
{
    IQueryable<ConfiguracionEmail> ConfiguracionesEmail { get; }
    IQueryable<EmailEnviado>       EmailsEnviados        { get; }
    IQueryable<Calificacion>       Calificaciones        { get; }
    IQueryable<Turno>              Turnos                { get; }
    IQueryable<Usuario>            Usuarios              { get; }
    IQueryable<Subservicio>        Subservicios          { get; }

    void AddEmailEnviado(EmailEnviado e);
    void AddCalificacion(Calificacion c);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
