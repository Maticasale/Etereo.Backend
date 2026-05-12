using Etereo.Domain.Entities.Imputaciones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Imputaciones;

public class MotivoBloqueoSalonConfiguration : IEntityTypeConfiguration<MotivoBloqueoSalon>
{
    public void Configure(EntityTypeBuilder<MotivoBloqueoSalon> b)
    {
        b.ToTable("motivos_bloqueo_salon");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        b.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        b.Property(x => x.OrdenDisplay).HasColumnName("orden_display").HasDefaultValue(0);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en");
        b.HasIndex(x => x.Nombre).IsUnique();
    }
}
