using Etereo.Application.Common;
using Etereo.Shared.Imputaciones;

namespace Etereo.Application.Interfaces.Imputaciones;

public interface IImputacionesService
{
    // Imputaciones
    Task<Result<List<ImputacionDto>>>  ListarAsync(DateOnly? fechaDesde, DateOnly? fechaHasta, string? tipo, int? operarioId);
    Task<Result<ResumenImputacionesDto>> ResumenAsync(DateOnly? fechaDesde, DateOnly? fechaHasta);
    Task<Result<ImputacionDto>>        CrearAsync(CrearImputacionRequest req, int cargadoPorId);
    Task<Result<ImputacionDto>>        ActualizarAsync(int id, ActualizarImputacionRequest req);
    Task<Result<bool>>                 EliminarAsync(int id);

    // Categorías de imputación
    Task<Result<List<CategoriaImputacionDto>>> ListarCategoriasAsync();
    Task<Result<CategoriaImputacionDto>>       CrearCategoriaAsync(CrearCategoriaImputacionRequest req);
    Task<Result<CategoriaImputacionDto>>       ActualizarCategoriaAsync(int id, ActualizarCategoriaImputacionRequest req);
    Task<Result<CategoriaImputacionDto>>       CambiarEstadoCategoriaAsync(int id, EstadoImputacionRequest req);

    // Métodos de pago
    Task<Result<List<MetodoPagoDto>>> ListarMetodosPagoAsync();
    Task<Result<MetodoPagoDto>>       CrearMetodoPagoAsync(CrearMetodoPagoRequest req);
    Task<Result<MetodoPagoDto>>       ActualizarMetodoPagoAsync(int id, ActualizarMetodoPagoRequest req);
    Task<Result<MetodoPagoDto>>       CambiarEstadoMetodoPagoAsync(int id, EstadoImputacionRequest req);

    // Motivos de bloqueo de salón
    Task<Result<List<MotivoBloqueoSalonDto>>> ListarMotivosBloqueoAsync();
    Task<Result<MotivoBloqueoSalonDto>>       CrearMotivoBloqueoAsync(CrearMotivoBloqueoRequest req);
    Task<Result<MotivoBloqueoSalonDto>>       ActualizarMotivoBloqueoAsync(int id, ActualizarMotivoBloqueoRequest req);
    Task<Result<MotivoBloqueoSalonDto>>       CambiarEstadoMotivoBloqueoAsync(int id, EstadoImputacionRequest req);
}
