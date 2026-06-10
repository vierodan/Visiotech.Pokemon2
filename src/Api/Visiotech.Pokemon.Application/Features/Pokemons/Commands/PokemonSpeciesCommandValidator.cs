using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Domain.Abstractions;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Commands;

internal static class PokemonSpeciesCommandValidator
{
    public static IReadOnlyDictionary<string, string[]> Validate(
        string? name,
        IReadOnlyCollection<string>? types,
        int health,
        int attack,
        int defense,
        int specialAttack,
        int specialDefense,
        int speed)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(name))
        {
            AddError(errors, "name", "Name is required.");
        }

        if (types is null || types.Count == 0)
        {
            AddError(errors, "types", "At least one type is required.");
        }
        else
        {
            if (types.Count > 2)
            {
                AddError(errors, "types", "No more than 2 types are allowed.");
            }

            if (types.Any(string.IsNullOrWhiteSpace))
            {
                AddError(errors, "types", "Types cannot contain empty values.");
            }

            var normalizedTypes = types
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

        ValidatePositive(errors, health, "baseStats.health");
        ValidatePositive(errors, attack, "baseStats.attack");
        ValidatePositive(errors, defense, "baseStats.defense");
        ValidatePositive(errors, specialAttack, "baseStats.specialAttack");
        ValidatePositive(errors, specialDefense, "baseStats.specialDefense");
        ValidatePositive(errors, speed, "baseStats.speed");

        return errors.ToDictionary(static pair => pair.Key, static pair => pair.Value.ToArray(), StringComparer.OrdinalIgnoreCase);
    }

    public static PokemonSpeciesCommandInput BuildInput(
        string name,
        IReadOnlyCollection<string> types,
        int health,
        int attack,
        int defense,
        int specialAttack,
        int specialDefense,
        int speed)
    {
        try
        {
            return new PokemonSpeciesCommandInput(
                Name.Create(name),
                PokemonTyping.Create(types.Select(ParseType)),
                BaseStats.Create(health, attack, defense, specialAttack, specialDefense, speed));
        }
        catch (DomainException exception)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["pokemonSpecies"] = [exception.Message]
            });
        }
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
