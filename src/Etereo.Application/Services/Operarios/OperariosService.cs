using Etereo.Application.Common;
using Etereo.Application.Interfaces.Operarios;
using Etereo.Domain.Entities.Auth;
using Etereo.Domain.Entities.Operarios;
using Etereo.Domain.Enums;
using Etereo.Shared.Auth;
using Etereo.Shared.Operarios;
using Microsoft.EntityFrameworkCore;

namespace Etereo.Application.Services.Operarios;

public class OperariosService : IOperariosService
{
    private readonly IOperariosDbContext _db;

    public OperariosService(IOperariosDbContext db) => _db = db;

    // ── Listar operarios ──────────────────────────────────────────────────────

    public async Task<Result<List<UsuarioDto>>> ListarAsync()
    {
        var operarios = await _db.Usuarios
            .Where(u => u.Rol == Rol.Operario)
            .OrderBy(u => u.Apellido).ThenBy(u => u.Nombre)
            .ToListAsync();

        return Result<List<UsuarioDto>>.Success(operarios.Select(ToUsuarioDto).ToList());
    }

    // ── Obtener operario ──────────────────────────────────────────────────────

    public async Task<Result<UsuarioDto>> ObtenerAsync(int id)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id && u.Rol == Rol.Operario);

        if (usuario is null)
            return Result<UsuarioDto>.Failure("OPERARIO_NO_ENCONTRADO", "Operario no encontrado.");

        return Result<UsuarioDto>.Success(ToUsuarioDto(usuario));
    }

    // ── Listar subservicios del operario ──────────────────────────────────────

    public async Task<Result<List<OperarioSubservicioDto>>> ListarSubserviciosAsync(int operarioId)
    {
        var existe = await _db.Usuarios.AnyAsync(u => u.Id == operarioId && u.Rol == Rol.Operario);
        if (!existe)
            return Result<List<OperarioSubservicioDto>>.Failure("OPERARIO_NO_ENCONTRADO", "Operario no encontrado.");

        var asignados  = await _db.OperarioSubservicios.Where(os => os.OperarioId == operarioId).ToListAsync();
        var subIds     = asignados.Select(os => os.SubservicioId).ToList();
        var subservicios = await _db.Subservicios.Where(s => subIds.Contains(s.Id)).ToListAsync();
        var servicios  = await _db.Servicios.ToListAsync();

        var dtos = asignados.Select(os =>
        {
            var sub = subservicios.FirstOrDefault(s => s.Id == os.SubservicioId);
            var svc = servicios.FirstOrDefault(s => s.Id == sub?.ServicioId);
            return new OperarioSubservicioDto
            {
                Id                 = os.Id,
                OperarioId         = os.OperarioId,
                SubservicioId      = os.SubservicioId,
                NombreSubservicio  = sub?.Nombre ?? string.Empty,
                NombreServicio     = svc?.Nombre ?? string.Empty,
                PorcentajeComision = os.PorcentajeComision
            };
        }).ToList();

        return Result<List<OperarioSubservicioDto>>.Success(dtos);
    }

    // ── Asignar subservicio ───────────────────────────────────────────────────

    public async Task<Result<OperarioSubservicioDto>> AsignarSubservicioAsync(int operarioId, AsignarSubservicioRequest req)
    {
        var existe = await _db.Usuarios.AnyAsync(u => u.Id == operarioId && u.Rol == Rol.Operario);
        if (!existe)
            return Result<OperarioSubservicioDto>.Failure("OPERARIO_NO_ENCONTRADO", "Operario no encontrado.");

        var subExiste = await _db.Subservicios.AnyAsync(s => s.Id == req.SubservicioId);
        if (!subExiste)
            return Result<OperarioSubservicioDto>.Failure("SUBSERVICIO_NO_ENCONTRADO", "Subservicio no encontrado.");

        var yaAsignado = await _db.OperarioSubservicios
            .AnyAsync(os => os.OperarioId == operarioId && os.SubservicioId == req.SubservicioId);
        if (yaAsignado)
            return Result<OperarioSubservicioDto>.Failure("YA_ASIGNADO", "El subservicio ya está asignado a este operario.");

        var os = new OperarioSubservicio
        {
            OperarioId         = operarioId,
            SubservicioId      = req.SubservicioId,
            PorcentajeComision = req.PorcentajeComision
        };

        _db.AddOperarioSubservicio(os);
        await _db.SaveChangesAsync();

        var sub = await _db.Subservicios.FirstOrDefaultAsync(s => s.Id == req.SubservicioId);
        var svc = sub is not null ? await _db.Servicios.FirstOrDefaultAsync(s => s.Id == sub.ServicioId) : null;

        return Result<OperarioSubservicioDto>.Success(new OperarioSubservicioDto
        {
            Id                 = os.Id,
            OperarioId         = os.OperarioId,
            SubservicioId      = os.SubservicioId,
            NombreSubservicio  = sub?.Nombre ?? string.Empty,
            NombreServicio     = svc?.Nombre ?? string.Empty,
            PorcentajeComision = os.PorcentajeComision
        });
    }

    // ── Actualizar comisión ───────────────────────────────────────────────────

    public async Task<Result<OperarioSubservicioDto>> ActualizarComisionAsync(int operarioId, int subservicioId, ActualizarComisionRequest req)
    {
        var os = await _db.OperarioSubservicios
            .FirstOrDefaultAsync(x => x.OperarioId == operarioId && x.SubservicioId == subservicioId);

        if (os is null)
            return Result<OperarioSubservicioDto>.Failure("ASIGNACION_NO_ENCONTRADA", "Asignación no encontrada.");

        os.PorcentajeComision = req.PorcentajeComision;
        await _db.SaveChangesAsync();

        var sub = await _db.Subservicios.FirstOrDefaultAsync(s => s.Id == subservicioId);
        var svc = sub is not null ? await _db.Servicios.FirstOrDefaultAsync(s => s.Id == sub.ServicioId) : null;

        return Result<OperarioSubservicioDto>.Success(new OperarioSubservicioDto
        {
            Id                 = os.Id,
            OperarioId         = os.OperarioId,
            SubservicioId      = os.SubservicioId,
            NombreSubservicio  = sub?.Nombre ?? string.Empty,
            NombreServicio     = svc?.Nombre ?? string.Empty,
            PorcentajeComision = os.PorcentajeComision
        });
    }

    // ── Quitar subservicio ────────────────────────────────────────────────────

    public async Task<Result<bool>> QuitarSubservicioAsync(int operarioId, int subservicioId)
    {
        var os = await _db.OperarioSubservicios
            .FirstOrDefaultAsync(x => x.OperarioId == operarioId && x.SubservicioId == subservicioId);

        if (os is null)
            return Result<bool>.Failure("ASIGNACION_NO_ENCONTRADA", "Asignación no encontrada.");

        _db.RemoveOperarioSubservicio(os);
        await _db.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    // ── Obtener vistas ────────────────────────────────────────────────────────

    public async Task<Result<OperarioVistasDto>> ObtenerVistasAsync(int operarioId)
    {
        var existe = await _db.Usuarios.AnyAsync(u => u.Id == operarioId && u.Rol == Rol.Operario);
        if (!existe)
            return Result<OperarioVistasDto>.Failure("OPERARIO_NO_ENCONTRADO", "Operario no encontrado.");

        var vistas = await _db.OperarioVistas.FirstOrDefaultAsync(v => v.OperarioId == operarioId);

        // Lazy-create si no existen
        if (vistas is null)
        {
            vistas = new OperarioVistas
            {
                OperarioId          = operarioId,
                VerMisTurnos        = true,
                VerMisComisiones    = true,
                VerMiCalificacion   = false,
                VerMisEstadisticas  = false,
                CreadoEn            = DateTime.UtcNow,
                ActualizadoEn       = DateTime.UtcNow
            };
            _db.AddOperarioVistas(vistas);
            await _db.SaveChangesAsync();
        }

        return Result<OperarioVistasDto>.Success(ToVistasDto(vistas));
    }

    // ── Actualizar vistas ─────────────────────────────────────────────────────

    public async Task<Result<OperarioVistasDto>> ActualizarVistasAsync(int operarioId, ActualizarVistasRequest req)
    {
        var existe = await _db.Usuarios.AnyAsync(u => u.Id == operarioId && u.Rol == Rol.Operario);
        if (!existe)
            return Result<OperarioVistasDto>.Failure("OPERARIO_NO_ENCONTRADO", "Operario no encontrado.");

        var vistas = await _db.OperarioVistas.FirstOrDefaultAsync(v => v.OperarioId == operarioId);

        if (vistas is null)
        {
            vistas = new OperarioVistas { OperarioId = operarioId, CreadoEn = DateTime.UtcNow };
            _db.AddOperarioVistas(vistas);
        }

        vistas.VerMisTurnos       = req.VerMisTurnos;
        vistas.VerMisComisiones   = req.VerMisComisiones;
        vistas.VerMiCalificacion  = req.VerMiCalificacion;
        vistas.VerMisEstadisticas = req.VerMisEstadisticas;
        vistas.ActualizadoEn      = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Result<OperarioVistasDto>.Success(ToVistasDto(vistas));
    }

    // ── Disponibilidad salón ─────────────────────────────────────────────────

    public async Task<Result<List<DisponibilidadSalonDto>>> ListarDisponibilidadSalonAsync()
    {
        var items   = await _db.DisponibilidadesSalon.OrderBy(d => d.Fecha).ToListAsync();
        var motivos = await _db.MotivosBloqueoSalon.ToListAsync();

        var dtos = items.Select(d => new DisponibilidadSalonDto
        {
            Id           = d.Id,
            Fecha        = d.Fecha,
            Salon        = d.Salon.ToString(),
            MotivoId     = d.MotivoId,
            NombreMotivo = motivos.FirstOrDefault(m => m.Id == d.MotivoId)?.Nombre ?? string.Empty,
            Descripcion  = d.Descripcion,
            CreadoPorId  = d.CreadoPorId
        }).ToList();

        return Result<List<DisponibilidadSalonDto>>.Success(dtos);
    }

    public async Task<Result<DisponibilidadSalonDto>> CrearDisponibilidadSalonAsync(CrearDisponibilidadSalonRequest req, int creadoPorId)
    {
        if (!Enum.TryParse<Salon>(req.Salon, true, out var salon))
            return Result<DisponibilidadSalonDto>.Failure("SALON_INVALIDO", "El valor de Salon es inválido. Use: Salon1, Salon2 o Ambos.");

        var motivoExiste = await _db.MotivosBloqueoSalon.AnyAsync(m => m.Id == req.MotivoId);
        if (!motivoExiste)
            return Result<DisponibilidadSalonDto>.Failure("MOTIVO_NO_ENCONTRADO", "Motivo de bloqueo no encontrado.");

        var d = new DisponibilidadSalon
        {
            Fecha       = req.Fecha,
            Salon       = salon,
            MotivoId    = req.MotivoId,
            Descripcion = req.Descripcion?.Trim(),
            CreadoPorId = creadoPorId,
            CreadoEn    = DateTime.UtcNow
        };

        _db.AddDisponibilidadSalon(d);
        await _db.SaveChangesAsync();

        var motivo = await _db.MotivosBloqueoSalon.FirstOrDefaultAsync(m => m.Id == req.MotivoId);
        return Result<DisponibilidadSalonDto>.Success(new DisponibilidadSalonDto
        {
            Id           = d.Id,
            Fecha        = d.Fecha,
            Salon        = d.Salon.ToString(),
            MotivoId     = d.MotivoId,
            NombreMotivo = motivo?.Nombre ?? string.Empty,
            Descripcion  = d.Descripcion,
            CreadoPorId  = d.CreadoPorId
        });
    }

    public async Task<Result<bool>> EliminarDisponibilidadSalonAsync(int id)
    {
        var d = await _db.DisponibilidadesSalon.FirstOrDefaultAsync(x => x.Id == id);

        if (d is null)
            return Result<bool>.Failure("DISPONIBILIDAD_NO_ENCONTRADA", "Bloqueo de salón no encontrado.");

        _db.RemoveDisponibilidadSalon(d);
        await _db.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    // ── Disponibilidad operario ───────────────────────────────────────────────

    public async Task<Result<List<DisponibilidadOperarioDto>>> ListarDisponibilidadOperarioAsync(int operarioId)
    {
        var existe = await _db.Usuarios.AnyAsync(u => u.Id == operarioId && u.Rol == Rol.Operario);
        if (!existe)
            return Result<List<DisponibilidadOperarioDto>>.Failure("OPERARIO_NO_ENCONTRADO", "Operario no encontrado.");

        var items = await _db.DisponibilidadesOperario
            .Where(d => d.OperarioId == operarioId)
            .OrderBy(d => d.Fecha)
            .ToListAsync();

        return Result<List<DisponibilidadOperarioDto>>.Success(items.Select(ToDisponibilidadOperarioDto).ToList());
    }

    public async Task<Result<DisponibilidadOperarioDto>> CrearDisponibilidadOperarioAsync(CrearDisponibilidadOperarioRequest req)
    {
        var existe = await _db.Usuarios.AnyAsync(u => u.Id == req.OperarioId && u.Rol == Rol.Operario);
        if (!existe)
            return Result<DisponibilidadOperarioDto>.Failure("OPERARIO_NO_ENCONTRADO", "Operario no encontrado.");

        var d = new DisponibilidadOperario
        {
            OperarioId     = req.OperarioId,
            Fecha          = req.Fecha,
            Trabaja        = req.Trabaja,
            MotivoAusencia = req.MotivoAusencia?.Trim(),
            CreadoEn       = DateTime.UtcNow
        };

        _db.AddDisponibilidadOperario(d);
        await _db.SaveChangesAsync();

        return Result<DisponibilidadOperarioDto>.Success(ToDisponibilidadOperarioDto(d));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static UsuarioDto ToUsuarioDto(Usuario u) => new()
    {
        Id                  = u.Id,
        Email               = u.Email,
        Nombre              = u.Nombre,
        Apellido            = u.Apellido,
        Telefono            = u.Telefono,
        Sexo                = u.Sexo.ToString(),
        Rol                 = u.Rol.ToString(),
        Estado              = u.Estado.ToString(),
        MotivoBloqueo       = u.MotivoBloqueo,
        DebeCambiarPassword = u.DebeCambiarPassword,
        AvatarUrl           = u.AvatarUrl,
        CreadoEn            = u.CreadoEn
    };

    private static OperarioVistasDto ToVistasDto(OperarioVistas v) => new()
    {
        Id                 = v.Id,
        OperarioId         = v.OperarioId,
        VerMisTurnos       = v.VerMisTurnos,
        VerMisComisiones   = v.VerMisComisiones,
        VerMiCalificacion  = v.VerMiCalificacion,
        VerMisEstadisticas = v.VerMisEstadisticas
    };

    private static DisponibilidadOperarioDto ToDisponibilidadOperarioDto(DisponibilidadOperario d) => new()
    {
        Id             = d.Id,
        OperarioId     = d.OperarioId,
        Fecha          = d.Fecha,
        Trabaja        = d.Trabaja,
        MotivoAusencia = d.MotivoAusencia
    };
}
