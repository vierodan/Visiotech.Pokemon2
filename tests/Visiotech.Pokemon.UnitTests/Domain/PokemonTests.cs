using Visiotech.Pokemon.Domain.Abstractions;
using Visiotech.Pokemon.Domain.Pokemons;
using PokemonAggregate = global::Visiotech.Pokemon.Domain.Pokemons.Pokemon;

namespace Visiotech.Pokemon.UnitTests.Domain;

public sealed class PokemonTests
{
    [Fact]
    public void Create_Should_Build_Aggregate_When_Arguments_Are_Valid()
    {
        var pokemon = PokemonAggregate.Create(
            Guid.NewGuid(),
            Name.Create("Pikachu"),
            PokemonType.Electric,
            Level.Create(25),
            BaseStats.Create(35, 55, 40, 50, 50, 90),
            [
                Move.Create("Thunderbolt", PokemonType.Electric, 90),
                Move.Create("Quick Attack", PokemonType.Normal, 40)
            ],
            [
                Ability.Create("Static")
            ]);

        Assert.Equal("Pikachu", pokemon.Name.Value);
        Assert.Equal(PokemonType.Electric, pokemon.Type);
        Assert.Equal(25, pokemon.Level.Value);
        Assert.Equal(90, pokemon.Stats.Speed);
        Assert.Equal(2, pokemon.Moves.Count);
    }

    [Fact]
    public void Create_Should_Reject_More_Than_Four_Moves()
    {
        var action = () => PokemonAggregate.Create(
            Guid.NewGuid(),
            Name.Create("Bulbasaur"),
            PokemonType.Grass,
            Level.Create(15),
            BaseStats.Create(45, 49, 49, 65, 65, 45),
            [
                Move.Create("Tackle", PokemonType.Normal, 40),
                Move.Create("Growl", PokemonType.Normal, 20),
                Move.Create("Vine Whip", PokemonType.Grass, 45),
                Move.Create("Razor Leaf", PokemonType.Grass, 55),
                Move.Create("Solar Beam", PokemonType.Grass, 120)
            ],
            [
                Ability.Create("Overgrow")
            ]);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Contains("moves", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
