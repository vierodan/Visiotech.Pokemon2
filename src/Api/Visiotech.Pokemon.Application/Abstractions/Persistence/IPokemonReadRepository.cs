using PokemonAggregate = global::Visiotech.Pokemon.Domain.Pokemons.Pokemon;

namespace Visiotech.Pokemon.Application.Abstractions.Persistence;

public interface IPokemonReadRepository
{
    Task<IReadOnlyCollection<PokemonAggregate>> GetAllAsync(CancellationToken cancellationToken);
}
