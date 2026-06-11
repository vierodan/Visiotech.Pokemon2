namespace Visiotech.Pokemon.Domain.Battles;

public sealed record BattlePhaseRegistration(
    int SequenceNumber,
    Guid AttackerMyPokemonId,
    Guid DefenderMyPokemonId,
    Guid MoveId,
    string MoveName,
    int RandomFactor,
    IReadOnlyCollection<BattlePhaseEffectivenessInput> EffectivenessBreakdown,
    decimal TotalEffectiveness,
    int Damage,
    int AttackerRemainingHealthPoints,
    int DefenderRemainingHealthPoints,
    Guid? NextAttackerMyPokemonId,
    bool FinishesBattle);
