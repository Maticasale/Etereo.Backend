using Etereo.Domain.Entities.Cupones;
using Etereo.Domain.Entities.Auth;
using Etereo.Domain.Entities.Imputaciones;
using Etereo.Domain.Entities.Operarios;
using Etereo.Domain.Entities.Servicios;
using Etereo.Domain.Entities.Turnos;

namespace Etereo.Application.Interfaces.Turnos;

public interface ITurnosDbContext
{
    // Turnos
    IQueryable<Sesion>               Sesiones               { get; }
    IQueryable<Turno>                Turnos                 { get; }
    IQueryable<ReglaDescuentoSesion> ReglasDescuentoSesion  { get; }

    // Usuarios y operarios
    IQueryable<Usuario>              Usuarios               { get; }
    IQueryable<OperarioSubservicio>  OperarioSubservicios   { get; }

    // Servicios
    IQueryable<Subservicio>          Subservicios           { get; }
    IQueryable<Servicio>             Servicios              { get; }
    IQueryable<VarianteSubservicio>  VariantesSubservicio   { get; }

    // Disponibilidad
    IQueryable<DisponibilidadSalon>    DisponibilidadesSalon    { get; }
    IQueryable<DisponibilidadOperario> DisponibilidadesOperario { get; }

    // Cupones
    IQueryable<Cupon>    Cupones   { get; }
    IQueryable<CuponUso> CuponUsos { get; }

    // Imputaciones (para creación automática al realizar)
    IQueryable<Imputacion>          Imputaciones           { get; }
    IQueryable<CategoriaImputacion> CategoriasImputacion   { get; }
    IQueryable<MetodoPago>          MetodosPago            { get; }

    void AddSesion(Sesion s);
    void AddTurno(Turno t);
    void AddImputacion(Imputacion i);
    void AddCuponUso(CuponUso u);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
