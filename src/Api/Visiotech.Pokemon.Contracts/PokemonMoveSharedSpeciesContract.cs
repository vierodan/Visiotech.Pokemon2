namespace Visiotech.Pokemon.Contracts;

public sealed record PokemonMoveSharedSpeciesContract(
    Guid PokemonMoveId,
    string PokemonMoveName,
    IReadOnlyCollection<PokemonSpeciesContract> PokemonSpecies);
