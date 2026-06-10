namespace Visiotech.Pokemon.Contracts;

public sealed record CreatePokemonSpeciesRequestContract(
    string? Name,
    IReadOnlyCollection<string>? Types,
    PokemonBaseStatsContract? BaseStats);
