using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Visiotech.Pokemon.Application.Abstractions.Clock;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Infrastructure.Clock;
using Visiotech.Pokemon.Infrastructure.Persistence;

namespace Visiotech.Pokemon.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.Configure<PokemonSeedOptions>(options =>
        {
            options.ApplyMvpRoster = bool.TryParse(
                configuration[$"{PokemonSeedOptions.SectionName}:ApplyMvpRoster"],
                out var applyMvpRoster)
                && applyMvpRoster;
        });

        services.AddDbContext<PokemonDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("Pokemon2Db");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'Pokemon2Db' is required.");
            }

            var provider = configuration["Persistence:Provider"];
            if (string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                options.UseSqlite(connectionString);
                return;
            }

            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IPokemonSpeciesReadRepository, PokemonSpeciesRepository>();
        services.AddScoped<IPokemonSpeciesWriteRepository, PokemonSpeciesRepository>();
        services.AddScoped<IUnitOfWork, EntityFrameworkUnitOfWork>();
        services.AddSingleton<DatabaseInitializer>();

        return services;
    }
}
