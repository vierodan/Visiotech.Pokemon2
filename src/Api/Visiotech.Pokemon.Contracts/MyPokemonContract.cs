namespace Visiotech.Pokemon.Contracts;

public sealed record MyPokemonContract(
    Guid Id,
    PokemonSpeciesContract Species,
    int Level,
    int CurrentHealthPoints,
    int TotalHealthPoints,
    IReadOnlyCollection<PokemonMoveContract> EquippedMoves);
