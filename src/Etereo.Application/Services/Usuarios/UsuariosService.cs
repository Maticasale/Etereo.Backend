using Etereo.Application.Common;
using Etereo.Application.Interfaces.Usuarios;
using Etereo.Domain.Entities.Auth;
using Etereo.Domain.Enums;
using Etereo.Shared.Auth;
using Etereo.Shared.Usuarios;
using Microsoft.EntityFrameworkCore;

namespace Etereo.Application.Services.Usuarios;

public class UsuariosService : IUsuariosService
{
    private readonly IUsuariosDbContext _db;

    public UsuariosService(IUsuariosDbContext db) => _db = db;

    // ── Listar todos ──────────────────────────────────────────────────────────

    public async Task<Result<UsuariosListResponse>> ListarAsync()
    {
        var usuarios = await _db.Usuarios
            .OrderBy(u => u.Apellido).ThenBy(u => u.Nombre)
            .ToListAsync();

        return Result<UsuariosListResponse>.Success(new UsuariosListResponse
        {
            Items = usuarios.Select(ToDto).ToList(),
            Total = usuarios.Count
        });
    }

    // ── Obtener por ID ────────────────────────────────────────────────────────

    public async Task<Result<UsuarioDto>> ObtenerAsync(int id)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

        if (usuario is null)
            return Result<UsuarioDto>.Failure("USUARIO_NO_ENCONTRADO", "Usuario no encontrado.");

