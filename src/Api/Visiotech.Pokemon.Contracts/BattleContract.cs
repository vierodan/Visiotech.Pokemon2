namespace Visiotech.Pokemon.Contracts;

public sealed record BattleContract(
    Guid Id,
    string Status,
    int CurrentTurnNumber,
    Guid NextAttackerMyPokemonId,
    IReadOnlyCollection<BattleCombatantContract> Combatants);
