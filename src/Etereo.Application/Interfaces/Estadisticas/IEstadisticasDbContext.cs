using Etereo.Domain.Entities.Auth;
using Etereo.Domain.Entities.Emails;
using Etereo.Domain.Entities.Imputaciones;
using Etereo.Domain.Entities.Servicios;
using Etereo.Domain.Entities.Turnos;

namespace Etereo.Application.Interfaces.Estadisticas;

public interface IEstadisticasDbContext
{
    IQueryable<Turno>              Turnos               { get; }
    IQueryable<Imputacion>         Imputaciones         { get; }
    IQueryable<Usuario>            Usuarios             { get; }
    IQueryable<Subservicio>        Subservicios         { get; }
    IQueryable<Servicio>           Servicios            { get; }
    IQueryable<CategoriaImputacion> CategoriasImputacion { get; }
    IQueryable<Calificacion>       Calificaciones       { get; }
}
