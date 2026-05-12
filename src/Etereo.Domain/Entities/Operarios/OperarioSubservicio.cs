using Etereo.Domain.Base;

namespace Etereo.Domain.Entities.Operarios;

public class OperarioSubservicio : EntityBase
{
    public int OperarioId { get; set; }
    public int SubservicioId { get; set; }
    public decimal PorcentajeComision { get; set; }
}
