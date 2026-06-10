namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record PokemonMoveResponse(
    Guid Id,
    string Name,
    string Type,
    string Category,
    int Power);
