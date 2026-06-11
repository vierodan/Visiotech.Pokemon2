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
            battle.WinnerMyPokemonId,
            battle.LoserMyPokemonId,
            battle.Combatants
                .OrderBy(combatant => combatant.SlotNumber)
                .Select(combatant => new BattleCombatantResponse(
                    combatant.SlotNumber,
                    combatant.MyPokemonId,
                    combatant.CurrentHealthPoints,
                    combatant.TotalHealthPoints))
                .ToArray(),
            battle.Phases
                .OrderBy(phase => phase.SequenceNumber)
                .Select(phase => new BattlePhaseResponse(
                    phase.SequenceNumber,
                    phase.AttackerMyPokemonId,
                    phase.DefenderMyPokemonId,
                    phase.MoveId,
                    phase.MoveName,
                    phase.RandomFactor,
                    phase.EffectivenessBreakdown
                        .OrderBy(item => item.DefenderType.ToString(), StringComparer.Ordinal)
                        .Select(item => new BattlePhaseEffectivenessResponse(
                            item.DefenderType.ToString(),
                            item.Multiplier))
                        .ToArray(),
                    phase.TotalEffectiveness,
                    phase.Damage,
                    phase.AttackerRemainingHealthPoints,
                    phase.DefenderRemainingHealthPoints))
                .ToArray());
}
