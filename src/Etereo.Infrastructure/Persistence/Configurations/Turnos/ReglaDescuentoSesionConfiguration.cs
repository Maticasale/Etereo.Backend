using Etereo.Domain.Entities.Turnos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Turnos;

public class ReglaDescuentoSesionConfiguration : IEntityTypeConfiguration<ReglaDescuentoSesion>
{
    public void Configure(EntityTypeBuilder<ReglaDescuentoSesion> b)
    {
        b.ToTable("reglas_descuento_sesion");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.ServicioId).HasColumnName("servicio_id").IsRequired();
        b.Property(x => x.ZonasMinimas).HasColumnName("zonas_minimas").HasDefaultValue(3);
        b.Property(x => x.PorcentajeDescuento).HasColumnName("porcentaje_descuento").HasColumnType("numeric(5,2)").IsRequired();
        b.Property(x => x.Activo).HasColumnName("activo").HasDefaultValue(true);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en");
        b.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en");
        b.HasIndex(x => x.ServicioId).IsUnique();
    }
}
