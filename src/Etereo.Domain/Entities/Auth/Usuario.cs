using Etereo.Domain.Base;
using Etereo.Domain.Enums;

namespace Etereo.Domain.Entities.Auth;

public class Usuario : EntityBase
{
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public Sexo Sexo { get; set; } = Sexo.NoEspecifica;
    public Rol Rol { get; set; } = Rol.Cliente;
    public AuthProvider AuthProvider { get; set; } = AuthProvider.Local;
    public string? GoogleId { get; set; }
    public EstadoUsuario Estado { get; set; } = EstadoUsuario.Activo;
    public string? MotivoBloqueo { get; set; }
    public bool DebeCambiarPassword { get; set; } = false;
    public string? AvatarUrl { get; set; }
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
    public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;
}
