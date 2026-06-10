namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record PokemonMoveSharedSpeciesResponse(
    Guid PokemonMoveId,
    string PokemonMoveName,
    IReadOnlyCollection<PokemonSpeciesResponse> PokemonSpecies);
