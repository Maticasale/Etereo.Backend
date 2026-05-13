using Etereo.Application.Common;
using Etereo.Domain.Enums;
using Etereo.Shared.Servicios;

namespace Etereo.Application.Interfaces.Servicios;

public interface IServiciosService
{
    // Servicios
    Task<Result<List<ServicioDto>>>       ListarAsync(SexoSubservicio? filtroSexo);
    Task<Result<EstadoConfiguracionDto>>  EstadoConfiguracionAsync();
    Task<Result<ServicioDto>>             ObtenerAsync(int id, SexoSubservicio? filtroSexo);
    Task<Result<ServicioDto>>             CrearAsync(CrearServicioRequest req);
    Task<Result<ServicioDto>>             ActualizarAsync(int id, ActualizarServicioRequest req);
    Task<Result<ServicioDto>>             CambiarEstadoAsync(int id, bool activo);

    // Subservicios
    Task<Result<SubservicioDto>>          CrearSubservicioAsync(CrearSubservicioRequest req);
    Task<Result<SubservicioDto>>          ActualizarSubservicioAsync(int id, ActualizarSubservicioRequest req);
    Task<Result<SubservicioDto>>          CambiarEstadoSubservicioAsync(int id, bool activo);

    // Variantes
    Task<Result<VarianteDto>>             CrearVarianteAsync(int subservicioId, CrearVarianteRequest req);
    Task<Result<VarianteDto>>             ActualizarVarianteAsync(int subservicioId, int varianteId, ActualizarVarianteRequest req);
    Task<Result<VarianteDto>>             CambiarEstadoVarianteAsync(int subservicioId, int varianteId, bool activo);

    // Reglas descuento
    Task<Result<List<ReglaDescuentoDto>>> ListarReglasAsync();
    Task<Result<ReglaDescuentoDto>>       ActualizarReglaAsync(int id, ActualizarReglaDescuentoRequest req);
}
