using Etereo.Application.Common;
using Etereo.Shared.Estadisticas;

namespace Etereo.Application.Interfaces.Estadisticas;

public interface IEstadisticasService
{
    // Módulo 9 — existentes
    Task<Result<ResumenEstadisticasDto>>          ResumenAsync();
    Task<Result<List<PuntoEvolucionDto>>>         EvolucionAsync(DateOnly fechaDesde, DateOnly fechaHasta, string agrupacion);
    Task<Result<List<ServicioRankingDto>>>         RankingServiciosAsync(DateOnly? fechaDesde, DateOnly? fechaHasta);
    Task<Result<List<OperariaEstadisticasDto>>>    EstadisticasOperariasAsync(DateOnly? fechaDesde, DateOnly? fechaHasta);
    Task<Result<List<OcupacionDiariaDto>>>         OcupacionAsync(DateOnly fechaDesde, DateOnly fechaHasta);

    // Módulo 10 — SOT 4.10 adicionales
    Task<Result<List<PuntoEvolucionDto>>>          IngresosEgresosAsync(DateOnly? fechaDesde, DateOnly? fechaHasta, string agrupacion);
    Task<Result<TurnosEstadisticasDto>>            TurnosAsync(DateOnly? fechaDesde, DateOnly? fechaHasta);
    Task<Result<CalificacionesEstadisticasDto>>    CalificacionesAsync(DateOnly? fechaDesde, DateOnly? fechaHasta);

    // Dashboard
    Task<Result<ResumenEstadisticasDto>>           KpisAsync();
    Task<Result<List<AlertaDashboardDto>>>         AlertasAsync();
    Task<Result<List<AgendaHoyItemDto>>>           AgendaHoyAsync(int? operarioId);

    // Comisiones
    Task<Result<List<ComisionDto>>>                ListarComisionesAsync(DateOnly? fechaDesde, DateOnly? fechaHasta, int? operarioId);
    Task<Result<ResumenComisionesDto>>             ResumenComisionesAsync(int operarioId, DateOnly? fechaDesde, DateOnly? fechaHasta);
}
