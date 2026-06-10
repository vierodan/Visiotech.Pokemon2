namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record PokemonResponse(
    Guid Id,
    string Name,
    string Type,
    int Level,
    int Health,
    int Attack,
    int Defense,
    int SpecialAttack,
    int SpecialDefense,
    int Speed,
    IReadOnlyCollection<PokemonMoveResponse> Moves,
    IReadOnlyCollection<PokemonAbilityResponse> Abilities);

