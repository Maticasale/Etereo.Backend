using Etereo.Application.Interfaces.Auth;
using Etereo.Application.Interfaces.Cupones;
using Etereo.Application.Interfaces.Emails;
using Etereo.Application.Interfaces.Imputaciones;
using Etereo.Application.Interfaces.Operarios;
using Etereo.Application.Interfaces.Servicios;
using Etereo.Application.Interfaces.Turnos;
using Etereo.Domain.Entities.Auth;
using Etereo.Domain.Entities.Cupones;
using Etereo.Domain.Entities.Emails;
using Etereo.Domain.Entities.Imputaciones;
using Etereo.Domain.Entities.Operarios;
using Etereo.Domain.Entities.Servicios;
using Etereo.Domain.Entities.Turnos;
using Microsoft.EntityFrameworkCore;

namespace Etereo.Infrastructure.Persistence;

public class AppDbContext : DbContext,
    IAuthDbContext,
    IServiciosDbContext,
    IOperariosDbContext,
    ITurnosDbContext,
    ICuponesDbContext,
    IImputacionesDbContext,
    IEmailsDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Auth
    public DbSet<Usuario> Usuarios { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; } = null!;
    public DbSet<DisponibilidadSalon> DisponibilidadesSalon { get; set; } = null!;
    public DbSet<DisponibilidadOperario> DisponibilidadesOperario { get; set; } = null!;
    public DbSet<OperarioVistas> OperarioVistas { get; set; } = null!;

    // Servicios
    public DbSet<Servicio> Servicios { get; set; } = null!;
    public DbSet<Subservicio> Subservicios { get; set; } = null!;
    public DbSet<VarianteSubservicio> VariantesSubservicio { get; set; } = null!;

    // Operarios
    public DbSet<OperarioSubservicio> OperarioSubservicios { get; set; } = null!;

    // Turnos
    public DbSet<Sesion> Sesiones { get; set; } = null!;
    public DbSet<Turno> Turnos { get; set; } = null!;
    public DbSet<ReglaDescuentoSesion> ReglasDescuentoSesion { get; set; } = null!;

    // Cupones
    public DbSet<Cupon> Cupones { get; set; } = null!;
    public DbSet<CuponUso> CuponUsos { get; set; } = null!;

    // Imputaciones
    public DbSet<CategoriaImputacion> CategoriasImputacion { get; set; } = null!;
    public DbSet<MetodoPago> MetodosPago { get; set; } = null!;
    public DbSet<MotivoBloqueoSalon> MotivosBloqueoSalon { get; set; } = null!;
    public DbSet<Imputacion> Imputaciones { get; set; } = null!;

    // Emails
    public DbSet<ConfiguracionEmail> ConfiguracionesEmail { get; set; } = null!;
    public DbSet<EmailEnviado> EmailsEnviados { get; set; } = null!;
    public DbSet<Calificacion> Calificaciones { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(mb);
    }

    // ── IAuthDbContext ─────────────────────────────────────────────
    IQueryable<Usuario>               IAuthDbContext.Usuarios                  => Usuarios.AsQueryable();
    IQueryable<RefreshToken>          IAuthDbContext.RefreshTokens             => RefreshTokens.AsQueryable();
    IQueryable<PasswordResetToken>    IAuthDbContext.PasswordResetTokens       => PasswordResetTokens.AsQueryable();
    IQueryable<DisponibilidadSalon>   IAuthDbContext.DisponibilidadesSalon     => DisponibilidadesSalon.AsQueryable();
    IQueryable<DisponibilidadOperario> IAuthDbContext.DisponibilidadesOperario => DisponibilidadesOperario.AsQueryable();
    IQueryable<OperarioVistas>        IAuthDbContext.OperarioVistas            => OperarioVistas.AsQueryable();

    void IAuthDbContext.AddUsuario(Usuario u)                   => Usuarios.Add(u);
    void IAuthDbContext.AddRefreshToken(RefreshToken t)         => RefreshTokens.Add(t);
    void IAuthDbContext.RemoveRefreshToken(RefreshToken t)      => RefreshTokens.Remove(t);
    void IAuthDbContext.AddPasswordResetToken(PasswordResetToken t) => PasswordResetTokens.Add(t);
    void IAuthDbContext.AddDisponibilidadSalon(DisponibilidadSalon d)   => DisponibilidadesSalon.Add(d);
    void IAuthDbContext.RemoveDisponibilidadSalon(DisponibilidadSalon d) => DisponibilidadesSalon.Remove(d);
    void IAuthDbContext.AddDisponibilidadOperario(DisponibilidadOperario d) => DisponibilidadesOperario.Add(d);
    void IAuthDbContext.AddOperarioVistas(OperarioVistas v)     => OperarioVistas.Add(v);

    // ── IServiciosDbContext ────────────────────────────────────────
    IQueryable<Servicio>          IServiciosDbContext.Servicios          => Servicios.AsQueryable();
    IQueryable<Subservicio>       IServiciosDbContext.Subservicios       => Subservicios.AsQueryable();
    IQueryable<VarianteSubservicio> IServiciosDbContext.VariantesSubservicio => VariantesSubservicio.AsQueryable();

    void IServiciosDbContext.AddServicio(Servicio s)                  => Servicios.Add(s);
    void IServiciosDbContext.AddSubservicio(Subservicio s)            => Subservicios.Add(s);
    void IServiciosDbContext.AddVarianteSubservicio(VarianteSubservicio v) => VariantesSubservicio.Add(v);

    // ── IOperariosDbContext ────────────────────────────────────────
    IQueryable<OperarioSubservicio> IOperariosDbContext.OperarioSubservicios => OperarioSubservicios.AsQueryable();

    void IOperariosDbContext.AddOperarioSubservicio(OperarioSubservicio os) => OperarioSubservicios.Add(os);
    void IOperariosDbContext.RemoveOperarioSubservicio(OperarioSubservicio os) => OperarioSubservicios.Remove(os);

    // ── ITurnosDbContext ───────────────────────────────────────────
    IQueryable<Sesion>               ITurnosDbContext.Sesiones             => Sesiones.AsQueryable();
    IQueryable<Turno>                ITurnosDbContext.Turnos               => Turnos.AsQueryable();
    IQueryable<ReglaDescuentoSesion> ITurnosDbContext.ReglasDescuentoSesion => ReglasDescuentoSesion.AsQueryable();

    void ITurnosDbContext.AddSesion(Sesion s) => Sesiones.Add(s);
    void ITurnosDbContext.AddTurno(Turno t)   => Turnos.Add(t);

    // ── ICuponesDbContext ──────────────────────────────────────────
    IQueryable<Cupon>    ICuponesDbContext.Cupones   => Cupones.AsQueryable();
    IQueryable<CuponUso> ICuponesDbContext.CuponUsos => CuponUsos.AsQueryable();

    void ICuponesDbContext.AddCupon(Cupon c)     => Cupones.Add(c);
    void ICuponesDbContext.AddCuponUso(CuponUso u) => CuponUsos.Add(u);

    // ── IImputacionesDbContext ─────────────────────────────────────
    IQueryable<CategoriaImputacion> IImputacionesDbContext.CategoriasImputacion  => CategoriasImputacion.AsQueryable();
    IQueryable<MetodoPago>          IImputacionesDbContext.MetodosPago            => MetodosPago.AsQueryable();
    IQueryable<MotivoBloqueoSalon>  IImputacionesDbContext.MotivosBloqueoSalon   => MotivosBloqueoSalon.AsQueryable();
    IQueryable<Imputacion>          IImputacionesDbContext.Imputaciones           => Imputaciones.AsQueryable();

    void IImputacionesDbContext.AddCategoriaImputacion(CategoriaImputacion c) => CategoriasImputacion.Add(c);
    void IImputacionesDbContext.AddMetodoPago(MetodoPago m)                   => MetodosPago.Add(m);
    void IImputacionesDbContext.AddMotivoBloqueoSalon(MotivoBloqueoSalon m)   => MotivosBloqueoSalon.Add(m);
    void IImputacionesDbContext.AddImputacion(Imputacion i)                   => Imputaciones.Add(i);

    // ── IEmailsDbContext ───────────────────────────────────────────
    IQueryable<ConfiguracionEmail> IEmailsDbContext.ConfiguracionesEmail => ConfiguracionesEmail.AsQueryable();
    IQueryable<EmailEnviado>       IEmailsDbContext.EmailsEnviados        => EmailsEnviados.AsQueryable();
    IQueryable<Calificacion>       IEmailsDbContext.Calificaciones        => Calificaciones.AsQueryable();

    void IEmailsDbContext.AddEmailEnviado(EmailEnviado e) => EmailsEnviados.Add(e);
    void IEmailsDbContext.AddCalificacion(Calificacion c) => Calificaciones.Add(c);
}
