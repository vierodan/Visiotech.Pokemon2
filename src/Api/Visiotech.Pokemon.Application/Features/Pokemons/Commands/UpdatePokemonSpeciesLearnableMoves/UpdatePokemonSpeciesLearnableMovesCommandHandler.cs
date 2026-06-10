using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Moves.Queries;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Commands.UpdatePokemonSpeciesLearnableMoves;

public sealed class UpdatePokemonSpeciesLearnableMovesCommandHandler(
    IPokemonSpeciesWriteRepository speciesRepository,
    IPokemonMoveReadRepository moveRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdatePokemonSpeciesLearnableMovesCommand, PokemonLearnableMovesResponse>
{
    public async Task<PokemonLearnableMovesResponse> Handle(
        UpdatePokemonSpeciesLearnableMovesCommand command,
        CancellationToken cancellationToken)
    {
        var addMoveIds = command.AddMoveIds?.ToArray() ?? [];
        var removeMoveIds = command.RemoveMoveIds?.ToArray() ?? [];

        var errors = Validate(command.PokemonSpeciesId, addMoveIds, removeMoveIds);
        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }

        var pokemonSpecies = await speciesRepository.GetForUpdateWithLearnableMovesAsync(
            command.PokemonSpeciesId,
            cancellationToken);

        if (pokemonSpecies is null)
        {
            throw new ApplicationNotFoundException(
                $"Pokemon species '{command.PokemonSpeciesId}' was not found.",
                "id");
        }

        var requestedMoveIds = addMoveIds
            .Concat(removeMoveIds)
            .Distinct()
            .ToArray();

        var requestedMoves = await moveRepository.GetByIdsAsync(requestedMoveIds, cancellationToken);
        var requestedMovesById = requestedMoves.ToDictionary(move => move.Id);

        var missingMoveIds = requestedMoveIds
            .Where(moveId => !requestedMovesById.ContainsKey(moveId))
            .ToArray();

        if (missingMoveIds.Length > 0)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["moveIds"] = missingMoveIds
                    .Select(static moveId => $"Pokemon move '{moveId}' was not found.")
                    .ToArray()
            });
        }

        var currentLearnableMoveIds = pokemonSpecies.LearnableMoveIds.ToHashSet();

        var duplicateAssociationIds = addMoveIds
            .Where(currentLearnableMoveIds.Contains)
            .Distinct()
            .ToArray();

        if (duplicateAssociationIds.Length > 0)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["addMoveIds"] = duplicateAssociationIds
                    .Select(static moveId => $"Pokemon move '{moveId}' is already associated with the species.")
                    .ToArray()
            });
        }

        var missingAssociationIds = removeMoveIds
            .Where(moveId => !currentLearnableMoveIds.Contains(moveId))
            .Distinct()
            .ToArray();

        if (missingAssociationIds.Length > 0)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["removeMoveIds"] = missingAssociationIds
                    .Select(static moveId => $"Pokemon move '{moveId}' is not currently associated with the species.")
                    .ToArray()
            });
        }

        foreach (var moveId in removeMoveIds)
        {
            pokemonSpecies.RemoveLearnableMove(moveId);
        }

        foreach (var moveId in addMoveIds)
        {
            pokemonSpecies.AddLearnableMove(moveId);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var learnableMoves = await moveRepository.GetByIdsAsync(pokemonSpecies.LearnableMoveIds.ToArray(), cancellationToken);
        var orderedMoves = learnableMoves
            .OrderBy(move => move.Name.Value, StringComparer.Ordinal)
            .Select(PokemonMoveMapping.ToResponse)
            .ToArray();

        return new PokemonLearnableMovesResponse(
            pokemonSpecies.Id,
            pokemonSpecies.Name.Value,
            orderedMoves);
    }

    private static Dictionary<string, string[]> Validate(
        Guid pokemonSpeciesId,
        IReadOnlyCollection<Guid> addMoveIds,
        IReadOnlyCollection<Guid> removeMoveIds)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        if (pokemonSpeciesId == Guid.Empty)
        {
            AddError("id", "Id is required.");
        }

        if (addMoveIds.Count == 0 && removeMoveIds.Count == 0)
        {
            AddError("moveIds", "At least one move id must be provided to add or remove learnable moves.");
        }

        if (addMoveIds.Any(static moveId => moveId == Guid.Empty))
        {
            AddError("addMoveIds", "AddMoveIds cannot contain empty ids.");
        }

        if (removeMoveIds.Any(static moveId => moveId == Guid.Empty))
        {
            AddError("removeMoveIds", "RemoveMoveIds cannot contain empty ids.");
        }

        if (addMoveIds.Count != addMoveIds.Distinct().Count())
        {
            AddError("addMoveIds", "AddMoveIds cannot contain duplicates.");
        }

        if (removeMoveIds.Count != removeMoveIds.Distinct().Count())
        {
            AddError("removeMoveIds", "RemoveMoveIds cannot contain duplicates.");
        }

        var overlappingMoveIds = addMoveIds.Intersect(removeMoveIds).ToArray();
        if (overlappingMoveIds.Length > 0)
        {
            AddError("moveIds", "The same move id cannot be added and removed in the same request.");
        }

        return errors.ToDictionary(static pair => pair.Key, static pair => pair.Value.ToArray(), StringComparer.Ordinal);

        void AddError(string key, string message)
        {
            if (!errors.TryGetValue(key, out var messages))
            {
                messages = [];
                errors[key] = messages;
            }

            messages.Add(message);
        }
    }
}