        return Result<UsuarioDto>.Success(ToDto(usuario));
    }

    // ── Actualizar ────────────────────────────────────────────────────────────

    public async Task<Result<UsuarioDto>> ActualizarAsync(int id, ActualizarUsuarioRequest req)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

        if (usuario is null)
            return Result<UsuarioDto>.Failure("USUARIO_NO_ENCONTRADO", "Usuario no encontrado.");

        if (req.Nombre  is not null) usuario.Nombre   = req.Nombre.Trim();
        if (req.Apellido is not null) usuario.Apellido = req.Apellido.Trim();
        if (req.Telefono is not null) usuario.Telefono = req.Telefono.Trim();
        if (req.Sexo    is not null && Enum.TryParse<Sexo>(req.Sexo, true, out var sexo))
            usuario.Sexo = sexo;

        usuario.ActualizadoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Result<UsuarioDto>.Success(ToDto(usuario));
    }

    // ── Bloquear ──────────────────────────────────────────────────────────────

    public async Task<Result<bool>> BloquearAsync(int id, BloquearUsuarioRequest req)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

        if (usuario is null)
            return Result<bool>.Failure("USUARIO_NO_ENCONTRADO", "Usuario no encontrado.");

        if (usuario.Rol == Rol.Admin)
            return Result<bool>.Failure("NO_PERMITIDO", "No se puede bloquear al administrador.");

        usuario.Estado        = EstadoUsuario.Bloqueado;
        usuario.MotivoBloqueo = req.Motivo.Trim();
        usuario.ActualizadoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    // ── Desbloquear ───────────────────────────────────────────────────────────

    public async Task<Result<bool>> DesbloquearAsync(int id)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

        if (usuario is null)
            return Result<bool>.Failure("USUARIO_NO_ENCONTRADO", "Usuario no encontrado.");

        usuario.Estado        = EstadoUsuario.Activo;
        usuario.MotivoBloqueo = null;
        usuario.ActualizadoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    // ── Promover a Operario ───────────────────────────────────────────────────

    public async Task<Result<UsuarioDto>> PromoverOperarioAsync(int id)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

        if (usuario is null)
            return Result<UsuarioDto>.Failure("USUARIO_NO_ENCONTRADO", "Usuario no encontrado.");

        if (usuario.Rol == Rol.Admin)
            return Result<UsuarioDto>.Failure("NO_PERMITIDO", "El administrador no puede ser promovido.");

        if (usuario.Rol == Rol.Operario)
            return Result<UsuarioDto>.Failure("YA_ES_OPERARIO", "El usuario ya es operario.");

        usuario.Rol                = Rol.Operario;
        usuario.DebeCambiarPassword = true;
        usuario.ActualizadoEn      = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Result<UsuarioDto>.Success(ToDto(usuario));
    }

    // ── Degradar a Cliente ────────────────────────────────────────────────────

    public async Task<Result<UsuarioDto>> DegradarClienteAsync(int id)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

        if (usuario is null)
            return Result<UsuarioDto>.Failure("USUARIO_NO_ENCONTRADO", "Usuario no encontrado.");

        if (usuario.Rol == Rol.Admin)
            return Result<UsuarioDto>.Failure("NO_PERMITIDO", "El administrador no puede ser degradado.");

        if (usuario.Rol == Rol.Cliente)
            return Result<UsuarioDto>.Failure("YA_ES_CLIENTE", "El usuario ya es cliente.");

        usuario.Rol           = Rol.Cliente;
        usuario.ActualizadoEn = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Result<UsuarioDto>.Success(ToDto(usuario));
    }

    // ── Crear cliente (Admin/Operario crea directamente) ─────────────────────

    public async Task<Result<UsuarioDto>> CrearClienteAsync(CrearClienteRequest req)
    {
        // Email es opcional, pero si se provee no puede estar duplicado
        if (req.Email is not null)
        {
            var email = req.Email.Trim().ToLowerInvariant();
            if (await _db.Usuarios.AnyAsync(u => u.Email == email))
                return Result<UsuarioDto>.Failure("CREDENCIALES_EN_USO", "El email ya está registrado.");
        }

        var sexo = req.Sexo != null && Enum.TryParse<Sexo>(req.Sexo, true, out var s)
            ? s
            : Sexo.NoEspecifica;

        var usuario = new Usuario
        {
            Email         = req.Email?.Trim().ToLowerInvariant() ?? string.Empty,
            Nombre        = req.Nombre.Trim(),
            Apellido      = req.Apellido.Trim(),
            Telefono      = req.Telefono?.Trim(),
            Sexo          = sexo,
            Rol           = Rol.Cliente,
            AuthProvider  = AuthProvider.Local,
            Estado        = EstadoUsuario.Activo,
            CreadoEn      = DateTime.UtcNow,
            ActualizadoEn = DateTime.UtcNow
        };

        _db.AddUsuario(usuario);
        await _db.SaveChangesAsync();

        return Result<UsuarioDto>.Success(ToDto(usuario));
    }

    // ── Buscar clientes ───────────────────────────────────────────────────────

    public async Task<Result<List<UsuarioDto>>> BuscarClientesAsync(string? q)
    {
        var query = _db.Usuarios.Where(u => u.Rol == Rol.Cliente);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLowerInvariant();
            query = query.Where(u =>
                u.Nombre.ToLower().Contains(term)   ||
                u.Apellido.ToLower().Contains(term) ||
                (u.Email != null && u.Email.ToLower().Contains(term)) ||
                (u.Telefono != null && u.Telefono.Contains(term)));
        }

        var usuarios = await query
            .OrderBy(u => u.Apellido).ThenBy(u => u.Nombre)
            .Take(50)
            .ToListAsync();

        return Result<List<UsuarioDto>>.Success(usuarios.Select(ToDto).ToList());
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static UsuarioDto ToDto(Usuario u) => new()
    {
        Id                  = u.Id,
        Email               = string.IsNullOrEmpty(u.Email) ? null! : u.Email,
        Nombre              = u.Nombre,
        Apellido            = u.Apellido,
        Telefono            = u.Telefono,
        Rol                 = u.Rol.ToString(),
        Estado              = u.Estado.ToString(),
        MotivoBloqueo       = u.MotivoBloqueo,
        DebeCambiarPassword = u.DebeCambiarPassword,
        AvatarUrl           = u.AvatarUrl,
        CreadoEn            = u.CreadoEn
    };
}
