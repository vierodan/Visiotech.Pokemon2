using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Moves.Queries.GetPokemonMovesCatalog;

public sealed record GetPokemonMovesCatalogQuery(
    string? Name = null,
    string? Type = null,
    string? Category = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PokemonMoveCatalogResponse>;
