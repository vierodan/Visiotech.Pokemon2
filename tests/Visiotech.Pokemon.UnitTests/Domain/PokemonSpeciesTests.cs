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
}
