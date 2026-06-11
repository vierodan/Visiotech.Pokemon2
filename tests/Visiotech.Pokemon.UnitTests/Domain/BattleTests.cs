using Visiotech.Pokemon.Domain.Abstractions;
using Visiotech.Pokemon.Domain.Battles;

namespace Visiotech.Pokemon.UnitTests.Domain;

public sealed class BattleTests
{
    [Fact]
    public void Create_Should_Initialize_Battle_State_And_Combatants()
    {
        var firstMyPokemonId = Guid.NewGuid();
        var secondMyPokemonId = Guid.NewGuid();

        var battle = Battle.Create(
            Guid.NewGuid(),
            firstMyPokemonId,
            140,
            160,
            secondMyPokemonId,
            180,
            200);

        Assert.Equal(BattleStatus.Created, battle.Status);
        Assert.Equal(1, battle.CurrentTurnNumber);
        Assert.Equal(firstMyPokemonId, battle.NextAttackerMyPokemonId);
        Assert.Collection(
            battle.Combatants.OrderBy(combatant => combatant.SlotNumber),
            first =>
            {
                Assert.Equal(1, first.SlotNumber);
                Assert.Equal(firstMyPokemonId, first.MyPokemonId);
                Assert.Equal(140, first.CurrentHealthPoints);
                Assert.Equal(160, first.TotalHealthPoints);
            },
            second =>
            {
                Assert.Equal(2, second.SlotNumber);
                Assert.Equal(secondMyPokemonId, second.MyPokemonId);
                Assert.Equal(180, second.CurrentHealthPoints);
                Assert.Equal(200, second.TotalHealthPoints);
            });
    }

    [Fact]
    public void Create_Should_Reject_Repeated_Combatants()
    {
        var myPokemonId = Guid.NewGuid();

        var action = () => Battle.Create(
            Guid.NewGuid(),
            myPokemonId,
            120,
            150,
            myPokemonId,
            130,
            150);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Contains("distinct", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Create_Should_Reject_Starting_With_Zero_Current_Health_Points()
    {
        var action = () => Battle.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            0,
            150,
            Guid.NewGuid(),
            130,
            150);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Contains("greater than 0", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
