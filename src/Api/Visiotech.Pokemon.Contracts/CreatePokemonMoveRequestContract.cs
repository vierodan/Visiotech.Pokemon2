namespace Visiotech.Pokemon.Contracts;

public sealed record CreatePokemonMoveRequestContract(
    string? Name,
    string? Type,
    string? Category,
    int Power);
