using Etereo.Domain.Entities.Turnos;
using Etereo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Turnos;

public class SesionConfiguration : IEntityTypeConfiguration<Sesion>
{
    public void Configure(EntityTypeBuilder<Sesion> b)
    {
        b.ToTable("sesiones");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.ClienteId).HasColumnName("cliente_id");
        b.Property(x => x.NombreAnonimo).HasColumnName("nombre_anonimo").HasMaxLength(200);
        b.Property(x => x.TelefonoAnonimo).HasColumnName("telefono_anonimo").HasMaxLength(30);
        b.Property(x => x.OperarioId).HasColumnName("operario_id").IsRequired();
        b.Property(x => x.Salon).HasColumnName("salon").HasConversion<string>().HasMaxLength(10).IsRequired();
        b.Property(x => x.FechaHoraInicio).HasColumnName("fecha_hora_inicio").IsRequired();
        b.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>().HasMaxLength(30)
            .HasDefaultValue(EstadoTurno.PendienteConfirmacion);
        b.Property(x => x.DescuentoAutoPct).HasColumnName("descuento_auto_pct").HasColumnType("numeric(5,2)");
        b.Property(x => x.CreadoEn).HasColumnName("creado_en");
    }
}
