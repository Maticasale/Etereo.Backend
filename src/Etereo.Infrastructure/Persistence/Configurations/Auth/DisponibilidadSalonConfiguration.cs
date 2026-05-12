using Etereo.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Auth;

public class DisponibilidadSalonConfiguration : IEntityTypeConfiguration<DisponibilidadSalon>
{
    public void Configure(EntityTypeBuilder<DisponibilidadSalon> b)
    {
        b.ToTable("disponibilidad_salon");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Salon).HasColumnName("salon").HasConversion<string>().HasMaxLength(10).IsRequired();
        b.Property(x => x.MotivoId).HasColumnName("motivo_id").IsRequired();
        b.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300);
        b.Property(x => x.CreadoPorId).HasColumnName("creado_por_id").IsRequired();
        b.Property(x => x.CreadoEn).HasColumnName("creado_en");
        b.HasIndex(x => x.Fecha).IsUnique();
    }
}
