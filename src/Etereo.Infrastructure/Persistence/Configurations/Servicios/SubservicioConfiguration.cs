using Etereo.Domain.Entities.Servicios;
using Etereo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Servicios;

public class SubservicioConfiguration : IEntityTypeConfiguration<Subservicio>
{
    public void Configure(EntityTypeBuilder<Subservicio> b)
    {
        b.ToTable("subservicios");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.ServicioId).HasColumnName("servicio_id").IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(150).IsRequired();
        b.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
        b.Property(x => x.Precio).HasColumnName("precio").HasColumnType("numeric(10,2)");
        b.Property(x => x.DuracionMin).HasColumnName("duracion_min");
        b.Property(x => x.RequiereSilencio).HasColumnName("requiere_silencio").HasDefaultValue(false);
        b.Property(x => x.EsPack).HasColumnName("es_pack").HasDefaultValue(false);
        b.Property(x => x.DetallePack).HasColumnName("detalle_pack").HasMaxLength(500);
        b.Property(x => x.Sexo).HasColumnName("sexo").HasConversion<string>().HasMaxLength(15)
            .HasDefaultValue(SexoSubservicio.Ambos);
        b.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        b.Property(x => x.OrdenDisplay).HasColumnName("orden_display").HasDefaultValue(0);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en");
        b.HasIndex(x => new { x.ServicioId, x.Nombre }).IsUnique();
    }
}
