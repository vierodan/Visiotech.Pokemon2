using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record PokemonSpeciesCatalogPage(
    IReadOnlyCollection<PokemonSpecies> Items,
    int TotalCount);
