using Etereo.Application.Common;
using Etereo.Application.Interfaces.Servicios;
using Etereo.Domain.Entities.Servicios;
using Etereo.Domain.Enums;
using Etereo.Shared.Servicios;
using Microsoft.EntityFrameworkCore;

namespace Etereo.Application.Services.Servicios;

public class ServiciosService : IServiciosService
{
    private readonly IServiciosDbContext _db;

    public ServiciosService(IServiciosDbContext db) => _db = db;

    // ── Listar servicios ──────────────────────────────────────────────────────

    public async Task<Result<List<ServicioDto>>> ListarAsync(SexoSubservicio? filtroSexo)
    {
        var servicios = await _db.Servicios
            .Where(s => s.Activo)
            .OrderBy(s => s.OrdenDisplay).ThenBy(s => s.Nombre)
            .ToListAsync();

        var subservicios = await _db.Subservicios
            .Where(s => s.Activo)
            .OrderBy(s => s.OrdenDisplay).ThenBy(s => s.Nombre)
            .ToListAsync();

        var variantes = await _db.VariantesSubservicio
            .Where(v => v.Activo)
            .OrderBy(v => v.OrdenDisplay).ThenBy(v => v.Nombre)
            .ToListAsync();

        var result = servicios
            .Select(s => ToServicioDto(s, subservicios, variantes, filtroSexo))
            .Where(s => s.Subservicios.Count > 0)
            .ToList();

        return Result<List<ServicioDto>>.Success(result);
    }

    // ── Estado de configuración ───────────────────────────────────────────────

    public async Task<Result<EstadoConfiguracionDto>> EstadoConfiguracionAsync()
    {
        var hayServiciosActivos = await _db.Servicios.AnyAsync(s => s.Activo);

        var haySubserviciosActivos = hayServiciosActivos &&
            await _db.Subservicios.AnyAsync(s => s.Activo);

        var dto = new EstadoConfiguracionDto
        {
            Configurado = haySubserviciosActivos,
            Mensaje     = haySubserviciosActivos
                ? "El sistema está correctamente configurado."
                : !hayServiciosActivos
                    ? "No hay servicios activos."
                    : "Hay servicios activos pero ningún subservicio activo."
        };

        return Result<EstadoConfiguracionDto>.Success(dto);
    }

    // ── Obtener servicio por ID ───────────────────────────────────────────────

    public async Task<Result<ServicioDto>> ObtenerAsync(int id, SexoSubservicio? filtroSexo)
    {
        var servicio = await _db.Servicios.FirstOrDefaultAsync(s => s.Id == id);

        if (servicio is null)
            return Result<ServicioDto>.Failure("SERVICIO_NO_ENCONTRADO", "Servicio no encontrado.");

        var subservicios = await _db.Subservicios
            .Where(s => s.ServicioId == id)
            .OrderBy(s => s.OrdenDisplay).ThenBy(s => s.Nombre)
            .ToListAsync();

        var variantes = await _db.VariantesSubservicio
            .Where(v => subservicios.Select(s => s.Id).Contains(v.SubservicioId))
            .OrderBy(v => v.OrdenDisplay).ThenBy(v => v.Nombre)
            .ToListAsync();

        return Result<ServicioDto>.Success(ToServicioDto(servicio, subservicios, variantes, filtroSexo));
    }

    // ── Crear servicio ────────────────────────────────────────────────────────

    public async Task<Result<ServicioDto>> CrearAsync(CrearServicioRequest req)
    {
        if (!Enum.TryParse<Salon>(req.Salon, true, out var salon))
            return Result<ServicioDto>.Failure("SALON_INVALIDO", "El valor de Salon es inválido. Use: Salon1, Salon2 o Ambos.");

        var servicio = new Servicio
        {
            Nombre                = req.Nombre.Trim(),
            Salon                 = salon,
            CategoriaImputacionId = req.CategoriaImputacionId,
            OrdenDisplay          = req.OrdenDisplay,
            Activo                = true,
            CreadoEn              = DateTime.UtcNow
        };

        _db.AddServicio(servicio);
        await _db.SaveChangesAsync();

        return Result<ServicioDto>.Success(ToServicioDto(servicio, [], [], null));
    }

    // ── Actualizar servicio ───────────────────────────────────────────────────

