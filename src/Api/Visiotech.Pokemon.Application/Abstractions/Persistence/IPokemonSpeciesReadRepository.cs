using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Abstractions.Persistence;

public interface IPokemonSpeciesReadRepository
{
    Task<PokemonSpeciesCatalogPage> SearchAsync(PokemonSpeciesCatalogFilter filter, CancellationToken cancellationToken);

    Task<PokemonSpecies?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PokemonSpecies?> GetByIdWithLearnableMovesAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<PokemonSpecies>> GetByLearnableMoveIdAsync(Guid moveId, CancellationToken cancellationToken);
}
