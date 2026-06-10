namespace Visiotech.Pokemon.Contracts;

public sealed record PokemonMoveCatalogContract(
    IReadOnlyCollection<PokemonMoveContract> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
