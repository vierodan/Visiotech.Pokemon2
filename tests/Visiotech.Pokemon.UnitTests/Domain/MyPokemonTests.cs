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
    public void Create_Should_Allow_Current_Health_Points_Equal_To_Zero()
    {
        var myPokemon = MyPokemon.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Level.Create(50),
            0,
            150,
            [Guid.NewGuid()]);

        Assert.Equal(0, myPokemon.CurrentHealthPoints);
        Assert.Equal(150, myPokemon.TotalHealthPoints);
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

    [Fact]
    public void Reconfigure_Should_Update_Level_Health_And_Equipped_Moves_When_Arguments_Are_Valid()
    {
        var firstMoveId = Guid.NewGuid();
        var secondMoveId = Guid.NewGuid();
        var thirdMoveId = Guid.NewGuid();
        var fourthMoveId = Guid.NewGuid();

        var myPokemon = MyPokemon.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Level.Create(50),
            120,
            150,
            [firstMoveId, secondMoveId]);

        myPokemon.Reconfigure(
            Level.Create(55),
            140,
            170,
            [thirdMoveId, firstMoveId, fourthMoveId, secondMoveId]);

        Assert.Equal(55, myPokemon.Level.Value);
        Assert.Equal(140, myPokemon.CurrentHealthPoints);
        Assert.Equal(170, myPokemon.TotalHealthPoints);
        Assert.Equal([thirdMoveId, firstMoveId, fourthMoveId, secondMoveId], myPokemon.EquippedMoveIds);
        Assert.Equal([1, 2, 3, 4], myPokemon.EquippedMoves.Select(slot => slot.SlotNumber).OrderBy(slot => slot).ToArray());
    }

    [Fact]
    public void Reconfigure_Should_Not_Mutate_Existing_State_When_New_Move_Set_Is_Invalid()
    {
        var firstMoveId = Guid.NewGuid();
        var secondMoveId = Guid.NewGuid();

        var myPokemon = MyPokemon.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Level.Create(50),
            120,
            150,
            [firstMoveId, secondMoveId]);

        var action = () => myPokemon.Reconfigure(
            Level.Create(60),
            130,
            160,
            [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]);

        var exception = Assert.Throws<DomainException>(action);

        Assert.Contains("between 1 and 4", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(50, myPokemon.Level.Value);
        Assert.Equal(120, myPokemon.CurrentHealthPoints);
        Assert.Equal(150, myPokemon.TotalHealthPoints);
        Assert.Equal([firstMoveId, secondMoveId], myPokemon.EquippedMoveIds);
    }
}
