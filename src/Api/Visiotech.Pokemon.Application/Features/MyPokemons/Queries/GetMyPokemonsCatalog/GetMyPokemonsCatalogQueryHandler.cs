using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.MyPokemons.Queries.GetMyPokemonsCatalog;

public sealed class GetMyPokemonsCatalogQueryHandler(
    IMyPokemonReadRepository repository,
    IPokemonSpeciesReadRepository speciesRepository,
    IPokemonMoveReadRepository moveRepository)
    : IQueryHandler<GetMyPokemonsCatalogQuery, MyPokemonCatalogResponse>
{
    private const int MaxPageSize = 100;

    public async Task<MyPokemonCatalogResponse> Handle(
        GetMyPokemonsCatalogQuery query,
        CancellationToken cancellationToken)
    {
        var errors = Validate(query);
        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }

        var result = await repository.SearchAsync(
            new MyPokemonCatalogFilter(query.Page, query.PageSize),
            cancellationToken);

        var speciesIds = result.Items
            .Select(item => item.PokemonSpeciesId)
            .Distinct()
            .ToArray();

        var moveIds = result.Items
            .SelectMany(item => item.EquippedMoveIds)
            .Distinct()
            .ToArray();

        var pokemonSpecies = await speciesRepository.GetByIdsAsync(speciesIds, cancellationToken);
        var equippedMoves = await moveRepository.GetByIdsAsync(moveIds, cancellationToken);

        var speciesById = pokemonSpecies.ToDictionary(species => species.Id);
        var movesById = equippedMoves.ToDictionary(move => move.Id);

        var items = result.Items
            .Select(item =>
            {
                var species = speciesById.GetValueOrDefault(item.PokemonSpeciesId)
                    ?? throw new InvalidOperationException($"Pokemon species '{item.PokemonSpeciesId}' referenced by my pokemon '{item.Id}' was not found.");

                var moves = item.EquippedMoveIds
                    .Where(movesById.ContainsKey)
                    .Select(moveId => movesById[moveId])
                    .ToArray();

                return MyPokemonMapping.ToResponse(item, species, moves);
            })
            .ToArray();

        var totalPages = result.TotalCount == 0
            ? 0
            : (int)Math.Ceiling(result.TotalCount / (double)query.PageSize);

        return new MyPokemonCatalogResponse(
            items,
            query.Page,
            query.PageSize,
            result.TotalCount,
            totalPages);
    }

    private static IReadOnlyDictionary<string, string[]> Validate(GetMyPokemonsCatalogQuery query)
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

        return errors;
    }
}
