using Etereo.Domain.Entities.Servicios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Servicios;

public class ServicioConfiguration : IEntityTypeConfiguration<Servicio>
{
    public void Configure(EntityTypeBuilder<Servicio> b)
    {
        b.ToTable("servicios");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
        b.Property(x => x.Salon).HasColumnName("salon").HasConversion<string>().HasMaxLength(10).IsRequired();
        b.Property(x => x.CategoriaImputacionId).HasColumnName("categoria_imputacion_id");
        b.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        b.Property(x => x.OrdenDisplay).HasColumnName("orden_display").HasDefaultValue(0);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en");
        b.HasIndex(x => x.Nombre).IsUnique();
    }
}
