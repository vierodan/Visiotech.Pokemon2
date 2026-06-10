using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Infrastructure.Persistence;

public static class PokemonMvpRosterSeed
{
    public static IReadOnlyCollection<PokemonSpecies> GetSpecies() =>
    [
        PokemonSpecies.Create(
            Guid.Parse("A4D380B9-84C4-4B80-8457-08AAE9E04001"),
            Name.Create("Charizard"),
            PokemonTyping.Create([PokemonType.Fire, PokemonType.Flying]),
            BaseStats.Create(78, 84, 78, 109, 85, 100)),
        PokemonSpecies.Create(
            Guid.Parse("A4D380B9-84C4-4B80-8457-08AAE9E04002"),
            Name.Create("Blastoise"),
            PokemonTyping.Create([PokemonType.Water]),
            BaseStats.Create(79, 83, 100, 85, 105, 78)),
        PokemonSpecies.Create(
            Guid.Parse("A4D380B9-84C4-4B80-8457-08AAE9E04003"),
            Name.Create("Venusaur"),
            PokemonTyping.Create([PokemonType.Grass, PokemonType.Poison]),
            BaseStats.Create(80, 82, 83, 100, 100, 80)),
        PokemonSpecies.Create(
            Guid.Parse("A4D380B9-84C4-4B80-8457-08AAE9E04004"),
            Name.Create("Pikachu"),
            PokemonTyping.Create([PokemonType.Electric]),
            BaseStats.Create(35, 55, 40, 50, 50, 90)),
        PokemonSpecies.Create(
            Guid.Parse("A4D380B9-84C4-4B80-8457-08AAE9E04005"),
            Name.Create("Gengar"),
            PokemonTyping.Create([PokemonType.Ghost, PokemonType.Poison]),
            BaseStats.Create(60, 65, 60, 130, 75, 110)),
        PokemonSpecies.Create(
            Guid.Parse("A4D380B9-84C4-4B80-8457-08AAE9E04006"),
            Name.Create("Golem"),
            PokemonTyping.Create([PokemonType.Rock, PokemonType.Ground]),
            BaseStats.Create(80, 120, 130, 55, 65, 45)),
        PokemonSpecies.Create(
            Guid.Parse("A4D380B9-84C4-4B80-8457-08AAE9E04007"),
            Name.Create("Alakazam"),
            PokemonTyping.Create([PokemonType.Psychic]),
            BaseStats.Create(55, 50, 45, 135, 95, 120)),
        PokemonSpecies.Create(
            Guid.Parse("A4D380B9-84C4-4B80-8457-08AAE9E04008"),
            Name.Create("Machamp"),
            PokemonTyping.Create([PokemonType.Fighting]),
            BaseStats.Create(90, 130, 80, 65, 85, 55)),
        PokemonSpecies.Create(
            Guid.Parse("A4D380B9-84C4-4B80-8457-08AAE9E04009"),
            Name.Create("Dragonite"),
            PokemonTyping.Create([PokemonType.Dragon, PokemonType.Flying]),
            BaseStats.Create(91, 134, 95, 100, 100, 80)),
        PokemonSpecies.Create(
            Guid.Parse("A4D380B9-84C4-4B80-8457-08AAE9E04010"),
            Name.Create("Snorlax"),
            PokemonTyping.Create([PokemonType.Normal]),
            BaseStats.Create(160, 110, 65, 65, 110, 30))
    ];
}
