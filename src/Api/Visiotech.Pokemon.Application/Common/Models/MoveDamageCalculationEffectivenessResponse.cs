namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record MoveDamageCalculationEffectivenessResponse(
    string DefenderType,
    decimal Multiplier);