    public async Task<Result<ServicioDto>> ActualizarAsync(int id, ActualizarServicioRequest req)
    {
        var servicio = await _db.Servicios.FirstOrDefaultAsync(s => s.Id == id);

        if (servicio is null)
            return Result<ServicioDto>.Failure("SERVICIO_NO_ENCONTRADO", "Servicio no encontrado.");

        if (req.Nombre is not null) servicio.Nombre = req.Nombre.Trim();
        if (req.Salon is not null)
        {
            if (!Enum.TryParse<Salon>(req.Salon, true, out var salon))
                return Result<ServicioDto>.Failure("SALON_INVALIDO", "El valor de Salon es inválido.");
            servicio.Salon = salon;
        }
        if (req.CategoriaImputacionId.HasValue) servicio.CategoriaImputacionId = req.CategoriaImputacionId;
        if (req.OrdenDisplay.HasValue)          servicio.OrdenDisplay          = req.OrdenDisplay.Value;

        await _db.SaveChangesAsync();

        var subservicios = await _db.Subservicios.Where(s => s.ServicioId == id).ToListAsync();
        var varIds       = subservicios.Select(s => s.Id).ToList();
        var variantes    = await _db.VariantesSubservicio.Where(v => varIds.Contains(v.SubservicioId)).ToListAsync();

        return Result<ServicioDto>.Success(ToServicioDto(servicio, subservicios, variantes, null));
    }

    // ── Cambiar estado servicio ───────────────────────────────────────────────

    public async Task<Result<ServicioDto>> CambiarEstadoAsync(int id, bool activo)
    {
        var servicio = await _db.Servicios.FirstOrDefaultAsync(s => s.Id == id);

        if (servicio is null)
            return Result<ServicioDto>.Failure("SERVICIO_NO_ENCONTRADO", "Servicio no encontrado.");

        servicio.Activo = activo;
        await _db.SaveChangesAsync();

        var subservicios = await _db.Subservicios.Where(s => s.ServicioId == id).ToListAsync();
        var varIds       = subservicios.Select(s => s.Id).ToList();
        var variantes    = await _db.VariantesSubservicio.Where(v => varIds.Contains(v.SubservicioId)).ToListAsync();

        return Result<ServicioDto>.Success(ToServicioDto(servicio, subservicios, variantes, null));
    }

    // ── Crear subservicio ─────────────────────────────────────────────────────

    public async Task<Result<SubservicioDto>> CrearSubservicioAsync(CrearSubservicioRequest req)
    {
        var servicioExiste = await _db.Servicios.AnyAsync(s => s.Id == req.ServicioId);
        if (!servicioExiste)
            return Result<SubservicioDto>.Failure("SERVICIO_NO_ENCONTRADO", "Servicio no encontrado.");

        if (!Enum.TryParse<SexoSubservicio>(req.Sexo, true, out var sexo))
            return Result<SubservicioDto>.Failure("SEXO_INVALIDO", "El valor de Sexo es inválido. Use: Masculino, Femenino o Ambos.");

        var sub = new Subservicio
        {
            ServicioId       = req.ServicioId,
            Nombre           = req.Nombre.Trim(),
            Descripcion      = req.Descripcion?.Trim(),
            Precio           = req.Precio,
            DuracionMin      = req.DuracionMin,
            RequiereSilencio = req.RequiereSilencio,
            EsPack           = req.EsPack,
            DetallePack      = req.DetallePack?.Trim(),
            Sexo             = sexo,
            OrdenDisplay     = req.OrdenDisplay,
            Activo           = true,
            CreadoEn         = DateTime.UtcNow
        };

        _db.AddSubservicio(sub);
        await _db.SaveChangesAsync();

        return Result<SubservicioDto>.Success(ToSubservicioDto(sub, []));
    }

    // ── Actualizar subservicio ────────────────────────────────────────────────

    public async Task<Result<SubservicioDto>> ActualizarSubservicioAsync(int id, ActualizarSubservicioRequest req)
    {
        var sub = await _db.Subservicios.FirstOrDefaultAsync(s => s.Id == id);

        if (sub is null)
            return Result<SubservicioDto>.Failure("SUBSERVICIO_NO_ENCONTRADO", "Subservicio no encontrado.");

        if (req.Nombre is not null)      sub.Nombre      = req.Nombre.Trim();
        if (req.Descripcion is not null) sub.Descripcion = req.Descripcion.Trim();
        if (req.Precio.HasValue)         sub.Precio      = req.Precio;
        if (req.DuracionMin.HasValue)    sub.DuracionMin  = req.DuracionMin;
        if (req.RequiereSilencio.HasValue) sub.RequiereSilencio = req.RequiereSilencio.Value;
        if (req.EsPack.HasValue)         sub.EsPack      = req.EsPack.Value;
        if (req.DetallePack is not null) sub.DetallePack = req.DetallePack.Trim();
        if (req.OrdenDisplay.HasValue)   sub.OrdenDisplay = req.OrdenDisplay.Value;
        if (req.Sexo is not null)
        {
            if (!Enum.TryParse<SexoSubservicio>(req.Sexo, true, out var sexo))
                return Result<SubservicioDto>.Failure("SEXO_INVALIDO", "El valor de Sexo es inválido.");
            sub.Sexo = sexo;
        }

        await _db.SaveChangesAsync();

        var variantes = await _db.VariantesSubservicio.Where(v => v.SubservicioId == id).ToListAsync();
        return Result<SubservicioDto>.Success(ToSubservicioDto(sub, variantes));
    }

