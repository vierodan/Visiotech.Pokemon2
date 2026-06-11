namespace Visiotech.Pokemon.Contracts;

public sealed record BattleCombatantContract(
    int SlotNumber,
    Guid MyPokemonId,
    int CurrentHealthPoints,
    int TotalHealthPoints);
