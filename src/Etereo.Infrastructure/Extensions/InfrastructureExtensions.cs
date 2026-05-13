using Etereo.Application.Interfaces.Auth;
using Etereo.Application.Interfaces.Cupones;
using Etereo.Application.Interfaces.Email;
using Etereo.Application.Interfaces.Emails;
using Etereo.Application.Interfaces.Estadisticas;
using Etereo.Application.Interfaces.Imputaciones;
using Etereo.Application.Interfaces.Operarios;
using Etereo.Application.Interfaces.Servicios;
using Etereo.Application.Interfaces.Turnos;
using Etereo.Application.Interfaces.Usuarios;
using Etereo.Application.Services.Cupones;
using Etereo.Application.Services.Emails;
using Etereo.Application.Services.Estadisticas;
using Etereo.Application.Services.Imputaciones;
using Etereo.Application.Services.Operarios;
using Etereo.Application.Services.Servicios;
using Etereo.Application.Services.Turnos;
using Etereo.Application.Services.Usuarios;
using Etereo.Infrastructure.Persistence;
using Etereo.Infrastructure.Services.Auth;
using Etereo.Infrastructure.Services.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Etereo.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config["DATABASE_URL"]
            ?? throw new InvalidOperationException("DATABASE_URL no configurada.");

        // ── EF Core ───────────────────────────────────────────────────────────
        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // ── IXxxDbContext → AppDbContext ──────────────────────────────────────
        services.AddScoped<IAuthDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IUsuariosDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IServiciosDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IOperariosDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ITurnosDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ICuponesDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IImputacionesDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IEmailsDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // ── Application services ──────────────────────────────────────────────
        services.AddScoped<IUsuariosService, UsuariosService>();
        services.AddScoped<IServiciosService, ServiciosService>();
        services.AddScoped<IOperariosService, OperariosService>();
        services.AddScoped<ITurnosService, TurnosService>();
        services.AddScoped<ICuponesService, CuponesService>();
        services.AddScoped<IImputacionesService, ImputacionesService>();
        services.AddScoped<IEmailsService, EmailsService>();
        services.AddScoped<IEstadisticasDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IEstadisticasService, EstadisticasService>();

        // ── Servicios de infraestructura ──────────────────────────────────────
        services.AddScoped<IJwtService, JwtService>();

        services.AddHttpClient<IEmailService, ResendEmailService>((sp, client) =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var key = cfg["RESEND_API_KEY"];
            if (!string.IsNullOrEmpty(key))
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");
        });

        // ── Seeder ────────────────────────────────────────────────────────────
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
