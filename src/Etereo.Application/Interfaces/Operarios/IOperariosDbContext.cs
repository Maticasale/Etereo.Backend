using Etereo.Domain.Entities.Auth;
using Etereo.Domain.Entities.Imputaciones;
using Etereo.Domain.Entities.Operarios;
using Etereo.Domain.Entities.Servicios;

namespace Etereo.Application.Interfaces.Operarios;

public interface IOperariosDbContext
{
    // Operarios y usuarios
    IQueryable<Usuario>              Usuarios               { get; }
    IQueryable<OperarioSubservicio>  OperarioSubservicios   { get; }
    IQueryable<OperarioVistas>       OperarioVistas         { get; }

    // Subservicios (para nombres)
    IQueryable<Subservicio>          Subservicios           { get; }
    IQueryable<Servicio>             Servicios              { get; }

    // Disponibilidad
    IQueryable<DisponibilidadSalon>     DisponibilidadesSalon     { get; }
    IQueryable<DisponibilidadOperario>  DisponibilidadesOperario  { get; }
    IQueryable<MotivoBloqueoSalon>      MotivosBloqueoSalon       { get; }

    void AddOperarioSubservicio(OperarioSubservicio os);
    void RemoveOperarioSubservicio(OperarioSubservicio os);
    void AddOperarioVistas(OperarioVistas v);
    void AddDisponibilidadSalon(DisponibilidadSalon d);
    void RemoveDisponibilidadSalon(DisponibilidadSalon d);
    void AddDisponibilidadOperario(DisponibilidadOperario d);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
