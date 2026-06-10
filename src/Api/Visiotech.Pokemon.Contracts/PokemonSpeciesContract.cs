namespace Visiotech.Pokemon.Contracts;

public sealed record PokemonSpeciesContract(
    Guid Id,
    string Name,
    IReadOnlyCollection<string> Types,
    PokemonBaseStatsContract BaseStats);
