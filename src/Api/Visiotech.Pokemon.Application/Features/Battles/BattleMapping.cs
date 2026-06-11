using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Domain.Battles;

namespace Visiotech.Pokemon.Application.Features.Battles;

internal static class BattleMapping
{
    public static BattleResponse ToResponse(Battle battle) =>
        new(
            battle.Id,
            battle.Status.ToString(),
            battle.CurrentTurnNumber,
            battle.NextAttackerMyPokemonId,
            battle.Combatants
                .OrderBy(combatant => combatant.SlotNumber)
                .Select(combatant => new BattleCombatantResponse(
                    combatant.SlotNumber,
                    combatant.MyPokemonId,
                    combatant.CurrentHealthPoints,
                    combatant.TotalHealthPoints))
                .ToArray());
}
