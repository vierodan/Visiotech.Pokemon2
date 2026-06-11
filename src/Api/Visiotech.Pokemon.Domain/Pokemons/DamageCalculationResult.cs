namespace Visiotech.Pokemon.Domain.Pokemons;

public sealed record DamageCalculationResult(
    string OffensiveStat,
    int OffensiveStatValue,
    string DefensiveStat,
    int DefensiveStatValue,
    int RandomFactor,
    decimal BaseDamage,
    IReadOnlyCollection<DamageCalculationEffectiveness> EffectivenessBreakdown,
    decimal TotalEffectiveness,
    int RawDamage,
    int Damage,
    int DefenderRemainingHealthPoints);
