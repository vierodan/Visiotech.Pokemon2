namespace Visiotech.Pokemon.Contracts;

public sealed record BattleContract(
    Guid Id,
    string Status,
    int CurrentTurnNumber,
    Guid? NextAttackerMyPokemonId,
    Guid? WinnerMyPokemonId,
    Guid? LoserMyPokemonId,
    IReadOnlyCollection<BattleCombatantContract> Combatants,
    IReadOnlyCollection<BattlePhaseContract> History);
