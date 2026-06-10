using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record MyPokemonCatalogPage(
    IReadOnlyCollection<MyPokemon> Items,
    int TotalCount);
