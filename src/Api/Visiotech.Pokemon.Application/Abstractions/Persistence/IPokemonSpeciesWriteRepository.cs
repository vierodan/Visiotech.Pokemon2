using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Abstractions.Persistence;

public interface IPokemonSpeciesWriteRepository
{
    Task AddAsync(PokemonSpecies pokemonSpecies, CancellationToken cancellationToken);

    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken);

    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, Guid excludedId, CancellationToken cancellationToken);

    Task<PokemonSpecies?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken);
}
