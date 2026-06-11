using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Battles;
using Visiotech.Pokemon.Domain.Battles;

namespace Visiotech.Pokemon.Application.Features.Battles.Commands.CreateBattle;

public sealed class CreateBattleCommandHandler(
    IBattleWriteRepository repository,
    IMyPokemonReadRepository myPokemonRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateBattleCommand, BattleResponse>
{
    public async Task<BattleResponse> Handle(
        CreateBattleCommand command,
        CancellationToken cancellationToken)
    {
        var errors = Validate(command);
        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }

        var firstMyPokemon = await myPokemonRepository.GetByIdAsync(command.FirstMyPokemonId, cancellationToken);
        if (firstMyPokemon is null)
        {
            throw new ApplicationNotFoundException(
                $"My pokemon '{command.FirstMyPokemonId}' was not found.",
                "firstMyPokemonId");
        }

        var secondMyPokemon = await myPokemonRepository.GetByIdAsync(command.SecondMyPokemonId, cancellationToken);
        if (secondMyPokemon is null)
        {
            throw new ApplicationNotFoundException(
                $"My pokemon '{command.SecondMyPokemonId}' was not found.",
                "secondMyPokemonId");
        }

        var startErrors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        if (firstMyPokemon.CurrentHealthPoints <= 0)
        {
            startErrors["firstMyPokemonId"] =
                [$"My pokemon '{firstMyPokemon.Id}' must have current health points greater than 0 to start a battle."];
        }

        if (secondMyPokemon.CurrentHealthPoints <= 0)
        {
            startErrors["secondMyPokemonId"] =
                [$"My pokemon '{secondMyPokemon.Id}' must have current health points greater than 0 to start a battle."];
        }

        if (startErrors.Count > 0)
        {
            throw new ApplicationValidationException(startErrors);
        }

        Battle battle;

        try
        {
            battle = Battle.Create(
                Guid.NewGuid(),
                firstMyPokemon.Id,
                firstMyPokemon.CurrentHealthPoints,
                firstMyPokemon.TotalHealthPoints,
                secondMyPokemon.Id,
                secondMyPokemon.CurrentHealthPoints,
                secondMyPokemon.TotalHealthPoints);
        }
        catch (Exception exception) when (exception is Visiotech.Pokemon.Domain.Abstractions.DomainException)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["battle"] = [exception.Message]
            });
        }

        await repository.AddAsync(battle, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return BattleMapping.ToResponse(battle);
    }

    private static IReadOnlyDictionary<string, string[]> Validate(CreateBattleCommand command)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (command.FirstMyPokemonId == Guid.Empty)
        {
            errors["firstMyPokemonId"] = ["FirstMyPokemonId is required."];
        }

        if (command.SecondMyPokemonId == Guid.Empty)
        {
            errors["secondMyPokemonId"] = ["SecondMyPokemonId is required."];
        }

        if (command.FirstMyPokemonId != Guid.Empty &&
            command.SecondMyPokemonId != Guid.Empty &&
            command.FirstMyPokemonId == command.SecondMyPokemonId)
        {
            errors["secondMyPokemonId"] = ["SecondMyPokemonId must be different from FirstMyPokemonId."];
        }

        return errors;
    }
}
