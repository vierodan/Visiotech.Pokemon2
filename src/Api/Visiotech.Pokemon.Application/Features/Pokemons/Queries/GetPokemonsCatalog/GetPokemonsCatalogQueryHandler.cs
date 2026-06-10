using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Domain.Pokemons;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonsCatalog;

public sealed class GetPokemonsCatalogQueryHandler(IPokemonSpeciesReadRepository repository)
    : IQueryHandler<GetPokemonsCatalogQuery, PokemonSpeciesCatalogResponse>
{
    private const int MaxPageSize = 100;

    public async Task<PokemonSpeciesCatalogResponse> Handle(
        GetPokemonsCatalogQuery query,
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

        var result = await repository.SearchAsync(
            new PokemonSpeciesCatalogFilter(normalizedName, type, query.Page, query.PageSize),
            cancellationToken);

        var totalPages = result.TotalCount == 0
            ? 0
            : (int)Math.Ceiling(result.TotalCount / (double)query.PageSize);

        return new PokemonSpeciesCatalogResponse(
            result.Items.Select(PokemonSpeciesMapping.ToResponse).ToArray(),
            query.Page,
            query.PageSize,
            result.TotalCount,
            totalPages);
    }

    private static IReadOnlyDictionary<string, string[]> Validate(GetPokemonsCatalogQuery query)
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
}
