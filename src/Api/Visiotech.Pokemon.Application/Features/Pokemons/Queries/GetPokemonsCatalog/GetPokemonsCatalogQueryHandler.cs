using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonsCatalog;

public sealed class GetPokemonsCatalogQueryHandler(IPokemonReadRepository repository)
    : IQueryHandler<GetPokemonsCatalogQuery, IReadOnlyCollection<PokemonResponse>>
{
    public async Task<IReadOnlyCollection<PokemonResponse>> Handle(
        GetPokemonsCatalogQuery query,
        CancellationToken cancellationToken)
    {
        var pokemons = await repository.GetAllAsync(cancellationToken);

        return pokemons
            .OrderBy(pokemon => pokemon.Name.Value, StringComparer.OrdinalIgnoreCase)
            .Select(pokemon => new PokemonResponse(
                pokemon.Id,
                pokemon.Name.Value,
                pokemon.Type.ToString(),
                pokemon.Level.Value,
                pokemon.Stats.Health,
                pokemon.Stats.Attack,
                pokemon.Stats.Defense,
                pokemon.Stats.SpecialAttack,
                pokemon.Stats.SpecialDefense,
                pokemon.Stats.Speed,
                pokemon.Moves
                    .Select(move => new PokemonMoveResponse(move.Name.Value, move.Type.ToString(), move.Power))
                    .ToArray(),
                pokemon.Abilities
                    .Select(ability => new PokemonAbilityResponse(ability.Name.Value))
                    .ToArray()))
            .ToArray();
    }
}

