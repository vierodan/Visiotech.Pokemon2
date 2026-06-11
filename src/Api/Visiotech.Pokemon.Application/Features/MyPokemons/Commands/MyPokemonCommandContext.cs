using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.MyPokemons.Commands;

internal sealed record MyPokemonCommandContext(
    PokemonSpecies Species,
    IReadOnlyCollection<PokemonMove> EquippedMoves);
