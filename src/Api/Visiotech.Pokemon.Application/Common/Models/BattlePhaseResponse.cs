namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record BattlePhaseResponse(
    int SequenceNumber,
    Guid AttackerMyPokemonId,
    Guid DefenderMyPokemonId,
    Guid MoveId,
    string MoveName,
    int RandomFactor,
    IReadOnlyCollection<BattlePhaseEffectivenessResponse> EffectivenessBreakdown,
    decimal TotalEffectiveness,
    int Damage,
    int AttackerRemainingHealthPoints,
    int DefenderRemainingHealthPoints);
