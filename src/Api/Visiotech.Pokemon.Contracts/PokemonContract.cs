namespace Visiotech.Pokemon.Contracts;

public sealed record PokemonContract(
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
    IReadOnlyCollection<PokemonMoveContract> Moves,
    IReadOnlyCollection<PokemonAbilityContract> Abilities);
