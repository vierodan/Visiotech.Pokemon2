using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.MyPokemons.Commands;
using Visiotech.Pokemon.Application.Features.MyPokemons.Queries;

namespace Visiotech.Pokemon.Application.Features.MyPokemons.Commands.UpdateMyPokemon;

public sealed class UpdateMyPokemonCommandHandler(
    IMyPokemonWriteRepository repository,
    IPokemonSpeciesReadRepository speciesRepository,
    IPokemonMoveReadRepository moveRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateMyPokemonCommand, MyPokemonResponse>
{
    public async Task<MyPokemonResponse> Handle(
        UpdateMyPokemonCommand command,
        CancellationToken cancellationToken)
    {
        var errors = MyPokemonCommandValidator.Validate(
            command.Level,
            command.CurrentHealthPoints,
            command.TotalHealthPoints,
            command.EquippedMoveIds);

        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }

        var myPokemon = await repository.GetForUpdateAsync(command.Id, cancellationToken);
        if (myPokemon is null)
        {
            throw new ApplicationNotFoundException(
                $"My pokemon '{command.Id}' was not found.",
                "id");
        }

        var input = MyPokemonCommandValidator.BuildInput(
            myPokemon.PokemonSpeciesId,
            command.Level,
            command.CurrentHealthPoints,
            command.TotalHealthPoints,
            command.EquippedMoveIds!);

        var context = await MyPokemonCommandGuard.ResolveSpeciesAndMovesAsync(
            myPokemon.PokemonSpeciesId,
            input.EquippedMoveIds,
            speciesRepository,
            moveRepository,
            cancellationToken);

        myPokemon.Reconfigure(
            input.Level,
            input.CurrentHealthPoints,
            input.TotalHealthPoints,
            input.EquippedMoveIds);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return MyPokemonMapping.ToResponse(myPokemon, context.Species, context.EquippedMoves);
    }
}
