using Etereo.Domain.Base;

namespace Etereo.Domain.Entities.Auth;

public class RefreshToken : EntityBase
{
    public int UsuarioId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime ExpiraEn { get; set; }
    public bool Revocado { get; set; } = false;
    public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
}
