using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Domain.Pokemons;
using PokemonAggregate = global::Visiotech.Pokemon.Domain.Pokemons.Pokemon;

namespace Visiotech.Pokemon.Infrastructure.Persistence.InMemory;

public sealed class InMemoryPokemonReadRepository : IPokemonReadRepository
{
    private static readonly IReadOnlyCollection<PokemonAggregate> Seed =
    [
        PokemonAggregate.Create(
            Guid.Parse("A4D380B9-84C4-4B80-8457-08AAE9E04001"),
            Name.Create("Charizard"),
            PokemonType.Fire,
            Level.Create(50),
            BaseStats.Create(78, 84, 78, 109, 85, 100),
            [
                Move.Create("Flamethrower", PokemonType.Fire, 90),
                Move.Create("Air Slash", PokemonType.Flying, 75),
                Move.Create("Slash", PokemonType.Normal, 70),
                Move.Create("Heat Wave", PokemonType.Fire, 95)
            ],
            [
                Ability.Create("Blaze"),
                Ability.Create("Solar Power")
            ]),
        PokemonAggregate.Create(
            Guid.Parse("A4D380B9-84C4-4B80-8457-08AAE9E04002"),
            Name.Create("Blastoise"),
            PokemonType.Water,
            Level.Create(50),
            BaseStats.Create(79, 83, 100, 85, 105, 78),
            [
                Move.Create("Surf", PokemonType.Water, 90),
                Move.Create("Bite", PokemonType.Normal, 60),
                Move.Create("Hydro Pump", PokemonType.Water, 110)
            ],
            [
                Ability.Create("Torrent"),
                Ability.Create("Rain Dish")
            ]),
        PokemonAggregate.Create(
            Guid.Parse("A4D380B9-84C4-4B80-8457-08AAE9E04003"),
            Name.Create("Venusaur"),
            PokemonType.Grass,
            Level.Create(50),
            BaseStats.Create(80, 82, 83, 100, 100, 80),
            [
                Move.Create("Solar Beam", PokemonType.Grass, 120),
                Move.Create("Vine Whip", PokemonType.Grass, 45),
                Move.Create("Razor Leaf", PokemonType.Grass, 55)
            ],
            [
                Ability.Create("Overgrow"),
                Ability.Create("Chlorophyll")
            ])
    ];

    public Task<IReadOnlyCollection<PokemonAggregate>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Seed);
}