    // ── Cambiar estado subservicio ────────────────────────────────────────────

    public async Task<Result<SubservicioDto>> CambiarEstadoSubservicioAsync(int id, bool activo)
    {
        var sub = await _db.Subservicios.FirstOrDefaultAsync(s => s.Id == id);

        if (sub is null)
            return Result<SubservicioDto>.Failure("SUBSERVICIO_NO_ENCONTRADO", "Subservicio no encontrado.");

        sub.Activo = activo;
        await _db.SaveChangesAsync();

        var variantes = await _db.VariantesSubservicio.Where(v => v.SubservicioId == id).ToListAsync();
        return Result<SubservicioDto>.Success(ToSubservicioDto(sub, variantes));
    }

    // ── Crear variante ────────────────────────────────────────────────────────

    public async Task<Result<VarianteDto>> CrearVarianteAsync(int subservicioId, CrearVarianteRequest req)
    {
        var subExiste = await _db.Subservicios.AnyAsync(s => s.Id == subservicioId);
        if (!subExiste)
            return Result<VarianteDto>.Failure("SUBSERVICIO_NO_ENCONTRADO", "Subservicio no encontrado.");

        if (!Enum.TryParse<SexoSubservicio>(req.Sexo, true, out var sexo))
            return Result<VarianteDto>.Failure("SEXO_INVALIDO", "El valor de Sexo es inválido.");

        var variante = new VarianteSubservicio
        {
            SubservicioId = subservicioId,
            Nombre        = req.Nombre.Trim(),
            Precio        = req.Precio,
            DuracionMin   = req.DuracionMin,
            Sexo          = sexo,
            OrdenDisplay  = req.OrdenDisplay,
            Activo        = true,
            CreadoEn      = DateTime.UtcNow
        };

        _db.AddVarianteSubservicio(variante);
        await _db.SaveChangesAsync();

        return Result<VarianteDto>.Success(ToVarianteDto(variante));
    }

    // ── Actualizar variante ───────────────────────────────────────────────────

    public async Task<Result<VarianteDto>> ActualizarVarianteAsync(int subservicioId, int varianteId, ActualizarVarianteRequest req)
    {
        var variante = await _db.VariantesSubservicio
            .FirstOrDefaultAsync(v => v.Id == varianteId && v.SubservicioId == subservicioId);

        if (variante is null)
            return Result<VarianteDto>.Failure("VARIANTE_NO_ENCONTRADA", "Variante no encontrada.");

        if (req.Nombre is not null)     variante.Nombre      = req.Nombre.Trim();
        if (req.Precio.HasValue)        variante.Precio      = req.Precio.Value;
        if (req.DuracionMin.HasValue)   variante.DuracionMin  = req.DuracionMin.Value;
        if (req.OrdenDisplay.HasValue)  variante.OrdenDisplay = req.OrdenDisplay.Value;
        if (req.Sexo is not null)
        {
            if (!Enum.TryParse<SexoSubservicio>(req.Sexo, true, out var sexo))
                return Result<VarianteDto>.Failure("SEXO_INVALIDO", "El valor de Sexo es inválido.");
            variante.Sexo = sexo;
        }

        await _db.SaveChangesAsync();
        return Result<VarianteDto>.Success(ToVarianteDto(variante));
    }

    // ── Cambiar estado variante ───────────────────────────────────────────────

    public async Task<Result<VarianteDto>> CambiarEstadoVarianteAsync(int subservicioId, int varianteId, bool activo)
    {
        var variante = await _db.VariantesSubservicio
            .FirstOrDefaultAsync(v => v.Id == varianteId && v.SubservicioId == subservicioId);

        if (variante is null)
            return Result<VarianteDto>.Failure("VARIANTE_NO_ENCONTRADA", "Variante no encontrada.");

        variante.Activo = activo;
        await _db.SaveChangesAsync();
        return Result<VarianteDto>.Success(ToVarianteDto(variante));
    }

