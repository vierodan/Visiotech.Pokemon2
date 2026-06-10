namespace Visiotech.Pokemon.Domain.Pokemons;

public static class PokemonTypeCatalog
{
    private static readonly IReadOnlyDictionary<string, PokemonType> TypesByName =
        Enum.GetValues<PokemonType>()
            .ToDictionary(type => type.ToString(), type => type, StringComparer.OrdinalIgnoreCase);

    public static IReadOnlyCollection<string> AllowedNames { get; } =
        TypesByName.Keys.OrderBy(static name => name, StringComparer.Ordinal).ToArray();

    public static bool TryParse(string? value, out PokemonType type)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            type = default;
            return false;
        }

        return TypesByName.TryGetValue(value.Trim(), out type);
    }
}
