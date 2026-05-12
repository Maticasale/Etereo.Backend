using Etereo.Domain.Entities.Turnos;

namespace Etereo.Application.Interfaces.Turnos;

public interface ITurnosDbContext
{
    IQueryable<Sesion> Sesiones { get; }
    IQueryable<Turno> Turnos { get; }
    IQueryable<ReglaDescuentoSesion> ReglasDescuentoSesion { get; }

    void AddSesion(Sesion s);
    void AddTurno(Turno t);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
