using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Abstractions.Persistence;

public interface IPokemonSpeciesReadRepository
{
    Task<IReadOnlyCollection<PokemonSpecies>> GetAllAsync(CancellationToken cancellationToken);
}
