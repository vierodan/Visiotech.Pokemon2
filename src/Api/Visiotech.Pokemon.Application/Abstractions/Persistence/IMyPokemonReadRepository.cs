using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Abstractions.Persistence;

public interface IMyPokemonReadRepository
{
    Task<MyPokemonCatalogPage> SearchAsync(MyPokemonCatalogFilter filter, CancellationToken cancellationToken);

    Task<MyPokemon?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<MyPokemon>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
}
