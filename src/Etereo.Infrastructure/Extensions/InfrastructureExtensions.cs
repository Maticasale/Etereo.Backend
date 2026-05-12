using Etereo.Application.Interfaces.Auth;
using Etereo.Application.Interfaces.Cupones;
using Etereo.Application.Interfaces.Emails;
using Etereo.Application.Interfaces.Imputaciones;
using Etereo.Application.Interfaces.Operarios;
using Etereo.Application.Interfaces.Servicios;
using Etereo.Application.Interfaces.Turnos;
using Etereo.Infrastructure.Persistence;
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

        services.AddDbContext<AppDbContext>(opt =>
            opt.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IAuthDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IServiciosDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IOperariosDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ITurnosDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<ICuponesDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IImputacionesDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IEmailsDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
