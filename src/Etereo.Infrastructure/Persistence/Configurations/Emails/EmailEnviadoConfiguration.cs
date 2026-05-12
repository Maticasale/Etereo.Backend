using Etereo.Domain.Entities.Emails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Emails;

public class EmailEnviadoConfiguration : IEntityTypeConfiguration<EmailEnviado>
{
    public void Configure(EntityTypeBuilder<EmailEnviado> b)
    {
        b.ToTable("emails_enviados");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.Tipo).HasColumnName("tipo").HasConversion<string>().HasMaxLength(50).IsRequired();
        b.Property(x => x.Destinatario).HasColumnName("destinatario").HasMaxLength(255).IsRequired();
        b.Property(x => x.TurnoId).HasColumnName("turno_id");
        b.Property(x => x.UsuarioId).HasColumnName("usuario_id");
        b.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(x => x.ErrorDetalle).HasColumnName("error_detalle").HasMaxLength(500);
        b.Property(x => x.EnviadoEn).HasColumnName("enviado_en");
        b.HasIndex(x => new { x.Tipo, x.TurnoId });
        b.HasIndex(x => new { x.Tipo, x.UsuarioId });
    }
}
