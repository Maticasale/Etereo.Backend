using Etereo.Application.Common;
using Etereo.Application.Interfaces.Imputaciones;
using Etereo.Domain.Entities.Imputaciones;
using Etereo.Domain.Enums;
using Etereo.Shared.Imputaciones;
using Microsoft.EntityFrameworkCore;

namespace Etereo.Application.Services.Imputaciones;

public class ImputacionesService : IImputacionesService
{
    private readonly IImputacionesDbContext _db;

    public ImputacionesService(IImputacionesDbContext db) => _db = db;

    // ── Listar imputaciones ───────────────────────────────────────────────────

    public async Task<Result<List<ImputacionDto>>> ListarAsync(
        DateOnly? fechaDesde, DateOnly? fechaHasta, string? tipo, int? operarioId)
    {
        var query = _db.Imputaciones.AsQueryable();

        if (fechaDesde.HasValue)
            query = query.Where(i => i.Fecha >= fechaDesde.Value);
        if (fechaHasta.HasValue)
            query = query.Where(i => i.Fecha <= fechaHasta.Value);
        if (!string.IsNullOrEmpty(tipo) && Enum.TryParse<TipoImputacion>(tipo, true, out var tipoEnum))
            query = query.Where(i => i.Tipo == tipoEnum);
        if (operarioId.HasValue)
            query = query.Where(i => i.OperarioId == operarioId.Value);

        var imputaciones = await query.OrderByDescending(i => i.Fecha).ThenByDescending(i => i.CreadoEn).ToListAsync();
        var cats = await _db.CategoriasImputacion.ToListAsync();
        var usuarios = await _db.Usuarios.ToListAsync();

        var dtos = imputaciones.Select(i =>
        {
            var cat = cats.FirstOrDefault(c => c.Id == i.CategoriaId);
            var op  = i.OperarioId.HasValue ? usuarios.FirstOrDefault(u => u.Id == i.OperarioId.Value) : null;
            return ToDto(i, cat?.Nombre ?? string.Empty, op is not null ? $"{op.Nombre} {op.Apellido}" : null);
        }).ToList();

        return Result<List<ImputacionDto>>.Success(dtos);
    }

    // ── Resumen ───────────────────────────────────────────────────────────────

    public async Task<Result<ResumenImputacionesDto>> ResumenAsync(DateOnly? fechaDesde, DateOnly? fechaHasta)
    {
        var query = _db.Imputaciones.AsQueryable();
        if (fechaDesde.HasValue) query = query.Where(i => i.Fecha >= fechaDesde.Value);
        if (fechaHasta.HasValue) query = query.Where(i => i.Fecha <= fechaHasta.Value);

        var imputaciones = await query.ToListAsync();
        var cats = await _db.CategoriasImputacion.ToListAsync();

        var totalIngresos = imputaciones.Where(i => i.Tipo == TipoImputacion.Ingreso).Sum(i => i.Monto);
        var totalEgresos  = imputaciones.Where(i => i.Tipo == TipoImputacion.Egreso).Sum(i => i.Monto);

        var porCategoria = imputaciones
            .GroupBy(i => i.CategoriaId)
            .Select(g =>
            {
                var cat = cats.FirstOrDefault(c => c.Id == g.Key);
                var tipo = g.First().Tipo;
                return new ResumenCategoriaDto
                {
                    NombreCategoria = cat?.Nombre ?? string.Empty,
                    Tipo  = tipo.ToString(),
                    Total = g.Sum(i => i.Monto)
                };
            })
            .OrderBy(r => r.Tipo)
            .ThenByDescending(r => r.Total)
            .ToList();

        return Result<ResumenImputacionesDto>.Success(new ResumenImputacionesDto
        {
            TotalIngresos = totalIngresos,
            TotalEgresos  = totalEgresos,
            PorCategoria  = porCategoria
        });
    }

    // ── Crear imputación manual ───────────────────────────────────────────────

