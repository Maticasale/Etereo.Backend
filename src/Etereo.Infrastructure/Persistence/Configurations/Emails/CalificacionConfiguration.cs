using Etereo.Domain.Entities.Emails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Emails;

public class CalificacionConfiguration : IEntityTypeConfiguration<Calificacion>
{
    public void Configure(EntityTypeBuilder<Calificacion> b)
    {
        b.ToTable("calificaciones");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.TurnoId).HasColumnName("turno_id").IsRequired();
        b.Property(x => x.ClienteId).HasColumnName("cliente_id").IsRequired();
        b.Property(x => x.OperarioId).HasColumnName("operario_id").IsRequired();
        b.Property(x => x.Puntuacion).HasColumnName("puntuacion").IsRequired();
        b.Property(x => x.Comentario).HasColumnName("comentario").HasMaxLength(1000);
        b.Property(x => x.CreadoEn).HasColumnName("creado_en");
        b.HasIndex(x => x.TurnoId).IsUnique();
    }
}
