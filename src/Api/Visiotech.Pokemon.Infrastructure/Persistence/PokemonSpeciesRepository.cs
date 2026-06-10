using Microsoft.EntityFrameworkCore;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Infrastructure.Persistence;

public sealed class PokemonSpeciesRepository(PokemonDbContext dbContext)
    : IPokemonSpeciesReadRepository, IPokemonSpeciesWriteRepository
{
    public async Task AddAsync(PokemonSpecies pokemonSpecies, CancellationToken cancellationToken) =>
        await dbContext.PokemonSpecies.AddAsync(pokemonSpecies, cancellationToken);

    public void Remove(PokemonSpecies pokemonSpecies) => dbContext.PokemonSpecies.Remove(pokemonSpecies);

    public Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken) =>
        dbContext.PokemonSpecies.AnyAsync(
            pokemonSpecies => pokemonSpecies.NormalizedName == normalizedName,
            cancellationToken);

    public Task<bool> ExistsByNormalizedNameAsync(string normalizedName, Guid excludedId, CancellationToken cancellationToken) =>
        dbContext.PokemonSpecies.AnyAsync(
            pokemonSpecies => pokemonSpecies.NormalizedName == normalizedName && pokemonSpecies.Id != excludedId,
            cancellationToken);

    public async Task<PokemonSpecies?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.PokemonSpecies
            .SingleOrDefaultAsync(pokemonSpecies => pokemonSpecies.Id == id, cancellationToken);

    public async Task<PokemonSpecies?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.PokemonSpecies
            .AsNoTracking()
            .SingleOrDefaultAsync(pokemonSpecies => pokemonSpecies.Id == id, cancellationToken);

    public async Task<PokemonSpeciesCatalogPage> SearchAsync(
        PokemonSpeciesCatalogFilter filter,
        CancellationToken cancellationToken)
    {
        var query = dbContext.PokemonSpecies.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.NormalizedName))
        {
            var namePattern = $"%{filter.NormalizedName}%";
            query = query.Where(pokemonSpecies =>
                EF.Functions.Like(pokemonSpecies.NormalizedName, namePattern));
        }

        if (filter.Type is not null)
        {
            var type = filter.Type.Value;
            query = query.Where(pokemonSpecies =>
                pokemonSpecies.Typing.PrimaryType == type ||
                pokemonSpecies.Typing.SecondaryType == type);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(pokemonSpecies => pokemonSpecies.Name.Value)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToArrayAsync(cancellationToken);

        return new PokemonSpeciesCatalogPage(items, totalCount);
    }
}
