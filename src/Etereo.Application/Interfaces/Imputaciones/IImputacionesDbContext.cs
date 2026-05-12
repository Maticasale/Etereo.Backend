using Etereo.Domain.Entities.Imputaciones;

namespace Etereo.Application.Interfaces.Imputaciones;

public interface IImputacionesDbContext
{
    IQueryable<CategoriaImputacion> CategoriasImputacion { get; }
    IQueryable<MetodoPago> MetodosPago { get; }
    IQueryable<MotivoBloqueoSalon> MotivosBloqueoSalon { get; }
    IQueryable<Imputacion> Imputaciones { get; }

    void AddCategoriaImputacion(CategoriaImputacion c);
    void AddMetodoPago(MetodoPago m);
    void AddMotivoBloqueoSalon(MotivoBloqueoSalon m);
    void AddImputacion(Imputacion i);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
