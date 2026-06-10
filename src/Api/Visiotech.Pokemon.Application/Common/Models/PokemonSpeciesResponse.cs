namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record PokemonSpeciesResponse(
    Guid Id,
    string Name,
    IReadOnlyCollection<string> Types,
    PokemonBaseStatsResponse BaseStats);
