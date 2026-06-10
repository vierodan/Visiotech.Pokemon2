namespace Visiotech.Pokemon.Domain.Pokemons;

public static class MoveCategoryCatalog
{
    public static IReadOnlyCollection<string> AllowedNames { get; } =
        Enum.GetNames<MoveCategory>();

    public static bool TryParse(string category, out MoveCategory moveCategory) =>
        Enum.TryParse(category, ignoreCase: true, out moveCategory)
        && Enum.IsDefined(moveCategory);
}
