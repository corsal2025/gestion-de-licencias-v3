using GestionLicencias.Core.Domain.Entities;
using GestionLicencias.Core.Domain.Interfaces;
using GestionLicencias.Infrastructure.Persistence;
using GestionLicencias.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestionLicencias.Infrastructure.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddGestionLicenciasInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=gestionlicencias.db";

        services.AddDbContext<GestionLicenciasDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<TramiteLicenciaService>();
        services.AddScoped<ITramiteService<TramiteLicencia>>(sp =>
            sp.GetRequiredService<TramiteLicenciaService>());

        return services;
    }
}
