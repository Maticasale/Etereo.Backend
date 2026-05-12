using Etereo.Domain.Entities.Auth;
using Etereo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Auth;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("usuarios");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
        b.Property(x => x.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        b.Property(x => x.Apellido).HasColumnName("apellido").HasMaxLength(100).IsRequired();
        b.Property(x => x.Telefono).HasColumnName("telefono").HasMaxLength(30);
        b.Property(x => x.Sexo).HasColumnName("sexo").HasConversion<string>().HasMaxLength(15)
            .HasDefaultValue(Sexo.NoEspecifica);
        b.Property(x => x.Rol).HasColumnName("rol").HasConversion<string>().HasMaxLength(20)
            .HasDefaultValue(Rol.Cliente);
        b.Property(x => x.AuthProvider).HasColumnName("auth_provider").HasConversion<string>().HasMaxLength(20)
            .HasDefaultValue(AuthProvider.Local);
        b.Property(x => x.GoogleId).HasColumnName("google_id").HasMaxLength(100);
        b.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>().HasMaxLength(30)
            .HasDefaultValue(EstadoUsuario.Activo);
        b.Property(x => x.MotivoBloqueo).HasColumnName("motivo_bloqueo").HasMaxLength(500);
        b.Property(x => x.DebeCambiarPassword).HasColumnName("debe_cambiar_password").HasDefaultValue(false);
        b.Property(x => x.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(500);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en");
        b.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en");
        b.HasIndex(x => x.Email).IsUnique();
        b.HasIndex(x => x.GoogleId).IsUnique();
    }
}
