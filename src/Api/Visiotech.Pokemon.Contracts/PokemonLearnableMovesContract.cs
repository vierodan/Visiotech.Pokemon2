namespace Visiotech.Pokemon.Contracts;

public sealed record PokemonLearnableMovesContract(
    Guid PokemonSpeciesId,
    string PokemonSpeciesName,
    IReadOnlyCollection<PokemonMoveContract> Moves);
