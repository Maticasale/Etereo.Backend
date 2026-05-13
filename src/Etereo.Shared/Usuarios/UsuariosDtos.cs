using Etereo.Shared.Auth;

namespace Etereo.Shared.Usuarios;

// ── Requests ──────────────────────────────────────────────────────────────────

public record ActualizarUsuarioRequest(
    string? Nombre,
    string? Apellido,
    string? Telefono,
    string? Sexo
);

public record BloquearUsuarioRequest(string Motivo);

public record CrearClienteRequest(
    string Nombre,
    string Apellido,
    string? Email,
    string? Telefono,
    string? Sexo
);

// ── Responses ─────────────────────────────────────────────────────────────────

/// <summary>Lista paginada de usuarios (reutiliza UsuarioDto de Auth)</summary>
public class UsuariosListResponse
{
    public List<UsuarioDto> Items { get; set; } = [];
    public int Total            { get; set; }
}
