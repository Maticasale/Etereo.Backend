using Etereo.Domain.Entities.Operarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Operarios;

public class OperarioSubservicioConfiguration : IEntityTypeConfiguration<OperarioSubservicio>
{
    public void Configure(EntityTypeBuilder<OperarioSubservicio> b)
    {
        b.ToTable("operario_subservicios");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.OperarioId).HasColumnName("operario_id").IsRequired();
        b.Property(x => x.SubservicioId).HasColumnName("subservicio_id").IsRequired();
        b.Property(x => x.PorcentajeComision).HasColumnName("porcentaje_comision").HasColumnType("numeric(4,2)").IsRequired();
        b.HasIndex(x => new { x.OperarioId, x.SubservicioId }).IsUnique();
    }
}
