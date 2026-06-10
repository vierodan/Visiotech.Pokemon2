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

        logger.LogInformation(
            "Initializing persistence with provider {Provider}. ApplyMvpRoster: {ApplyMvpRoster}",
            dbContext.Database.ProviderName,
            seedOptions.Value.ApplyMvpRoster);

        logger.LogInformation("Applying pending Entity Framework migrations.");
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (!seedOptions.Value.ApplyMvpRoster)
        {
            logger.LogInformation("Skipping seed because MVP roster initialization is disabled.");
            return;
        }

        if (!await dbContext.PokemonSpecies.AnyAsync(cancellationToken))
        {
            await dbContext.PokemonSpecies.AddRangeAsync(PokemonMvpRosterSeed.GetSpecies(), cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded MVP pokemon species roster with {Count} records.", 10);
        }
        else
        {
            logger.LogInformation("Skipping species seed because the catalog already contains records.");
        }

        if (await dbContext.PokemonMoves.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Skipping move seed because the move catalog already contains records.");
        }
        else
        {
            await dbContext.PokemonMoves.AddRangeAsync(PokemonMvpMoveSeed.GetMoves(), cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Seeded MVP pokemon move catalog with {Count} records.", 27);
        }

        if (await dbContext.PokemonLearnableMoves.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Skipping learnable move seed because the learnable catalog already contains records.");
            return;
        }

        await dbContext.PokemonLearnableMoves.AddRangeAsync(PokemonMvpLearnableMoveSeed.GetLearnableMoves(), cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded MVP pokemon learnable move catalog with {Count} records.", 48);
    }
}
