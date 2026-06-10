using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.MyPokemons.Commands;
using Visiotech.Pokemon.Application.Features.MyPokemons.Queries;

namespace Visiotech.Pokemon.Application.Features.MyPokemons.Commands.CreateMyPokemon;

public sealed class CreateMyPokemonCommandHandler(
    IMyPokemonWriteRepository repository,
    IPokemonSpeciesReadRepository speciesRepository,
    IPokemonMoveReadRepository moveRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateMyPokemonCommand, MyPokemonResponse>
{
    public async Task<MyPokemonResponse> Handle(
        CreateMyPokemonCommand command,
        CancellationToken cancellationToken)
    {
        var errors = MyPokemonCommandValidator.Validate(
            command.PokemonSpeciesId,
            command.Level,
            command.CurrentHealthPoints,
            command.TotalHealthPoints,
            command.EquippedMoveIds);

        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }

        var input = MyPokemonCommandValidator.BuildInput(
            command.PokemonSpeciesId,
            command.Level,
            command.CurrentHealthPoints,
            command.TotalHealthPoints,
            command.EquippedMoveIds!);

        var pokemonSpecies = await speciesRepository.GetByIdWithLearnableMovesAsync(input.PokemonSpeciesId, cancellationToken);
        if (pokemonSpecies is null)
        {
            throw new ApplicationNotFoundException(
                $"Pokemon species '{input.PokemonSpeciesId}' was not found.",
                "pokemonSpeciesId");
        }

        var requestedMoveIds = input.EquippedMoveIds.Distinct().ToArray();
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

        var myPokemon = global::Visiotech.Pokemon.Domain.Pokemons.MyPokemon.Create(
            Guid.NewGuid(),
            pokemonSpecies.Id,
            input.Level,
            input.CurrentHealthPoints,
            input.TotalHealthPoints,
            input.EquippedMoveIds);

        await repository.AddAsync(myPokemon, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MyPokemonMapping.ToResponse(myPokemon, pokemonSpecies, requestedMoves);
    }
}
