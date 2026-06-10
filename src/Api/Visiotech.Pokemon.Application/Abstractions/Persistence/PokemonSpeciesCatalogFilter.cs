using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record PokemonSpeciesCatalogFilter(
    string? NormalizedName,
    PokemonType? Type,
    int Page,
    int PageSize);
