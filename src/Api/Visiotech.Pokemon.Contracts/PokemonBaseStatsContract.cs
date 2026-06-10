namespace Visiotech.Pokemon.Contracts;

public sealed record PokemonBaseStatsContract(
    int Health,
    int Attack,
    int Defense,
    int SpecialAttack,
    int SpecialDefense,
    int Speed);
