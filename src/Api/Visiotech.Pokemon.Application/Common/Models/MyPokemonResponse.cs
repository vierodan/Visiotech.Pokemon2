namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record MyPokemonResponse(
    Guid Id,
    PokemonSpeciesResponse Species,
    int Level,
    int CurrentHealthPoints,
    int TotalHealthPoints,
    IReadOnlyCollection<PokemonMoveResponse> EquippedMoves);
