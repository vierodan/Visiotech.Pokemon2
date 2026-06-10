using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonsCatalog;

public sealed class GetPokemonsCatalogQueryHandler(IPokemonSpeciesReadRepository repository)
    : IQueryHandler<GetPokemonsCatalogQuery, IReadOnlyCollection<PokemonSpeciesResponse>>
{
    public async Task<IReadOnlyCollection<PokemonSpeciesResponse>> Handle(
        GetPokemonsCatalogQuery query,
        CancellationToken cancellationToken)
    {
        var species = await repository.GetAllAsync(cancellationToken);

        return species
            .OrderBy(static pokemonSpecies => pokemonSpecies.Name.Value, StringComparer.OrdinalIgnoreCase)
            .Select(static pokemonSpecies => new PokemonSpeciesResponse(
                pokemonSpecies.Id,
                pokemonSpecies.Name.Value,
                pokemonSpecies.Types.Select(static type => type.ToString()).ToArray(),
                new PokemonBaseStatsResponse(
                    pokemonSpecies.BaseStats.Health,
                    pokemonSpecies.BaseStats.Attack,
                    pokemonSpecies.BaseStats.Defense,
                    pokemonSpecies.BaseStats.SpecialAttack,
                    pokemonSpecies.BaseStats.SpecialDefense,
                    pokemonSpecies.BaseStats.Speed)))
            .ToArray();
    }
}
