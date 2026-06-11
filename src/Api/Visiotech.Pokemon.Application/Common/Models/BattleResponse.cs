namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record BattleResponse(
    Guid Id,
    string Status,
    int CurrentTurnNumber,
    Guid? NextAttackerMyPokemonId,
    Guid? WinnerMyPokemonId,
    Guid? LoserMyPokemonId,
    IReadOnlyCollection<BattleCombatantResponse> Combatants,
    IReadOnlyCollection<BattlePhaseResponse> History);
