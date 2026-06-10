namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record PokemonLearnableMovesResponse(
    Guid PokemonSpeciesId,
    string PokemonSpeciesName,
    IReadOnlyCollection<PokemonMoveResponse> Moves);
