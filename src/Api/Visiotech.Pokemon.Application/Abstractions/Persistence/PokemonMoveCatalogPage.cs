using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record PokemonMoveCatalogPage(
    IReadOnlyCollection<PokemonMove> Items,
    int TotalCount);
