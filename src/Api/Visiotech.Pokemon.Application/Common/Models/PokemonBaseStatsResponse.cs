namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record PokemonBaseStatsResponse(
    int Health,
    int Attack,
    int Defense,
    int SpecialAttack,
    int SpecialDefense,
    int Speed);
