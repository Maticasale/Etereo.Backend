using Etereo.Domain.Entities.Imputaciones;
using Etereo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Imputaciones;

public class ImputacionConfiguration : IEntityTypeConfiguration<Imputacion>
{
    public void Configure(EntityTypeBuilder<Imputacion> b)
    {
        b.ToTable("imputaciones");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.Fecha).HasColumnName("fecha").IsRequired();
        b.Property(x => x.Tipo).HasColumnName("tipo").HasConversion<string>().HasMaxLength(10).IsRequired();
        b.Property(x => x.CategoriaId).HasColumnName("categoria_id").IsRequired();
        b.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
        b.Property(x => x.Monto).HasColumnName("monto").HasColumnType("numeric(12,2)").IsRequired();
        b.Property(x => x.TurnoId).HasColumnName("turno_id");
        b.Property(x => x.OperarioId).HasColumnName("operario_id");
        b.Property(x => x.CargadoPorId).HasColumnName("cargado_por_id").IsRequired();
        b.Property(x => x.Origen).HasColumnName("origen").HasConversion<string>().HasMaxLength(20)
            .HasDefaultValue(OrigenImputacion.Manual);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en");
        b.HasIndex(x => x.Fecha);
        b.HasIndex(x => new { x.Tipo, x.CategoriaId });
        b.HasIndex(x => x.TurnoId);
        b.HasIndex(x => x.OperarioId);
    }
}
