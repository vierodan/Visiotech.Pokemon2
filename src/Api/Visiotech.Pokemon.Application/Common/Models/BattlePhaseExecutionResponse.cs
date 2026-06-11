namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record BattlePhaseExecutionResponse(
    BattleResponse Battle,
    MoveDamageCalculationResponse DamageCalculation);
