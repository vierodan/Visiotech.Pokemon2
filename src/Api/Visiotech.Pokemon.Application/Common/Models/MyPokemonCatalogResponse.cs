namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record MyPokemonCatalogResponse(
    IReadOnlyCollection<MyPokemonResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
