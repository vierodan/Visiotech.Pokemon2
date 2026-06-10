using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Domain.Abstractions;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.Moves.Commands;

internal static class PokemonMoveCommandValidator
{
    public static IReadOnlyDictionary<string, string[]> Validate(
        string? name,
        string? type,
        string? category,
        int power)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(name))
        {
            AddError(errors, "name", "Name is required.");
        }
        else if (name.Trim().Length > 100)
        {
            AddError(errors, "name", "Name cannot exceed 100 characters.");
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            AddError(errors, "type", "Type is required.");
        }
        else if (!PokemonTypeCatalog.TryParse(type.Trim(), out _))
        {
            AddError(
                errors,
                "type",
                $"Unsupported type '{type}'. Allowed values: {string.Join(", ", PokemonTypeCatalog.AllowedNames)}.");
        }

        MoveCategory? parsedCategory = null;

        if (string.IsNullOrWhiteSpace(category))
        {
            AddError(errors, "category", "Category is required.");
        }
        else if (!MoveCategoryCatalog.TryParse(category.Trim(), out var moveCategory))
        {
            AddError(
                errors,
                "category",
                $"Unsupported category '{category}'. Allowed values: {string.Join(", ", MoveCategoryCatalog.AllowedNames)}.");
        }
        else
        {
            parsedCategory = moveCategory;
        }

        if (parsedCategory is MoveCategory.Status && power != 0)
        {
            AddError(errors, "power", "Power must be 0 for Status moves.");
        }
        else if (parsedCategory is MoveCategory.Physical or MoveCategory.Special && power <= 0)
        {
            AddError(errors, "power", "Power must be greater than 0 for Physical or Special moves.");
        }

        return errors.ToDictionary(static pair => pair.Key, static pair => pair.Value.ToArray(), StringComparer.OrdinalIgnoreCase);
    }

    public static PokemonMoveCommandInput BuildInput(
        string name,
        string type,
        string category,
        int power)
    {
        try
        {
            if (!PokemonTypeCatalog.TryParse(type, out var pokemonType))
            {
                throw new DomainException($"Unsupported pokemon type '{type}'.");
            }

            if (!MoveCategoryCatalog.TryParse(category, out var moveCategory))
            {
                throw new DomainException($"Unsupported move category '{category}'.");
            }

            var move = Move.Create(name, pokemonType, moveCategory, power);

            return new PokemonMoveCommandInput(
                move.Name,
                move.Type,
                move.Category,
                move.Power);
        }
        catch (DomainException exception)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["pokemonMove"] = [exception.Message]
            });
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
