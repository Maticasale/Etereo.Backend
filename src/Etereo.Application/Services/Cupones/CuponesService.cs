using Etereo.Application.Common;
using Etereo.Application.Interfaces.Cupones;
using Etereo.Domain.Entities.Cupones;
using Etereo.Domain.Enums;
using Etereo.Shared.Cupones;
using Microsoft.EntityFrameworkCore;

namespace Etereo.Application.Services.Cupones;

public class CuponesService : ICuponesService
{
    private readonly ICuponesDbContext _db;

    public CuponesService(ICuponesDbContext db) => _db = db;

    // GET /cupones — Admin
    public async Task<Result<List<CuponDto>>> ListarAsync()
    {
        var cupones = await _db.Cupones
            .OrderByDescending(c => c.CreadoEn)
            .ToListAsync();

        return Result<List<CuponDto>>.Success(cupones.Select(ToDto).ToList());
    }

    // POST /cupones — Admin
    public async Task<Result<CuponDto>> CrearAsync(CrearCuponRequest req)
    {
        if (!Enum.TryParse<TipoDescuento>(req.TipoDescuento, true, out var tipo))
            return Result<CuponDto>.Failure("TIPO_DESCUENTO_INVALIDO", "TipoDescuento inválido. Use: Porcentaje o MontoFijo.");

        if (req.FechaHasta < req.FechaDesde)
            return Result<CuponDto>.Failure("FECHAS_INVALIDAS", "FechaHasta debe ser mayor o igual a FechaDesde.");

        var codigoOcupado = await _db.Cupones.AnyAsync(c => c.Codigo == req.Codigo.ToUpper().Trim());
        if (codigoOcupado)
            return Result<CuponDto>.Failure("CODIGO_EN_USO", "Ya existe un cupón con ese código.");

        var cupon = new Cupon
        {
            Codigo          = req.Codigo.ToUpper().Trim(),
            Descripcion     = req.Descripcion?.Trim(),
            TipoDescuento   = tipo,
            Valor           = req.Valor,
            ServiciosIds    = req.ServiciosIds,
            FechaDesde      = req.FechaDesde,
            FechaHasta      = req.FechaHasta,
            UsosMaximos     = req.UsosMaximos,
            UsosActuales    = 0,
            UnUsoPorCliente = req.UnUsoPorCliente,
            Activo          = true,
            CreadoEn        = DateTime.UtcNow
        };

        _db.AddCupon(cupon);
        await _db.SaveChangesAsync();
        return Result<CuponDto>.Success(ToDto(cupon));
    }

    // PUT /cupones/{id} — Admin
    public async Task<Result<CuponDto>> ActualizarAsync(int id, ActualizarCuponRequest req)
    {
        var cupon = await _db.Cupones.FirstOrDefaultAsync(c => c.Id == id);
        if (cupon is null)
            return Result<CuponDto>.Failure("CUPON_NO_ENCONTRADO", "Cupón no encontrado.");

        if (req.FechaDesde.HasValue && req.FechaHasta.HasValue && req.FechaHasta < req.FechaDesde)
            return Result<CuponDto>.Failure("FECHAS_INVALIDAS", "FechaHasta debe ser mayor o igual a FechaDesde.");

        if (req.Descripcion    is not null) cupon.Descripcion     = req.Descripcion.Trim();
        if (req.Valor          is not null) cupon.Valor           = req.Valor.Value;
        if (req.ServiciosIds   is not null) cupon.ServiciosIds    = req.ServiciosIds;
        if (req.FechaDesde     is not null) cupon.FechaDesde      = req.FechaDesde.Value;
        if (req.FechaHasta     is not null) cupon.FechaHasta      = req.FechaHasta.Value;
        if (req.UsosMaximos    is not null) cupon.UsosMaximos     = req.UsosMaximos;
        if (req.UnUsoPorCliente is not null) cupon.UnUsoPorCliente = req.UnUsoPorCliente.Value;

        await _db.SaveChangesAsync();
        return Result<CuponDto>.Success(ToDto(cupon));
    }

    // PATCH /cupones/{id}/estado — Admin
    public async Task<Result<CuponDto>> CambiarEstadoAsync(int id, EstadoCuponRequest req)
    {
        var cupon = await _db.Cupones.FirstOrDefaultAsync(c => c.Id == id);
        if (cupon is null)
            return Result<CuponDto>.Failure("CUPON_NO_ENCONTRADO", "Cupón no encontrado.");

        cupon.Activo = req.Activo;
        await _db.SaveChangesAsync();
        return Result<CuponDto>.Success(ToDto(cupon));
    }

    // GET /cupones/disponibles — Cliente
    public async Task<Result<List<CuponDto>>> DisponiblesAsync(int clienteId)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var cupones = await _db.Cupones
            .Where(c => c.Activo && c.FechaDesde <= hoy && c.FechaHasta >= hoy)
            .ToListAsync();

        // Filtrar: usos máximos no alcanzados
        cupones = cupones
            .Where(c => c.UsosMaximos == null || c.UsosActuales < c.UsosMaximos.Value)
            .ToList();

        // Filtrar: UnUsoPorCliente — excluir los que el cliente ya usó
        var cuponIds = cupones.Select(c => c.Id).ToList();
        var usadosPorCliente = await _db.CuponUsos
            .Where(u => u.ClienteId == clienteId && cuponIds.Contains(u.CuponId))
            .Select(u => u.CuponId)
            .ToListAsync();

        cupones = cupones
            .Where(c => !c.UnUsoPorCliente || !usadosPorCliente.Contains(c.Id))
            .ToList();

        return Result<List<CuponDto>>.Success(cupones.Select(ToDto).ToList());
    }

    // GET /cupones/validar/{codigo} — Cliente
    public async Task<Result<CuponDto>> ValidarAsync(string codigo, int clienteId)
    {
        var cupon = await _db.Cupones
            .FirstOrDefaultAsync(c => c.Codigo == codigo.ToUpper().Trim() && c.Activo);

        if (cupon is null)
            return Result<CuponDto>.Failure("CUPON_NO_ENCONTRADO", "Cupón no encontrado o inactivo.");

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        if (hoy < cupon.FechaDesde || hoy > cupon.FechaHasta)
            return Result<CuponDto>.Failure("CUPON_EXPIRADO", "El cupón no está vigente.");

        if (cupon.UsosMaximos.HasValue && cupon.UsosActuales >= cupon.UsosMaximos.Value)
            return Result<CuponDto>.Failure("CUPON_AGOTADO", "El cupón ha alcanzado el máximo de usos.");

        if (cupon.UnUsoPorCliente)
        {
            var yaUsado = await _db.CuponUsos
                .AnyAsync(u => u.CuponId == cupon.Id && u.ClienteId == clienteId);
            if (yaUsado)
                return Result<CuponDto>.Failure("CUPON_YA_USADO", "Ya usaste este cupón.");
        }

        return Result<CuponDto>.Success(ToDto(cupon));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static CuponDto ToDto(Cupon c) => new()
    {
        Id              = c.Id,
        Codigo          = c.Codigo,
        Descripcion     = c.Descripcion,
        TipoDescuento   = c.TipoDescuento.ToString(),
        Valor           = c.Valor,
        ServiciosIds    = c.ServiciosIds,
        FechaDesde      = c.FechaDesde,
        FechaHasta      = c.FechaHasta,
        UsosMaximos     = c.UsosMaximos,
        UsosActuales    = c.UsosActuales,
        UnUsoPorCliente = c.UnUsoPorCliente,
        Activo          = c.Activo,
        CreadoEn        = c.CreadoEn
    };
}
