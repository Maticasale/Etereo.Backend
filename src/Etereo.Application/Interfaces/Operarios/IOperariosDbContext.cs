using Etereo.Domain.Entities.Operarios;

namespace Etereo.Application.Interfaces.Operarios;

public interface IOperariosDbContext
{
    IQueryable<OperarioSubservicio> OperarioSubservicios { get; }

    void AddOperarioSubservicio(OperarioSubservicio os);
    void RemoveOperarioSubservicio(OperarioSubservicio os);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
