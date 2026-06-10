using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Abstractions.Persistence;

public interface IPokemonSpeciesWriteRepository
{
    Task AddAsync(PokemonSpecies pokemonSpecies, CancellationToken cancellationToken);

    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken);
}
