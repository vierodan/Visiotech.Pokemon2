using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Abstractions.Persistence;

public interface IPokemonMoveReadRepository
{
    Task<PokemonMoveCatalogPage> SearchAsync(PokemonMoveCatalogFilter filter, CancellationToken cancellationToken);

    Task<PokemonMove?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
