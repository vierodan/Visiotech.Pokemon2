namespace Visiotech.Pokemon.Contracts;

public sealed record UpdateMyPokemonRequestContract(
    int Level,
    int CurrentHealthPoints,
    int TotalHealthPoints,
    IReadOnlyCollection<Guid>? EquippedMoveIds);
