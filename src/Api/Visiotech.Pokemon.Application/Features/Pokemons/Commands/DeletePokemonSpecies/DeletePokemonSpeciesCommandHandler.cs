using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Commands.DeletePokemonSpecies;

public sealed class DeletePokemonSpeciesCommandHandler(
    IPokemonSpeciesWriteRepository repository,
    IPokemonSpeciesDeletionDependencyChecker dependencyChecker,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeletePokemonSpeciesCommand, Guid>
{
    public async Task<Guid> Handle(DeletePokemonSpeciesCommand command, CancellationToken cancellationToken)
    {
        if (command.Id == Guid.Empty)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["id"] = ["Id is required."]
            });
        }

        var pokemonSpecies = await repository.GetForUpdateAsync(command.Id, cancellationToken);
        if (pokemonSpecies is null)
        {
            throw new ApplicationNotFoundException(
                $"Pokemon species '{command.Id}' was not found.",
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

        repository.Remove(pokemonSpecies);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return command.Id;
    }
}
