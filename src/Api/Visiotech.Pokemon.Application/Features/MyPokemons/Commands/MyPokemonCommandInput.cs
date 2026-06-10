using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.MyPokemons.Commands;

internal sealed record MyPokemonCommandInput(
    Guid PokemonSpeciesId,
    Level Level,
    int CurrentHealthPoints,
    int TotalHealthPoints,
    IReadOnlyCollection<Guid> EquippedMoveIds);
