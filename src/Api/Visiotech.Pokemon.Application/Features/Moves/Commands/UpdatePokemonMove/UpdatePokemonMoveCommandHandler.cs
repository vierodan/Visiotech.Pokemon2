using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Moves.Queries;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.Moves.Commands.UpdatePokemonMove;

public sealed class UpdatePokemonMoveCommandHandler(
    IPokemonMoveWriteRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdatePokemonMoveCommand, PokemonMoveResponse>
{
    public async Task<PokemonMoveResponse> Handle(
        UpdatePokemonMoveCommand command,
        CancellationToken cancellationToken)
    {
        var errors = PokemonMoveCommandValidator.Validate(
            command.Name,
            command.Type,
            command.Category,
            command.Power);

        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }

        var pokemonMove = await repository.GetForUpdateAsync(command.Id, cancellationToken);
        if (pokemonMove is null)
        {
            throw new ApplicationNotFoundException(
                $"Pokemon move '{command.Id}' was not found.",
                "id");
        }

        var input = PokemonMoveCommandValidator.BuildInput(
            command.Name!,
            command.Type!,
            command.Category!,
            command.Power);

        if (await repository.ExistsByNormalizedNameAsync(input.Name.NormalizedValue, pokemonMove.Id, cancellationToken))
        {
            throw new ApplicationConflictException(
                $"Pokemon move '{input.Name.Value}' already exists.",
                "name");
        }

        pokemonMove.Reconfigure(Move.Create(input.Name, input.Type, input.Category, input.Power));
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return PokemonMoveMapping.ToResponse(pokemonMove);
    }
}
