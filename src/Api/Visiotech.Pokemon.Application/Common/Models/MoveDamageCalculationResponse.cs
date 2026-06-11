namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record MoveDamageCalculationResponse(
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
    IReadOnlyCollection<MoveDamageCalculationEffectivenessResponse> EffectivenessBreakdown,
    decimal TotalEffectiveness,
    int RawDamage,
    int Damage,
    int DefenderRemainingHealthPoints);
