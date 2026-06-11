namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record BattleHistoryResponse(
    Guid BattleId,
    IReadOnlyCollection<BattlePhaseResponse> Phases);
