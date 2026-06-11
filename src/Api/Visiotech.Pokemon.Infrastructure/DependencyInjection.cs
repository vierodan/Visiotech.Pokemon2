using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Visiotech.Pokemon.Application.Abstractions.Clock;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Abstractions.Randomization;
using Visiotech.Pokemon.Infrastructure.Clock;
using Visiotech.Pokemon.Infrastructure.Persistence;
using Visiotech.Pokemon.Infrastructure.Randomization;

namespace Visiotech.Pokemon.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<IDamageRandomProvider, DamageRandomProvider>();

        var persistenceSection = configuration.GetSection(PersistenceOptions.SectionName);
        services.Configure<PersistenceOptions>(options =>
        {
            options.Provider = persistenceSection[nameof(PersistenceOptions.Provider)];
            options.InMemoryDatabaseName = persistenceSection[nameof(PersistenceOptions.InMemoryDatabaseName)];
        });

        var provider = PersistenceProviderResolver.Resolve(configuration, environment);

        services.PostConfigure<PersistenceOptions>(options =>
        {
            options.Provider = provider.ToString();

            if (provider == PersistenceProvider.InMemory &&
                string.IsNullOrWhiteSpace(options.InMemoryDatabaseName))
            {
                options.InMemoryDatabaseName = "Pokemon2-development";
            }
        });

        services.Configure<PokemonSeedOptions>(options =>
        {
            options.ApplyMvpRoster = bool.TryParse(
                configuration[$"{PokemonSeedOptions.SectionName}:ApplyMvpRoster"],
                out var applyMvpRoster)
                && applyMvpRoster;
        });

        services.AddDbContext<PokemonDbContext>(options =>
        {
            if (provider == PersistenceProvider.InMemory)
            {
                var databaseName = configuration[$"{PersistenceOptions.SectionName}:InMemoryDatabaseName"];
                if (string.IsNullOrWhiteSpace(databaseName))
                {
                    databaseName = "Pokemon2-development";
                }

                options.UseInMemoryDatabase(databaseName);
                return;
            }

            var connectionString = configuration.GetConnectionString("Pokemon2Db");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'Pokemon2Db' is required when using PostgreSQL persistence.");
            }

            options.UseNpgsql(
                connectionString,
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "pokemon2"));
        });

        services.AddScoped<IPokemonSpeciesReadRepository, PokemonSpeciesRepository>();
        services.AddScoped<IPokemonMoveReadRepository, PokemonMoveRepository>();
        services.AddScoped<IBattleReadRepository, BattleRepository>();
        services.AddScoped<IBattleWriteRepository, BattleRepository>();
        services.AddScoped<IMyPokemonReadRepository, MyPokemonRepository>();
        services.AddScoped<IMyPokemonWriteRepository, MyPokemonRepository>();
        services.AddScoped<IMyPokemonDeletionDependencyChecker, MyPokemonDeletionDependencyChecker>();
        services.AddScoped<IPokemonMoveWriteRepository, PokemonMoveRepository>();
        services.AddScoped<IPokemonSpeciesWriteRepository, PokemonSpeciesRepository>();
        services.AddScoped<IPokemonMoveDeletionDependencyChecker, PokemonMoveDeletionDependencyChecker>();
        services.AddScoped<IPokemonSpeciesDeletionDependencyChecker, PokemonSpeciesDeletionDependencyChecker>();
        services.AddScoped<IUnitOfWork, EntityFrameworkUnitOfWork>();
        services.AddSingleton<DatabaseInitializer>();

        return services;
    }
}
