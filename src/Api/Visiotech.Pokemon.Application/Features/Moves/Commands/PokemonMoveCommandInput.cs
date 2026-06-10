using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.Moves.Commands;

internal sealed record PokemonMoveCommandInput(
    Name Name,
    PokemonType Type,
    MoveCategory Category,
    int Power);