    // ── Listar reglas descuento ───────────────────────────────────────────────

    public async Task<Result<List<ReglaDescuentoDto>>> ListarReglasAsync()
    {
        var reglas    = await _db.ReglasDescuentoSesion.OrderBy(r => r.Id).ToListAsync();
        var servicios = await _db.Servicios.ToListAsync();

        var dtos = reglas.Select(r => new ReglaDescuentoDto
        {
            Id                  = r.Id,
            ServicioId          = r.ServicioId,
            NombreServicio      = servicios.FirstOrDefault(s => s.Id == r.ServicioId)?.Nombre ?? string.Empty,
            ZonasMinimas        = r.ZonasMinimas,
            PorcentajeDescuento = r.PorcentajeDescuento,
            Activo              = r.Activo
        }).ToList();

        return Result<List<ReglaDescuentoDto>>.Success(dtos);
    }

    // ── Actualizar regla descuento ────────────────────────────────────────────

    public async Task<Result<ReglaDescuentoDto>> ActualizarReglaAsync(int id, ActualizarReglaDescuentoRequest req)
    {
        var regla = await _db.ReglasDescuentoSesion.FirstOrDefaultAsync(r => r.Id == id);

        if (regla is null)
            return Result<ReglaDescuentoDto>.Failure("REGLA_NO_ENCONTRADA", "Regla de descuento no encontrada.");

        regla.ZonasMinimas        = req.ZonasMinimas;
        regla.PorcentajeDescuento = req.PorcentajeDescuento;
        regla.Activo              = req.Activo;
        regla.ActualizadoEn       = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var servicio = await _db.Servicios.FirstOrDefaultAsync(s => s.Id == regla.ServicioId);
        return Result<ReglaDescuentoDto>.Success(new ReglaDescuentoDto
        {
            Id                  = regla.Id,
            ServicioId          = regla.ServicioId,
            NombreServicio      = servicio?.Nombre ?? string.Empty,
            ZonasMinimas        = regla.ZonasMinimas,
            PorcentajeDescuento = regla.PorcentajeDescuento,
            Activo              = regla.Activo
        });
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ServicioDto ToServicioDto(
        Servicio servicio,
        List<Subservicio> subservicios,
        List<VarianteSubservicio> variantes,
        SexoSubservicio? filtroSexo)
    {
        var subsFiltrados = subservicios
            .Where(s => s.ServicioId == servicio.Id)
            .Where(s => filtroSexo == null || s.Sexo == filtroSexo || s.Sexo == SexoSubservicio.Ambos)
            .OrderBy(s => s.OrdenDisplay).ThenBy(s => s.Nombre)
            .Select(s => ToSubservicioDto(s, variantes.Where(v => v.SubservicioId == s.Id).ToList()))
            .ToList();

        return new ServicioDto
        {
            Id                    = servicio.Id,
            Nombre                = servicio.Nombre,
            Salon                 = servicio.Salon.ToString(),
            CategoriaImputacionId = servicio.CategoriaImputacionId,
            Activo                = servicio.Activo,
            OrdenDisplay          = servicio.OrdenDisplay,
            Subservicios          = subsFiltrados
        };
    }

    private static SubservicioDto ToSubservicioDto(Subservicio s, List<VarianteSubservicio> variantes) => new()
    {
        Id               = s.Id,
        ServicioId       = s.ServicioId,
        Nombre           = s.Nombre,
        Descripcion      = s.Descripcion,
        Precio           = s.Precio,
        DuracionMin      = s.DuracionMin,
        RequiereSilencio = s.RequiereSilencio,
        EsPack           = s.EsPack,
        DetallePack      = s.DetallePack,
        Sexo             = s.Sexo.ToString(),
        Activo           = s.Activo,
        OrdenDisplay     = s.OrdenDisplay,
        Variantes        = variantes.Select(ToVarianteDto).ToList()
    };

    private static VarianteDto ToVarianteDto(VarianteSubservicio v) => new()
    {
        Id           = v.Id,
        Nombre       = v.Nombre,
        Precio       = v.Precio,
        DuracionMin  = v.DuracionMin,
        Sexo         = v.Sexo.ToString(),
        Activo       = v.Activo,
        OrdenDisplay = v.OrdenDisplay
    };
}
