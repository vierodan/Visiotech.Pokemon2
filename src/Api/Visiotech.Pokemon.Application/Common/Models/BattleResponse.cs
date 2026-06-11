namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record BattleResponse(
    Guid Id,
    string Status,
    int CurrentTurnNumber,
    Guid NextAttackerMyPokemonId,
    IReadOnlyCollection<BattleCombatantResponse> Combatants);
