namespace Visiotech.Pokemon.Contracts;

public sealed record UpdatePokemonSpeciesRequestContract(
    string? Name,
    IReadOnlyCollection<string>? Types,
    PokemonBaseStatsContract? BaseStats);
