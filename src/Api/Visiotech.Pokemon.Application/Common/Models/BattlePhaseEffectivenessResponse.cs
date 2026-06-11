namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record BattlePhaseEffectivenessResponse(
    string DefenderType,
    decimal Multiplier);
