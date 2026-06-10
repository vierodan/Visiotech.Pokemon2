using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Domain.Abstractions;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Commands.CreatePokemonSpecies;

public sealed class CreatePokemonSpeciesCommandHandler(
    IPokemonSpeciesWriteRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreatePokemonSpeciesCommand, PokemonSpeciesResponse>
{
    public async Task<PokemonSpeciesResponse> Handle(
        CreatePokemonSpeciesCommand command,
        CancellationToken cancellationToken)
    {
        var errors = Validate(command);
        if (errors.Count > 0)
        {
            throw new ApplicationValidationException(errors);
        }

        try
        {
            var name = Name.Create(command.Name!);
            var typing = PokemonTyping.Create(command.Types!.Select(ParseType));
            var baseStats = BaseStats.Create(
                command.Health,
                command.Attack,
                command.Defense,
                command.SpecialAttack,
                command.SpecialDefense,
                command.Speed);

            if (await repository.ExistsByNormalizedNameAsync(name.NormalizedValue, cancellationToken))
            {
                throw new ApplicationConflictException(
                    $"Pokemon species '{name.Value}' already exists.",
                    "name");
            }

            var pokemonSpecies = PokemonSpecies.Create(Guid.NewGuid(), name, typing, baseStats);
            await repository.AddAsync(pokemonSpecies, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Map(pokemonSpecies);
        }
        catch (DomainException exception)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["pokemonSpecies"] = [exception.Message]
            });
        }
    }

    private static PokemonSpeciesResponse Map(PokemonSpecies pokemonSpecies) =>
        new(
            pokemonSpecies.Id,
            pokemonSpecies.Name.Value,
            pokemonSpecies.Types.Select(static type => type.ToString()).ToArray(),
            new PokemonBaseStatsResponse(
                pokemonSpecies.BaseStats.Health,
                pokemonSpecies.BaseStats.Attack,
                pokemonSpecies.BaseStats.Defense,
                pokemonSpecies.BaseStats.SpecialAttack,
                pokemonSpecies.BaseStats.SpecialDefense,
                pokemonSpecies.BaseStats.Speed));

    private static IReadOnlyDictionary<string, string[]> Validate(CreatePokemonSpeciesCommand command)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(command.Name))
        {
            AddError(errors, "name", "Name is required.");
        }

        if (command.Types is null || command.Types.Count == 0)
        {
            AddError(errors, "types", "At least one type is required.");
        }
        else
        {
            if (command.Types.Count > 2)
            {
                AddError(errors, "types", "No more than 2 types are allowed.");
            }

            if (command.Types.Any(string.IsNullOrWhiteSpace))
            {
                AddError(errors, "types", "Types cannot contain empty values.");
            }

            var normalizedTypes = command.Types
                .Where(static type => !string.IsNullOrWhiteSpace(type))
                .Select(static type => type.Trim())
                .ToArray();

            var invalidTypes = normalizedTypes
                .Where(type => !PokemonTypeCatalog.TryParse(type, out _))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (invalidTypes.Length > 0)
            {
                AddError(
                    errors,
                    "types",
                    $"Unsupported types: {string.Join(", ", invalidTypes)}. Allowed values: {string.Join(", ", PokemonTypeCatalog.AllowedNames)}.");
            }

            if (normalizedTypes
                .GroupBy(static type => type, StringComparer.OrdinalIgnoreCase)
                .Any(static group => group.Count() > 1))
            {
                AddError(errors, "types", "Types must be unique.");
            }
        }

        ValidatePositive(errors, command.Health, "baseStats.health");
        ValidatePositive(errors, command.Attack, "baseStats.attack");
        ValidatePositive(errors, command.Defense, "baseStats.defense");
        ValidatePositive(errors, command.SpecialAttack, "baseStats.specialAttack");
        ValidatePositive(errors, command.SpecialDefense, "baseStats.specialDefense");
        ValidatePositive(errors, command.Speed, "baseStats.speed");

        return errors.ToDictionary(static pair => pair.Key, static pair => pair.Value.ToArray(), StringComparer.OrdinalIgnoreCase);
    }

    private static PokemonType ParseType(string type)
    {
        if (!PokemonTypeCatalog.TryParse(type, out var pokemonType))
        {
            throw new DomainException($"Unsupported pokemon type '{type}'.");
        }

        return pokemonType;
    }

    private static void ValidatePositive(IDictionary<string, List<string>> errors, int value, string key)
    {
        if (value <= 0)
        {
            AddError(errors, key, "Value must be greater than 0.");
        }
    }

    private static void AddError(IDictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var messages))
        {
            messages = [];
            errors[key] = messages;
        }

        messages.Add(message);
    }
}
