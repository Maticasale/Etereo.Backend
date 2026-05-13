using Etereo.Application.Common;
using Etereo.Shared.Estadisticas;

namespace Etereo.Application.Interfaces.Estadisticas;

public interface IEstadisticasService
{
    Task<Result<ResumenEstadisticasDto>>          ResumenAsync();
    Task<Result<List<PuntoEvolucionDto>>>         EvolucionAsync(DateOnly fechaDesde, DateOnly fechaHasta, string agrupacion);
    Task<Result<List<ServicioRankingDto>>>         RankingServiciosAsync(DateOnly? fechaDesde, DateOnly? fechaHasta);
    Task<Result<List<OperariaEstadisticasDto>>>    EstadisticasOperariasAsync(DateOnly? fechaDesde, DateOnly? fechaHasta);
    Task<Result<List<OcupacionDiariaDto>>>         OcupacionAsync(DateOnly fechaDesde, DateOnly fechaHasta);
}
