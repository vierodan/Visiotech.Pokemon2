using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.Moves.Queries;

internal static class PokemonMoveMapping
{
    public static PokemonMoveResponse ToResponse(PokemonMove pokemonMove) =>
        new(
            pokemonMove.Id,
            pokemonMove.Name.Value,
            pokemonMove.Type.ToString(),
            pokemonMove.Category.ToString(),
            pokemonMove.Power);
}
