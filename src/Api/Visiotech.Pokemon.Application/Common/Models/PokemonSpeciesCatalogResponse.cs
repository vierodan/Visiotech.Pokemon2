namespace Visiotech.Pokemon.Application.Common.Models;

public sealed record PokemonSpeciesCatalogResponse(
    IReadOnlyCollection<PokemonSpeciesResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
