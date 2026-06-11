using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;

namespace Visiotech.Pokemon.Application.Features.MyPokemons.Commands.DeleteMyPokemon;

public sealed class DeleteMyPokemonCommandHandler(
    IMyPokemonWriteRepository repository,
    IMyPokemonDeletionDependencyChecker dependencyChecker,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteMyPokemonCommand, Guid>
{
    public async Task<Guid> Handle(DeleteMyPokemonCommand command, CancellationToken cancellationToken)
    {
        if (command.Id == Guid.Empty)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["id"] = ["Id is required."]
            });
        }

        var myPokemon = await repository.GetForUpdateAsync(command.Id, cancellationToken);
        if (myPokemon is null)
        {
            throw new ApplicationNotFoundException(
                $"My pokemon '{command.Id}' was not found.",
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

        repository.Remove(myPokemon);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}
