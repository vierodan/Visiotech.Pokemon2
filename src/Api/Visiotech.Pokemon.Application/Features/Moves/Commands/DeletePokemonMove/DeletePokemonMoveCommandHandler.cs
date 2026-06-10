using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;

namespace Visiotech.Pokemon.Application.Features.Moves.Commands.DeletePokemonMove;

public sealed class DeletePokemonMoveCommandHandler(
    IPokemonMoveWriteRepository repository,
    IPokemonMoveDeletionDependencyChecker dependencyChecker,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeletePokemonMoveCommand, Guid>
{
    public async Task<Guid> Handle(DeletePokemonMoveCommand command, CancellationToken cancellationToken)
    {
        if (command.Id == Guid.Empty)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["id"] = ["Id is required."]
            });
        }

        var pokemonMove = await repository.GetForUpdateAsync(command.Id, cancellationToken);
        if (pokemonMove is null)
        {
            throw new ApplicationNotFoundException(
                $"Pokemon move '{command.Id}' was not found.",
                "id");
        }

        var blockingReasons = await dependencyChecker.GetBlockingReasonsAsync(command.Id, cancellationToken);
        if (blockingReasons.Count > 0)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["dependencies"] = blockingReasons.ToArray()
            });
        }

        repository.Remove(pokemonMove);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}
