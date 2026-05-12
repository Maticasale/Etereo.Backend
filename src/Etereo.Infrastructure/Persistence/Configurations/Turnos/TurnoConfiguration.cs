using Etereo.Domain.Entities.Turnos;
using Etereo.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Etereo.Infrastructure.Persistence.Configurations.Turnos;

public class TurnoConfiguration : IEntityTypeConfiguration<Turno>
{
    public void Configure(EntityTypeBuilder<Turno> b)
    {
        b.ToTable("turnos");
        b.HasKey(x => x.Id);
        b.Property(x => x.Id).HasColumnName("id").UseIdentityColumn();
        b.Property(x => x.Salon).HasColumnName("salon").HasConversion<string>().HasMaxLength(10).IsRequired();
        b.Property(x => x.ClienteId).HasColumnName("cliente_id");
        b.Property(x => x.NombreAnonimo).HasColumnName("nombre_anonimo").HasMaxLength(200);
        b.Property(x => x.TelefonoAnonimo).HasColumnName("telefono_anonimo").HasMaxLength(30);
        b.Property(x => x.OperarioId).HasColumnName("operario_id").IsRequired();
        b.Property(x => x.SubservicioId).HasColumnName("subservicio_id").IsRequired();
        b.Property(x => x.VarianteId).HasColumnName("variante_id");
        b.Property(x => x.SesionId).HasColumnName("sesion_id");
        b.Property(x => x.FechaHoraInicio).HasColumnName("fecha_hora_inicio").IsRequired();
        b.Property(x => x.DuracionMin).HasColumnName("duracion_min").IsRequired();
        b.Property(x => x.Estado).HasColumnName("estado").HasConversion<string>().HasMaxLength(30)
            .HasDefaultValue(EstadoTurno.PendienteConfirmacion);
        b.Property(x => x.MotivoRechazo).HasColumnName("motivo_rechazo").HasMaxLength(500);
        b.Property(x => x.PrecioBase).HasColumnName("precio_base").HasColumnType("numeric(10,2)").IsRequired();
        b.Property(x => x.PorcentajeDescuento).HasColumnName("porcentaje_descuento").HasColumnType("numeric(5,2)");
        b.Property(x => x.CuponId).HasColumnName("cupon_id");
        b.Property(x => x.PrecioFinal).HasColumnName("precio_final").HasColumnType("numeric(10,2)");
        b.Property(x => x.MetodoPagoId).HasColumnName("metodo_pago_id");
        b.Property(x => x.ComisionCalculada).HasColumnName("comision_calculada").HasColumnType("numeric(10,2)");
        b.Property(x => x.Notas).HasColumnName("notas").HasMaxLength(500);
        b.Property(x => x.IpOrigen).HasColumnName("ip_origen").HasMaxLength(45);
        b.Property(x => x.CreadoPorId).HasColumnName("creado_por_id");
        b.Property(x => x.CreadoEn).HasColumnName("creado_en");
        b.Property(x => x.ActualizadoEn).HasColumnName("actualizado_en");
        b.HasIndex(x => x.FechaHoraInicio);
        b.HasIndex(x => x.OperarioId);
        b.HasIndex(x => x.ClienteId);
        b.HasIndex(x => x.Estado);
        b.HasIndex(x => x.SesionId);
    }
}
