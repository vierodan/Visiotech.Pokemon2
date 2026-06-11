namespace Visiotech.Pokemon.Contracts;

public sealed record CreateBattleRequestContract(
    Guid FirstMyPokemonId,
    Guid SecondMyPokemonId);
