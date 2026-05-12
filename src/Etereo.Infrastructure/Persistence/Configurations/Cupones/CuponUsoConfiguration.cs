using Etereo.Domain.Entities.Cupones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Cupones;

public class CuponUsoConfiguration : IEntityTypeConfiguration<CuponUso>
{
    public void Configure(EntityTypeBuilder<CuponUso> b)
    {
        b.ToTable("cupon_usos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.CuponId).HasColumnName("cupon_id").IsRequired();
        b.Property(x => x.ClienteId).HasColumnName("cliente_id").IsRequired();
        b.Property(x => x.TurnoId).HasColumnName("turno_id").IsRequired();
        b.Property(x => x.UsadoEn).HasColumnName("usado_en");
        b.HasIndex(x => new { x.CuponId, x.ClienteId });
    }
}
