namespace Visiotech.Pokemon.Contracts;

public sealed record MyPokemonCatalogContract(
    IReadOnlyCollection<MyPokemonContract> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
