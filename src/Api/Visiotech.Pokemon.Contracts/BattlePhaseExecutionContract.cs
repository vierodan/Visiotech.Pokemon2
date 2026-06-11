namespace Visiotech.Pokemon.Contracts;

public sealed record BattlePhaseExecutionContract(
    BattleContract Battle,
    MoveDamageCalculationContract DamageCalculation);
