using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.MyPokemons.Commands;

internal static class MyPokemonCommandGuard
{
    public static async Task<MyPokemonCommandContext> ResolveSpeciesAndMovesAsync(
        Guid pokemonSpeciesId,
        IReadOnlyCollection<Guid> equippedMoveIds,
        IPokemonSpeciesReadRepository speciesRepository,
        IPokemonMoveReadRepository moveRepository,
        CancellationToken cancellationToken)
    {
        var pokemonSpecies = await speciesRepository.GetByIdWithLearnableMovesAsync(pokemonSpeciesId, cancellationToken);
        if (pokemonSpecies is null)
        {
            throw new ApplicationNotFoundException(
                $"Pokemon species '{pokemonSpeciesId}' was not found.",
                "pokemonSpeciesId");
        }

        var requestedMoveIds = equippedMoveIds.Distinct().ToArray();
        var requestedMoves = await moveRepository.GetByIdsAsync(requestedMoveIds, cancellationToken);
        var requestedMovesById = requestedMoves.ToDictionary(move => move.Id);

        var missingMoveIds = requestedMoveIds
            .Where(moveId => !requestedMovesById.ContainsKey(moveId))
            .ToArray();

        if (missingMoveIds.Length > 0)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["equippedMoveIds"] = missingMoveIds
                    .Select(static moveId => $"Pokemon move '{moveId}' was not found.")
                    .ToArray()
            });
        }

        var learnableMoveIds = pokemonSpecies.LearnableMoveIds.ToHashSet();
        if (learnableMoveIds.Count == 0)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["equippedMoveIds"] = [$"Pokemon species '{pokemonSpecies.Name.Value}' does not have learnable moves configured."]
            });
        }

        var nonLearnableMoveIds = requestedMoveIds
            .Where(moveId => !learnableMoveIds.Contains(moveId))
            .ToArray();

        if (nonLearnableMoveIds.Length > 0)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["equippedMoveIds"] = nonLearnableMoveIds
                    .Select(moveId => $"Pokemon move '{requestedMovesById[moveId].Name.Value}' is not learnable by species '{pokemonSpecies.Name.Value}'.")
                    .ToArray()
            });
        }

        return new MyPokemonCommandContext(pokemonSpecies, requestedMoves);
    }
}
