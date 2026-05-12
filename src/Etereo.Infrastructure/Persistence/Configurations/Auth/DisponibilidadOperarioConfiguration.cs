using Etereo.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Auth;

public class DisponibilidadOperarioConfiguration : IEntityTypeConfiguration<DisponibilidadOperario>
{
    public void Configure(EntityTypeBuilder<DisponibilidadOperario> b)
    {
        b.ToTable("disponibilidad_operario");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.OperarioId).HasColumnName("operario_id").IsRequired();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Trabaja).HasColumnName("trabaja").HasDefaultValue(true);
        b.Property(x => x.MotivoAusencia).HasColumnName("motivo_ausencia").HasMaxLength(200);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en");
        b.HasIndex(x => new { x.OperarioId, x.Fecha }).IsUnique();
    }
}
