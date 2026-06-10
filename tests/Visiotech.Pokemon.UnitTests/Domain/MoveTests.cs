using Visiotech.Pokemon.Domain.Abstractions;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Domain;

public sealed class MoveTests
{
    [Fact]
    public void Create_Should_Allow_Status_Move_With_Zero_Power()
    {
        var move = Move.Create("Protect", PokemonType.Normal, MoveCategory.Status, 0);

        Assert.Equal("Protect", move.Name.Value);
        Assert.Equal(MoveCategory.Status, move.Category);
        Assert.Equal(0, move.Power);
    }

    [Fact]
    public void Create_Should_Reject_Non_Status_Move_With_Zero_Power()
    {
        var action = () => Move.Create("Thunderbolt", PokemonType.Electric, MoveCategory.Special, 0);

        var exception = Assert.Throws<DomainException>(action);
        Assert.Contains("greater than 0", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
