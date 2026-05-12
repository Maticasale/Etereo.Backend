using Etereo.Domain.Entities.Cupones;

namespace Etereo.Application.Interfaces.Cupones;

public interface ICuponesDbContext
{
    IQueryable<Cupon> Cupones { get; }
    IQueryable<CuponUso> CuponUsos { get; }

    void AddCupon(Cupon c);
    void AddCuponUso(CuponUso u);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
