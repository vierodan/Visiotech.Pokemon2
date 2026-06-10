namespace Visiotech.Pokemon.Contracts;

public sealed record UpdatePokemonLearnableMovesRequestContract(
    IReadOnlyCollection<Guid>? AddMoveIds,
    IReadOnlyCollection<Guid>? RemoveMoveIds);
