namespace Visiotech.Pokemon.Contracts;

public sealed record UpdatePokemonMoveRequestContract(
    string? Name,
    string? Type,
    string? Category,
    int Power);
