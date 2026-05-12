using Etereo.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Auth;

public class OperarioVistasConfiguration : IEntityTypeConfiguration<OperarioVistas>
{
    public void Configure(EntityTypeBuilder<OperarioVistas> b)
    {
        b.ToTable("operario_vistas");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.OperarioId).HasColumnName("operario_id").IsRequired();
        b.Property(x => x.VerMisTurnos).HasColumnName("ver_mis_turnos").HasDefaultValue(true);
        b.Property(x => x.VerMisComisiones).HasColumnName("ver_mis_comisiones").HasDefaultValue(true);
        b.Property(x => x.VerMiCalificacion).HasColumnName("ver_mi_calificacion").HasDefaultValue(false);
        b.Property(x => x.VerMisEstadisticas).HasColumnName("ver_mis_estadisticas").HasDefaultValue(false);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en");
        b.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en");
        b.HasIndex(x => x.OperarioId).IsUnique();
    }
}
