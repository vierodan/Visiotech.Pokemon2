using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Abstractions.Persistence;

public interface IPokemonMoveWriteRepository
{
    Task AddAsync(PokemonMove pokemonMove, CancellationToken cancellationToken);

    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken);

    Task<bool> ExistsByNormalizedNameAsync(string normalizedName, Guid excludedId, CancellationToken cancellationToken);

    Task<PokemonMove?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken);

    void Remove(PokemonMove pokemonMove);
}
