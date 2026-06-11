namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record MyPokemonEquippedMovesResponse(
    Guid MyPokemonId,
    IReadOnlyCollection<PokemonMoveResponse> Moves);
