using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Infrastructure.Persistence;

public static class PokemonMvpMoveSeed
{
    public static IReadOnlyCollection<PokemonMove> GetMoves() =>
    [
        Create("Flamethrower", PokemonType.Fire, MoveCategory.Special, 90, "A4D380B9-84C4-4B80-8457-08AAE9E05001"),
        Create("Fire Blast", PokemonType.Fire, MoveCategory.Special, 110, "A4D380B9-84C4-4B80-8457-08AAE9E05002"),
        Create("Fly", PokemonType.Flying, MoveCategory.Physical, 90, "A4D380B9-84C4-4B80-8457-08AAE9E05003"),
        Create("Hyper Beam", PokemonType.Normal, MoveCategory.Special, 150, "A4D380B9-84C4-4B80-8457-08AAE9E05004"),
        Create("Solar Beam", PokemonType.Grass, MoveCategory.Special, 120, "A4D380B9-84C4-4B80-8457-08AAE9E05005"),
        Create("Surf", PokemonType.Water, MoveCategory.Special, 90, "A4D380B9-84C4-4B80-8457-08AAE9E05006"),
        Create("Hydro Pump", PokemonType.Water, MoveCategory.Special, 110, "A4D380B9-84C4-4B80-8457-08AAE9E05007"),
        Create("Ice Beam", PokemonType.Ice, MoveCategory.Special, 90, "A4D380B9-84C4-4B80-8457-08AAE9E05008"),
        Create("Sleep Powder", PokemonType.Grass, MoveCategory.Status, 0, "A4D380B9-84C4-4B80-8457-08AAE9E05009"),
        Create("Seed Bomb", PokemonType.Grass, MoveCategory.Physical, 80, "A4D380B9-84C4-4B80-8457-08AAE9E05010"),
        Create("Sludge Wave", PokemonType.Poison, MoveCategory.Special, 95, "A4D380B9-84C4-4B80-8457-08AAE9E05011"),
        Create("Thunderbolt", PokemonType.Electric, MoveCategory.Special, 90, "A4D380B9-84C4-4B80-8457-08AAE9E05012"),
        Create("Discharge", PokemonType.Electric, MoveCategory.Special, 80, "A4D380B9-84C4-4B80-8457-08AAE9E05013"),
        Create("Shadow Ball", PokemonType.Ghost, MoveCategory.Special, 80, "A4D380B9-84C4-4B80-8457-08AAE9E05014"),
        Create("Dark Pulse", PokemonType.Dark, MoveCategory.Special, 80, "A4D380B9-84C4-4B80-8457-08AAE9E05015"),
        Create("Poison Jab", PokemonType.Poison, MoveCategory.Physical, 80, "A4D380B9-84C4-4B80-8457-08AAE9E05016"),
        Create("Psychic", PokemonType.Psychic, MoveCategory.Special, 90, "A4D380B9-84C4-4B80-8457-08AAE9E05017"),
        Create("Close Combat", PokemonType.Fighting, MoveCategory.Physical, 120, "A4D380B9-84C4-4B80-8457-08AAE9E05018"),
        Create("Drain Punch", PokemonType.Fighting, MoveCategory.Physical, 75, "A4D380B9-84C4-4B80-8457-08AAE9E05019"),
        Create("Earthquake", PokemonType.Ground, MoveCategory.Physical, 100, "A4D380B9-84C4-4B80-8457-08AAE9E05020"),
        Create("Bulldoze", PokemonType.Ground, MoveCategory.Physical, 60, "A4D380B9-84C4-4B80-8457-08AAE9E05021"),
        Create("Protect", PokemonType.Normal, MoveCategory.Status, 0, "A4D380B9-84C4-4B80-8457-08AAE9E05022"),
        Create("Air Slash", PokemonType.Flying, MoveCategory.Special, 75, "A4D380B9-84C4-4B80-8457-08AAE9E05023"),
        Create("Thunder Punch", PokemonType.Electric, MoveCategory.Physical, 75, "A4D380B9-84C4-4B80-8457-08AAE9E05024"),
        Create("Ice Punch", PokemonType.Ice, MoveCategory.Physical, 75, "A4D380B9-84C4-4B80-8457-08AAE9E05025"),
        Create("Body Slam", PokemonType.Normal, MoveCategory.Physical, 85, "A4D380B9-84C4-4B80-8457-08AAE9E05026"),
        Create("Rest", PokemonType.Psychic, MoveCategory.Status, 0, "A4D380B9-84C4-4B80-8457-08AAE9E05027")
    ];

    private static PokemonMove Create(
        string name,
        PokemonType type,
        MoveCategory category,
        int power,
        string id) =>
        PokemonMove.Create(
            Guid.Parse(id),
            Move.Create(name, type, category, power));
}
