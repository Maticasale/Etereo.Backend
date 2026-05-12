using Etereo.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Auth;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("refresh_tokens");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.UsuarioId).HasColumnName("usuario_id").IsRequired();
        b.Property(x => x.TokenHash).HasColumnName("token_hash").HasMaxLength(255).IsRequired();
        b.Property(x => x.ExpiraEn).HasColumnName("expira_en").IsRequired();
        b.Property(x => x.Revocado).HasColumnName("revocado").HasDefaultValue(false);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en");
        b.HasIndex(x => x.TokenHash).IsUnique();
    }
}
