namespace Visiotech.Pokemon.Contracts;

public sealed record BattleHistoryContract(
    Guid BattleId,
    IReadOnlyCollection<BattlePhaseContract> Phases);
