namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record PokemonMoveCatalogResponse(
    IReadOnlyCollection<PokemonMoveResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