    public async Task<Result<ImputacionDto>> CrearAsync(CrearImputacionRequest req, int cargadoPorId)
    {
        if (!Enum.TryParse<TipoImputacion>(req.Tipo, true, out var tipo))
            return Result<ImputacionDto>.Failure("TIPO_INVALIDO", "Tipo inválido. Use: Ingreso o Egreso.");

        var catExiste = await _db.CategoriasImputacion.AnyAsync(c => c.Id == req.CategoriaId && c.Activo);
        if (!catExiste)
            return Result<ImputacionDto>.Failure("CATEGORIA_NO_ENCONTRADA", "Categoría de imputación no encontrada.");

        var imp = new Imputacion
        {
            Fecha        = req.Fecha,
            Tipo         = tipo,
            CategoriaId  = req.CategoriaId,
            Descripcion  = req.Descripcion?.Trim(),
            Monto        = req.Monto,
            TurnoId      = req.TurnoId,
            OperarioId   = req.OperarioId,
            CargadoPorId = cargadoPorId,
            Origen       = OrigenImputacion.Manual,
            CreadoEn     = DateTime.UtcNow
        };

        _db.AddImputacion(imp);
        await _db.SaveChangesAsync();

        var cat = await _db.CategoriasImputacion.FirstOrDefaultAsync(c => c.Id == imp.CategoriaId);
        var op  = imp.OperarioId.HasValue ? await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == imp.OperarioId.Value) : null;

