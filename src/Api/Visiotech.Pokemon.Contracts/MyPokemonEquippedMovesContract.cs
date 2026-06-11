namespace Visiotech.Pokemon.Contracts;

public sealed record MyPokemonEquippedMovesContract(
    Guid MyPokemonId,
    IReadOnlyCollection<PokemonMoveContract> Moves);
