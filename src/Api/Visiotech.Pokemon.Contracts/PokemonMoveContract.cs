namespace Visiotech.Pokemon.Contracts;

public sealed record PokemonMoveContract(
    Guid Id,
    string Name,
    string Type,
    string Category,
    int Power);
