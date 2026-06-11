namespace Visiotech.Pokemon.Contracts;

public sealed record BattlePhaseContract(
    int SequenceNumber,
    Guid AttackerMyPokemonId,
    Guid DefenderMyPokemonId,
    Guid MoveId,
    string MoveName,
    int RandomFactor,
    IReadOnlyCollection<BattlePhaseEffectivenessContract> EffectivenessBreakdown,
    decimal TotalEffectiveness,
    int Damage,
    int AttackerRemainingHealthPoints,
    int DefenderRemainingHealthPoints);
