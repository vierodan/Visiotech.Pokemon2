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

            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "pokemon2"));
        });

        services.AddScoped<IPokemonSpeciesReadRepository, PokemonSpeciesRepository>();
        services.AddScoped<IPokemonMoveReadRepository, PokemonMoveRepository>();
        services.AddScoped<IMyPokemonWriteRepository, MyPokemonRepository>();
        services.AddScoped<IPokemonMoveWriteRepository, PokemonMoveRepository>();
        services.AddScoped<IPokemonSpeciesWriteRepository, PokemonSpeciesRepository>();
        services.AddScoped<IPokemonMoveDeletionDependencyChecker, PokemonMoveDeletionDependencyChecker>();
        services.AddScoped<IPokemonSpeciesDeletionDependencyChecker, PokemonSpeciesDeletionDependencyChecker>();
        services.AddScoped<IUnitOfWork, EntityFrameworkUnitOfWork>();
        services.AddSingleton<DatabaseInitializer>();

        return services;
    }
}
