using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record PokemonMoveCatalogFilter(
    string? NormalizedName,
    PokemonType? Type,
    MoveCategory? Category,
    int Page,
    int PageSize);
