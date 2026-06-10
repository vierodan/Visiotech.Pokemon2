using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.MyPokemons.Queries.GetMyPokemonsCatalog;

public sealed record GetMyPokemonsCatalogQuery(
    int Page = 1,
    int PageSize = 20) : IQuery<MyPokemonCatalogResponse>;
