using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Abstractions.Persistence;

public interface IMyPokemonWriteRepository
{
    Task AddAsync(MyPokemon myPokemon, CancellationToken cancellationToken);

    Task<MyPokemon?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken);
}
