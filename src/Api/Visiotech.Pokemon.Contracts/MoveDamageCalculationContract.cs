namespace Visiotech.Pokemon.Contracts;

public sealed record MoveDamageCalculationContract(
    Guid AttackerMyPokemonId,
    Guid DefenderMyPokemonId,
    Guid MoveId,
    string MoveName,
    string MoveType,
    string MoveCategory,
    int AttackerLevel,
    int MovePower,
    string OffensiveStat,
    int OffensiveStatValue,
    string DefensiveStat,
    int DefensiveStatValue,
    int DefenderCurrentHealthPoints,
    int RandomFactor,
    decimal BaseDamage,
    IReadOnlyCollection<MoveDamageCalculationEffectivenessContract> EffectivenessBreakdown,
    decimal TotalEffectiveness,
    int RawDamage,
    int Damage,
    int DefenderRemainingHealthPoints);
