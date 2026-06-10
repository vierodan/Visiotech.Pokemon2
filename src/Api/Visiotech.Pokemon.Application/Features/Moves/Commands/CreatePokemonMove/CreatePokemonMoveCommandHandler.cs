using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Moves.Queries;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.Moves.Commands.CreatePokemonMove;

public sealed class CreatePokemonMoveCommandHandler(
    IPokemonMoveWriteRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreatePokemonMoveCommand, PokemonMoveResponse>
{
    public async Task<PokemonMoveResponse> Handle(
        CreatePokemonMoveCommand command,
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

        var input = PokemonMoveCommandValidator.BuildInput(
            command.Name!,
            command.Type!,
            command.Category!,
            command.Power);

        if (await repository.ExistsByNormalizedNameAsync(input.Name.NormalizedValue, cancellationToken))
        {
            throw new ApplicationConflictException(
                $"Pokemon move '{input.Name.Value}' already exists.",
                "name");
        }

        var pokemonMove = PokemonMove.Create(
            Guid.NewGuid(),
            Move.Create(input.Name, input.Type, input.Category, input.Power));

        await repository.AddAsync(pokemonMove, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return PokemonMoveMapping.ToResponse(pokemonMove);
    }
}
