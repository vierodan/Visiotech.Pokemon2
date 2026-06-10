using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Visiotech.Pokemon.Infrastructure.Persistence;

public sealed class DatabaseInitializer(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<PokemonSeedOptions> seedOptions,
    ILogger<DatabaseInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PokemonDbContext>();

        if (string.Equals(dbContext.Database.ProviderName, "Microsoft.EntityFrameworkCore.Sqlite", StringComparison.Ordinal))
        {
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        }
        else
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        }

        if (!seedOptions.Value.ApplyMvpRoster)
        {
            return;
        }

        if (!await dbContext.PokemonSpecies.AnyAsync(cancellationToken))
        {
            await dbContext.PokemonSpecies.AddRangeAsync(PokemonMvpRosterSeed.GetSpecies(), cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded MVP pokemon species roster with {Count} records.", 10);
        }

        if (await dbContext.PokemonMoves.AnyAsync(cancellationToken))
        {
            return;
        }

        await dbContext.PokemonMoves.AddRangeAsync(PokemonMvpMoveSeed.GetMoves(), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded MVP pokemon move catalog with {Count} records.", 27);
    }
}
