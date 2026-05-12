using Etereo.Domain.Entities.Cupones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Cupones;

public class CuponConfiguration : IEntityTypeConfiguration<Cupon>
{
    public void Configure(EntityTypeBuilder<Cupon> b)
    {
        b.ToTable("cupones");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.Codigo).HasColumnName("codigo").HasMaxLength(50).IsRequired();
        b.Property(x => x.Descripcion).HasColumnName("descripcion").HasMaxLength(300);
        b.Property(x => x.TipoDescuento).HasColumnName("tipo_descuento").HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(x => x.Valor).HasColumnName("valor").HasColumnType("numeric(10,2)").IsRequired();
        b.Property(x => x.ServiciosIds).HasColumnName("servicios_ids").HasColumnType("integer[]");
        b.Property(x => x.FechaDesde).HasColumnName("fecha_desde").IsRequired();
        b.Property(x => x.FechaHasta).HasColumnName("fecha_hasta").IsRequired();
        b.Property(x => x.UsosMaximos).HasColumnName("usos_maximos");
        b.Property(x => x.UsosActuales).HasColumnName("usos_actuales").HasDefaultValue(0);
        b.Property(x => x.UnUsoPorCliente).HasColumnName("un_uso_por_cliente").HasDefaultValue(true);
        b.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en");
        b.HasIndex(x => x.Codigo).IsUnique();
    }
}
