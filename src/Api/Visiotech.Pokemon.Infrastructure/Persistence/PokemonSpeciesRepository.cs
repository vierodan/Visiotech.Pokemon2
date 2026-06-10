using Microsoft.EntityFrameworkCore;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Infrastructure.Persistence;

public sealed class PokemonSpeciesRepository(PokemonDbContext dbContext)
    : IPokemonSpeciesReadRepository, IPokemonSpeciesWriteRepository
{
    public async Task AddAsync(PokemonSpecies pokemonSpecies, CancellationToken cancellationToken) =>
        await dbContext.PokemonSpecies.AddAsync(pokemonSpecies, cancellationToken);

    public Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken) =>
        dbContext.PokemonSpecies.AnyAsync(
            pokemonSpecies => pokemonSpecies.NormalizedName == normalizedName,
            cancellationToken);

    public async Task<IReadOnlyCollection<PokemonSpecies>> GetAllAsync(CancellationToken cancellationToken) =>
        await dbContext.PokemonSpecies
            .AsNoTracking()
            .OrderBy(pokemonSpecies => pokemonSpecies.Name.Value)
            .ToArrayAsync(cancellationToken);
}
