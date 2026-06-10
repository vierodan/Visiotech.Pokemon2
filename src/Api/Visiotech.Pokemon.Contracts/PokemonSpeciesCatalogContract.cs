namespace Visiotech.Pokemon.Contracts;

public sealed record PokemonSpeciesCatalogContract(
    IReadOnlyCollection<PokemonSpeciesContract> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
