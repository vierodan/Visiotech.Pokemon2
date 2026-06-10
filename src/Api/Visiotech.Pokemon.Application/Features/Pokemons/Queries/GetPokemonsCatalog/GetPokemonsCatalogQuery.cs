using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonsCatalog;

public sealed record GetPokemonsCatalogQuery(
    string? Name,
    string? Type,
    int Page = 1,
    int PageSize = 20) : IQuery<PokemonSpeciesCatalogResponse>;
