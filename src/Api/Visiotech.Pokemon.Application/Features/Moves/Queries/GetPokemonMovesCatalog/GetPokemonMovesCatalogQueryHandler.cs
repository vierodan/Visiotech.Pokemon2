using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.Moves.Queries.GetPokemonMovesCatalog;

public sealed class GetPokemonMovesCatalogQueryHandler(IPokemonMoveReadRepository repository)
    : IQueryHandler<GetPokemonMovesCatalogQuery, PokemonMoveCatalogResponse>
{
    private const int MaxPageSize = 100;

    public async Task<PokemonMoveCatalogResponse> Handle(
        GetPokemonMovesCatalogQuery query,
        CancellationToken cancellationToken)
    {
        var errors = Validate(query);
        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }

        var normalizedName = string.IsNullOrWhiteSpace(query.Name)
            ? null
            : query.Name.Trim().ToUpperInvariant();

        PokemonType? type = string.IsNullOrWhiteSpace(query.Type)
            ? null
            : ParseType(query.Type.Trim());

        MoveCategory? category = string.IsNullOrWhiteSpace(query.Category)
            ? null
            : ParseCategory(query.Category.Trim());

        var result = await repository.SearchAsync(
            new PokemonMoveCatalogFilter(normalizedName, type, category, query.Page, query.PageSize),
            cancellationToken);

        var totalPages = result.TotalCount == 0
            ? 0
            : (int)Math.Ceiling(result.TotalCount / (double)query.PageSize);

        return new PokemonMoveCatalogResponse(
            result.Items.Select(PokemonMoveMapping.ToResponse).ToArray(),
            query.Page,
            query.PageSize,
            result.TotalCount,
            totalPages);
    }

    private static IReadOnlyDictionary<string, string[]> Validate(GetPokemonMovesCatalogQuery query)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (query.Page <= 0)
        {
            errors["page"] = ["Page must be greater than 0."];
        }

        if (query.PageSize <= 0 || query.PageSize > MaxPageSize)
        {
            errors["pageSize"] = [$"PageSize must be between 1 and {MaxPageSize}."];
        }

        if (!string.IsNullOrWhiteSpace(query.Type) &&
            !PokemonTypeCatalog.TryParse(query.Type.Trim(), out _))
        {
            errors["type"] =
            [
                $"Unsupported type '{query.Type}'. Allowed values: {string.Join(", ", PokemonTypeCatalog.AllowedNames)}."
            ];
        }

        if (!string.IsNullOrWhiteSpace(query.Category) &&
            !MoveCategoryCatalog.TryParse(query.Category.Trim(), out _))
        {
            errors["category"] =
            [
                $"Unsupported category '{query.Category}'. Allowed values: {string.Join(", ", MoveCategoryCatalog.AllowedNames)}."
            ];
        }

        return errors;
    }

    private static PokemonType ParseType(string type)
    {
        if (!PokemonTypeCatalog.TryParse(type, out var pokemonType))
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["type"] = [$"Unsupported type '{type}'."]
            });
        }

        return pokemonType;
    }

    private static MoveCategory ParseCategory(string category)
    {
        if (!MoveCategoryCatalog.TryParse(category, out var moveCategory))
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["category"] = [$"Unsupported category '{category}'."]
            });
        }

        return moveCategory;
    }
}
