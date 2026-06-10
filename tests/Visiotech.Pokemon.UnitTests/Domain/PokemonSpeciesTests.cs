using Visiotech.Pokemon.Domain.Abstractions;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Domain;

public sealed class PokemonSpeciesTests
{
    [Fact]
    public void Create_Should_Build_Aggregate_When_Arguments_Are_Valid()
    {
        var pokemonSpecies = PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create("Charizard"),
            PokemonTyping.Create([PokemonType.Fire, PokemonType.Flying]),
            BaseStats.Create(78, 84, 78, 109, 85, 100));

        Assert.Equal("Charizard", pokemonSpecies.Name.Value);
        Assert.Equal(["Fire", "Flying"], pokemonSpecies.Types.Select(type => type.ToString()).ToArray());
        Assert.Equal(100, pokemonSpecies.BaseStats.Speed);
    }

    [Fact]
    public void Create_Should_Reject_Duplicate_Types()
    {
        var action = () => PokemonTyping.Create([PokemonType.Fire, PokemonType.Fire]);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Contains("unique", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AddLearnableMove_Should_Reject_Duplicate_Move_Association()
    {
        var pokemonSpecies = PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create("Blastoise"),
            PokemonTyping.Create([PokemonType.Water]),
            BaseStats.Create(79, 83, 100, 85, 105, 78));
        var moveId = Guid.NewGuid();

        pokemonSpecies.AddLearnableMove(moveId);

        var action = () => pokemonSpecies.AddLearnableMove(moveId);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Contains("duplicate", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RemoveLearnableMove_Should_Remove_Existing_Association()
    {
        var pokemonSpecies = PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create("Venusaur"),
            PokemonTyping.Create([PokemonType.Grass, PokemonType.Poison]),
            BaseStats.Create(80, 82, 83, 100, 100, 80));
        var moveId = Guid.NewGuid();

        pokemonSpecies.AddLearnableMove(moveId);

        pokemonSpecies.RemoveLearnableMove(moveId);

        Assert.Empty(pokemonSpecies.LearnableMoveIds);
    }
}
