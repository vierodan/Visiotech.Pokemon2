namespace Visiotech.Pokemon.Contracts;

public sealed record ExecuteBattlePhaseRequestContract(
    Guid AttackerMyPokemonId,
    Guid MoveId);
