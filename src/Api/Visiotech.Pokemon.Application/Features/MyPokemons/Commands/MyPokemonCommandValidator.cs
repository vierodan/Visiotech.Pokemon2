using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Domain.Abstractions;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Application.Features.MyPokemons.Commands;

internal static class MyPokemonCommandValidator
{
    public static IReadOnlyDictionary<string, string[]> Validate(
        Guid pokemonSpeciesId,
        int level,
        int currentHealthPoints,
        int totalHealthPoints,
        IReadOnlyCollection<Guid>? equippedMoveIds)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        if (pokemonSpeciesId == Guid.Empty)
        {
            AddError(errors, "pokemonSpeciesId", "PokemonSpeciesId is required.");
        }

        AppendPlayableStateErrors(errors, level, currentHealthPoints, totalHealthPoints, equippedMoveIds);

        return errors.ToDictionary(static pair => pair.Key, static pair => pair.Value.ToArray(), StringComparer.OrdinalIgnoreCase);
    }

    public static IReadOnlyDictionary<string, string[]> Validate(
        int level,
        int currentHealthPoints,
        int totalHealthPoints,
        IReadOnlyCollection<Guid>? equippedMoveIds)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        AppendPlayableStateErrors(errors, level, currentHealthPoints, totalHealthPoints, equippedMoveIds);

        return errors.ToDictionary(static pair => pair.Key, static pair => pair.Value.ToArray(), StringComparer.OrdinalIgnoreCase);
    }

    private static void AppendPlayableStateErrors(
        IDictionary<string, List<string>> errors,
        int level,
        int currentHealthPoints,
        int totalHealthPoints,
        IReadOnlyCollection<Guid>? equippedMoveIds)
    {

        if (level is < 1 or > 100)
        {
            AddError(errors, "level", "Level must be between 1 and 100.");
        }

        if (currentHealthPoints <= 0)
        {
            AddError(errors, "currentHealthPoints", "CurrentHealthPoints must be greater than 0.");
        }

        if (totalHealthPoints <= 0)
        {
            AddError(errors, "totalHealthPoints", "TotalHealthPoints must be greater than 0.");
        }

        if (currentHealthPoints > 0 && totalHealthPoints > 0 && currentHealthPoints > totalHealthPoints)
        {
            AddError(errors, "currentHealthPoints", "CurrentHealthPoints cannot exceed TotalHealthPoints.");
        }

        if (equippedMoveIds is null || equippedMoveIds.Count == 0)
        {
            AddError(errors, "equippedMoveIds", "At least one equipped move is required.");
        }
        else
        {
            if (equippedMoveIds.Count > 4)
            {
                AddError(errors, "equippedMoveIds", "No more than 4 equipped moves are allowed.");
            }

            if (equippedMoveIds.Any(moveId => moveId == Guid.Empty))
            {
                AddError(errors, "equippedMoveIds", "EquippedMoveIds cannot contain empty ids.");
            }

            if (equippedMoveIds.Count != equippedMoveIds.Distinct().Count())
            {
                AddError(errors, "equippedMoveIds", "EquippedMoveIds must be unique.");
            }
        }
    }

    public static MyPokemonCommandInput BuildInput(
        Guid pokemonSpeciesId,
        int level,
        int currentHealthPoints,
        int totalHealthPoints,
        IReadOnlyCollection<Guid> equippedMoveIds)
    {
        try
        {
            return new MyPokemonCommandInput(
                pokemonSpeciesId,
                Level.Create(level),
                currentHealthPoints,
                totalHealthPoints,
                equippedMoveIds.ToArray());
        }
        catch (DomainException exception)
        {
            throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["myPokemon"] = [exception.Message]
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
