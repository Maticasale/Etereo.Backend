using Etereo.Domain.Entities.Servicios;
using Etereo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Servicios;

public class VarianteSubservicioConfiguration : IEntityTypeConfiguration<VarianteSubservicio>
{
    public void Configure(EntityTypeBuilder<VarianteSubservicio> b)
    {
        b.ToTable("variantes_subservicio");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.SubservicioId).HasColumnName("subservicio_id").IsRequired();
        b.Property(x => x.Nombre).HasColumnName("nombre").HasMaxLength(150).IsRequired();
        b.Property(x => x.Precio).HasColumnName("precio").HasColumnType("numeric(10,2)").IsRequired();
        b.Property(x => x.DuracionMin).HasColumnName("duracion_min").IsRequired();
        b.Property(x => x.Sexo).HasColumnName("sexo").HasConversion<string>().HasMaxLength(15);
        b.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        b.Property(x => x.OrdenDisplay).HasColumnName("orden_display").HasDefaultValue(0);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en");
        b.HasIndex(x => new { x.SubservicioId, x.Nombre }).IsUnique();
    }
}
