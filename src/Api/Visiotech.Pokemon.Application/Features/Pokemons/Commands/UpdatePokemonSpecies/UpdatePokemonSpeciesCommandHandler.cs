using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Commands.UpdatePokemonSpecies;

public sealed class UpdatePokemonSpeciesCommandHandler(
    IPokemonSpeciesWriteRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdatePokemonSpeciesCommand, PokemonSpeciesResponse>
{
    public async Task<PokemonSpeciesResponse> Handle(
        UpdatePokemonSpeciesCommand command,
        CancellationToken cancellationToken)
    {
        var errors = PokemonSpeciesCommandValidator.Validate(
            command.Name,
            command.Types,
            command.Health,
            command.Attack,
            command.Defense,
            command.SpecialAttack,
            command.SpecialDefense,
            command.Speed);

        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }

        var pokemonSpecies = await repository.GetForUpdateAsync(command.Id, cancellationToken);
        if (pokemonSpecies is null)
        {
            throw new ApplicationNotFoundException(
                $"Pokemon species '{command.Id}' was not found.",
                "id");
        }

        var input = PokemonSpeciesCommandValidator.BuildInput(
            command.Name!,
            command.Types!,
            command.Health,
            command.Attack,
            command.Defense,
            command.SpecialAttack,
            command.SpecialDefense,
            command.Speed);

        if (await repository.ExistsByNormalizedNameAsync(input.Name.NormalizedValue, pokemonSpecies.Id, cancellationToken))
        {
            throw new ApplicationConflictException(
                $"Pokemon species '{input.Name.Value}' already exists.",
                "name");
        }

        pokemonSpecies.Rename(input.Name);
        pokemonSpecies.ReconfigureTyping(input.Typing);
        pokemonSpecies.ReconfigureBaseStats(input.BaseStats);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return PokemonSpeciesMapping.ToResponse(pokemonSpecies);
    }
}
