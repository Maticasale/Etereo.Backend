using Etereo.Domain.Entities.Emails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Emails;

public class ConfiguracionEmailConfiguration : IEntityTypeConfiguration<ConfiguracionEmail>
{
    public void Configure(EntityTypeBuilder<ConfiguracionEmail> b)
    {
        b.ToTable("configuracion_email");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.RecordatorioDiasAntes).HasColumnName("recordatorio_dias_antes").HasDefaultValue(1);
        b.Property(x => x.PostturnoHorasDespues).HasColumnName("postturno_horas_despues").HasDefaultValue(2);
        b.Property(x => x.EmailsActivos).HasColumnName("emails_activos").HasDefaultValue(true);
        b.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en");
    }
}
