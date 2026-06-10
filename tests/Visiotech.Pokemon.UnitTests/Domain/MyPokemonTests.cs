using Visiotech.Pokemon.Domain.Abstractions;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Domain;

public sealed class MyPokemonTests
{
    [Fact]
    public void Create_Should_Build_Aggregate_When_Arguments_Are_Valid()
    {
        var myPokemon = MyPokemon.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Level.Create(50),
            120,
            150,
            [Guid.NewGuid(), Guid.NewGuid()]);

        Assert.Equal(50, myPokemon.Level.Value);
        Assert.Equal(120, myPokemon.CurrentHealthPoints);
        Assert.Equal(150, myPokemon.TotalHealthPoints);
        Assert.Equal(2, myPokemon.EquippedMoves.Count);
        Assert.Equal([1, 2], myPokemon.EquippedMoves.Select(slot => slot.SlotNumber).ToArray());
    }

    [Fact]
    public void Create_Should_Reject_Duplicate_Equipped_Moves()
    {
        var moveId = Guid.NewGuid();

        var action = () => MyPokemon.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Level.Create(25),
            60,
            80,
            [moveId, moveId]);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Contains("same move", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_Should_Reject_More_Than_Four_Moves()
    {
        var action = () => MyPokemon.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Level.Create(25),
            60,
            80,
            [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Contains("between 1 and 4", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_Should_Reject_Inconsistent_Health_Points()
    {
        var action = () => MyPokemon.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Level.Create(25),
            90,
            80,
            [Guid.NewGuid()]);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Contains("cannot exceed", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
