using Etereo.Domain.Entities.Servicios;
using Etereo.Domain.Entities.Turnos;

namespace Etereo.Application.Interfaces.Servicios;

public interface IServiciosDbContext
{
    IQueryable<Servicio>             Servicios             { get; }
    IQueryable<Subservicio>          Subservicios          { get; }
    IQueryable<VarianteSubservicio>  VariantesSubservicio  { get; }
    IQueryable<ReglaDescuentoSesion> ReglasDescuentoSesion { get; }

    void AddServicio(Servicio s);
    void AddSubservicio(Subservicio s);
    void AddVarianteSubservicio(VarianteSubservicio v);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