        return Result<ImputacionDto>.Success(
            ToDto(imp, cat?.Nombre ?? string.Empty, op is not null ? $"{op.Nombre} {op.Apellido}" : null));
    }

    // ── Actualizar imputación ─────────────────────────────────────────────────

    public async Task<Result<ImputacionDto>> ActualizarAsync(int id, ActualizarImputacionRequest req)
    {
        var imp = await _db.Imputaciones.FirstOrDefaultAsync(i => i.Id == id);
        if (imp is null)
            return Result<ImputacionDto>.Failure("IMPUTACION_NO_ENCONTRADA", "Imputación no encontrada.");

        if (imp.Origen == OrigenImputacion.Automatico)
            return Result<ImputacionDto>.Failure("NO_EDITABLE", "Las imputaciones automáticas no son editables.");

        if (req.CategoriaId.HasValue)
        {
            var catExiste = await _db.CategoriasImputacion.AnyAsync(c => c.Id == req.CategoriaId.Value && c.Activo);
            if (!catExiste)
                return Result<ImputacionDto>.Failure("CATEGORIA_NO_ENCONTRADA", "Categoría de imputación no encontrada.");
            imp.CategoriaId = req.CategoriaId.Value;
        }
        if (req.Fecha.HasValue)        imp.Fecha       = req.Fecha.Value;
        if (req.Descripcion is not null) imp.Descripcion = req.Descripcion.Trim();
        if (req.Monto.HasValue)        imp.Monto       = req.Monto.Value;
        if (req.OperarioId.HasValue)   imp.OperarioId  = req.OperarioId;

        await _db.SaveChangesAsync();

        var cat = await _db.CategoriasImputacion.FirstOrDefaultAsync(c => c.Id == imp.CategoriaId);
        var op  = imp.OperarioId.HasValue ? await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == imp.OperarioId.Value) : null;

        return Result<ImputacionDto>.Success(
            ToDto(imp, cat?.Nombre ?? string.Empty, op is not null ? $"{op.Nombre} {op.Apellido}" : null));
    }

    // ── Eliminar imputación ───────────────────────────────────────────────────

    public async Task<Result<bool>> EliminarAsync(int id)
    {
        var imp = await _db.Imputaciones.FirstOrDefaultAsync(i => i.Id == id);
        if (imp is null)
            return Result<bool>.Failure("IMPUTACION_NO_ENCONTRADA", "Imputación no encontrada.");

        if (imp.Origen == OrigenImputacion.Automatico)
            return Result<bool>.Failure("NO_ELIMINABLE", "Las imputaciones automáticas no son eliminables.");

        _db.RemoveImputacion(imp);
        await _db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    // ── Categorías ────────────────────────────────────────────────────────────

    public async Task<Result<List<CategoriaImputacionDto>>> ListarCategoriasAsync()
    {
        var cats = await _db.CategoriasImputacion.OrderBy(c => c.OrdenDisplay).ToListAsync();
        return Result<List<CategoriaImputacionDto>>.Success(cats.Select(ToCatDto).ToList());
    }

    public async Task<Result<CategoriaImputacionDto>> CrearCategoriaAsync(CrearCategoriaImputacionRequest req)
    {
        if (!Enum.TryParse<TipoCategoriaImp>(req.Tipo, true, out var tipo))
            return Result<CategoriaImputacionDto>.Failure("TIPO_INVALIDO", "Tipo inválido. Use: Ingreso, Egreso o Ambos.");

        var maxOrden = await _db.CategoriasImputacion.MaxAsync(c => (int?)c.OrdenDisplay) ?? -1;
        var cat = new CategoriaImputacion
        {
            Nombre      = req.Nombre.Trim(),
            Tipo        = tipo,
            Descripcion = req.Descripcion?.Trim(),
            Activo      = true,
            OrdenDisplay = maxOrden + 1,
            CreadoEn    = DateTime.UtcNow
        };
        _db.AddCategoriaImputacion(cat);
        await _db.SaveChangesAsync();
        return Result<CategoriaImputacionDto>.Success(ToCatDto(cat));
    }

    public async Task<Result<CategoriaImputacionDto>> ActualizarCategoriaAsync(int id, ActualizarCategoriaImputacionRequest req)
    {
        var cat = await _db.CategoriasImputacion.FirstOrDefaultAsync(c => c.Id == id);
        if (cat is null)
            return Result<CategoriaImputacionDto>.Failure("CATEGORIA_NO_ENCONTRADA", "Categoría no encontrada.");

        if (req.Nombre is not null)      cat.Nombre      = req.Nombre.Trim();
        if (req.Descripcion is not null) cat.Descripcion = req.Descripcion.Trim();
        await _db.SaveChangesAsync();
        return Result<CategoriaImputacionDto>.Success(ToCatDto(cat));
    }

    public async Task<Result<CategoriaImputacionDto>> CambiarEstadoCategoriaAsync(int id, EstadoImputacionRequest req)
    {
        var cat = await _db.CategoriasImputacion.FirstOrDefaultAsync(c => c.Id == id);
        if (cat is null)
            return Result<CategoriaImputacionDto>.Failure("CATEGORIA_NO_ENCONTRADA", "Categoría no encontrada.");
        cat.Activo = req.Activo;
        await _db.SaveChangesAsync();
        return Result<CategoriaImputacionDto>.Success(ToCatDto(cat));
    }

    // ── Métodos de pago ───────────────────────────────────────────────────────

    public async Task<Result<List<MetodoPagoDto>>> ListarMetodosPagoAsync()
    {
        var metodos = await _db.MetodosPago.OrderBy(m => m.OrdenDisplay).ToListAsync();
        return Result<List<MetodoPagoDto>>.Success(metodos.Select(ToMetodoDto).ToList());
    }

    public async Task<Result<MetodoPagoDto>> CrearMetodoPagoAsync(CrearMetodoPagoRequest req)
    {
        var maxOrden = await _db.MetodosPago.MaxAsync(m => (int?)m.OrdenDisplay) ?? -1;
        var m = new MetodoPago { Nombre = req.Nombre.Trim(), Activo = true, OrdenDisplay = maxOrden + 1, CreadoEn = DateTime.UtcNow };
        _db.AddMetodoPago(m);
        await _db.SaveChangesAsync();
        return Result<MetodoPagoDto>.Success(ToMetodoDto(m));
    }

    public async Task<Result<MetodoPagoDto>> ActualizarMetodoPagoAsync(int id, ActualizarMetodoPagoRequest req)
    {
        var m = await _db.MetodosPago.FirstOrDefaultAsync(x => x.Id == id);
        if (m is null) return Result<MetodoPagoDto>.Failure("METODO_PAGO_NO_ENCONTRADO", "Método de pago no encontrado.");
        m.Nombre = req.Nombre.Trim();
        await _db.SaveChangesAsync();
        return Result<MetodoPagoDto>.Success(ToMetodoDto(m));
    }

    public async Task<Result<MetodoPagoDto>> CambiarEstadoMetodoPagoAsync(int id, EstadoImputacionRequest req)
    {
        var m = await _db.MetodosPago.FirstOrDefaultAsync(x => x.Id == id);
        if (m is null) return Result<MetodoPagoDto>.Failure("METODO_PAGO_NO_ENCONTRADO", "Método de pago no encontrado.");
        m.Activo = req.Activo;
        await _db.SaveChangesAsync();
        return Result<MetodoPagoDto>.Success(ToMetodoDto(m));
    }

    // ── Motivos de bloqueo de salón ───────────────────────────────────────────

    public async Task<Result<List<MotivoBloqueoSalonDto>>> ListarMotivosBloqueoAsync()
    {
        var motivos = await _db.MotivosBloqueoSalon.OrderBy(m => m.OrdenDisplay).ToListAsync();
        return Result<List<MotivoBloqueoSalonDto>>.Success(motivos.Select(ToMotivoDto).ToList());
    }

    public async Task<Result<MotivoBloqueoSalonDto>> CrearMotivoBloqueoAsync(CrearMotivoBloqueoRequest req)
    {
        var maxOrden = await _db.MotivosBloqueoSalon.MaxAsync(m => (int?)m.OrdenDisplay) ?? -1;
        var m = new MotivoBloqueoSalon { Nombre = req.Nombre.Trim(), Activo = true, OrdenDisplay = maxOrden + 1, CreadoEn = DateTime.UtcNow };
        _db.AddMotivoBloqueoSalon(m);
        await _db.SaveChangesAsync();
        return Result<MotivoBloqueoSalonDto>.Success(ToMotivoDto(m));
    }

    public async Task<Result<MotivoBloqueoSalonDto>> ActualizarMotivoBloqueoAsync(int id, ActualizarMotivoBloqueoRequest req)
    {
        var m = await _db.MotivosBloqueoSalon.FirstOrDefaultAsync(x => x.Id == id);
        if (m is null) return Result<MotivoBloqueoSalonDto>.Failure("MOTIVO_NO_ENCONTRADO", "Motivo de bloqueo no encontrado.");
        m.Nombre = req.Nombre.Trim();
        await _db.SaveChangesAsync();
        return Result<MotivoBloqueoSalonDto>.Success(ToMotivoDto(m));
    }

    public async Task<Result<MotivoBloqueoSalonDto>> CambiarEstadoMotivoBloqueoAsync(int id, EstadoImputacionRequest req)
    {
        var m = await _db.MotivosBloqueoSalon.FirstOrDefaultAsync(x => x.Id == id);
        if (m is null) return Result<MotivoBloqueoSalonDto>.Failure("MOTIVO_NO_ENCONTRADO", "Motivo de bloqueo no encontrado.");
        m.Activo = req.Activo;
        await _db.SaveChangesAsync();
        return Result<MotivoBloqueoSalonDto>.Success(ToMotivoDto(m));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static ImputacionDto ToDto(Imputacion i, string nombreCat, string? nombreOp) => new()
    {
        Id              = i.Id,
        Fecha           = i.Fecha,
        Tipo            = i.Tipo.ToString(),
        CategoriaId     = i.CategoriaId,
        NombreCategoria = nombreCat,
        Descripcion     = i.Descripcion,
        Monto           = i.Monto,
        TurnoId         = i.TurnoId,
        OperarioId      = i.OperarioId,
        NombreOperario  = nombreOp,
        CargadoPorId    = i.CargadoPorId,
        Origen          = i.Origen.ToString(),
        CreadoEn        = i.CreadoEn
    };

    private static CategoriaImputacionDto ToCatDto(CategoriaImputacion c) => new()
    {
        Id = c.Id, Nombre = c.Nombre, Tipo = c.Tipo.ToString(),
        Descripcion = c.Descripcion, Activo = c.Activo, OrdenDisplay = c.OrdenDisplay
    };

    private static MetodoPagoDto ToMetodoDto(MetodoPago m) => new()
    {
        Id = m.Id, Nombre = m.Nombre, Activo = m.Activo, OrdenDisplay = m.OrdenDisplay
    };

    private static MotivoBloqueoSalonDto ToMotivoDto(MotivoBloqueoSalon m) => new()
    {
        Id = m.Id, Nombre = m.Nombre, Activo = m.Activo, OrdenDisplay = m.OrdenDisplay
    };
}
