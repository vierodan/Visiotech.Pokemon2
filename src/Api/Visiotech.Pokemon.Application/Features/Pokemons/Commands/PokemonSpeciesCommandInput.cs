using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Commands;

internal sealed record PokemonSpeciesCommandInput(
    Name Name,
    PokemonTyping Typing,
    BaseStats BaseStats);
