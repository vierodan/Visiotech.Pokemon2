using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Abstractions.Randomization;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.Damage.Queries.CalculateMoveDamage;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class CalculateMoveDamageQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_Defender_Does_Not_Exist()
    {
        var attacker = MyPokemon.Create(Guid.NewGuid(), Guid.NewGuid(), Level.Create(50), 120, 150, [Guid.NewGuid()]);

        var myPokemonRepository = Substitute.For<IMyPokemonReadRepository>();
        myPokemonRepository.GetByIdAsync(attacker.Id, Arg.Any<CancellationToken>()).Returns(attacker);
        myPokemonRepository.GetByIdAsync(Arg.Is<Guid>(id => id != attacker.Id), Arg.Any<CancellationToken>())
            .Returns((MyPokemon?)null);

        var handler = new CalculateMoveDamageQueryHandler(
            myPokemonRepository,
            Substitute.For<IPokemonSpeciesReadRepository>(),
            Substitute.For<IPokemonMoveReadRepository>(),
            CreateRandomProvider(100));

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new CalculateMoveDamageQuery(attacker.Id, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None));

        Assert.Equal("defenderMyPokemonId", exception.Target);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_Move_Does_Not_Exist()
    {
        var moveId = Guid.NewGuid();
        var attacker = MyPokemon.Create(Guid.NewGuid(), Guid.NewGuid(), Level.Create(50), 120, 150, [moveId]);
        var defender = MyPokemon.Create(Guid.NewGuid(), Guid.NewGuid(), Level.Create(50), 130, 150, [Guid.NewGuid()]);

        var myPokemonRepository = Substitute.For<IMyPokemonReadRepository>();
        myPokemonRepository.GetByIdAsync(attacker.Id, Arg.Any<CancellationToken>()).Returns(attacker);
        myPokemonRepository.GetByIdAsync(defender.Id, Arg.Any<CancellationToken>()).Returns(defender);

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        moveRepository.GetByIdAsync(moveId, Arg.Any<CancellationToken>())
            .Returns((PokemonMove?)null);

        var handler = new CalculateMoveDamageQueryHandler(
            myPokemonRepository,
            Substitute.For<IPokemonSpeciesReadRepository>(),
            moveRepository,
            CreateRandomProvider(100));

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new CalculateMoveDamageQuery(attacker.Id, defender.Id, moveId),
            CancellationToken.None));

        Assert.Equal("moveId", exception.Target);
    }

    [Fact]
    public async Task Handle_Should_Reject_Status_Moves()
    {
        var moveId = Guid.NewGuid();
        var speciesId = Guid.NewGuid();
        var attacker = MyPokemon.Create(Guid.NewGuid(), speciesId, Level.Create(50), 120, 150, [moveId]);
        var defender = MyPokemon.Create(Guid.NewGuid(), Guid.NewGuid(), Level.Create(50), 130, 150, [Guid.NewGuid()]);
        var protect = PokemonMove.Create(moveId, Move.Create("Protect", PokemonType.Normal, MoveCategory.Status, 0));

        var myPokemonRepository = Substitute.For<IMyPokemonReadRepository>();
        myPokemonRepository.GetByIdAsync(attacker.Id, Arg.Any<CancellationToken>()).Returns(attacker);
        myPokemonRepository.GetByIdAsync(defender.Id, Arg.Any<CancellationToken>()).Returns(defender);

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        moveRepository.GetByIdAsync(moveId, Arg.Any<CancellationToken>()).Returns(protect);

        var handler = new CalculateMoveDamageQueryHandler(
            myPokemonRepository,
            Substitute.For<IPokemonSpeciesReadRepository>(),
            moveRepository,
            CreateRandomProvider(100));

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new CalculateMoveDamageQuery(attacker.Id, defender.Id, moveId),
            CancellationToken.None));

        Assert.Contains("moveId", exception.Errors.Keys);
    }

    private static IDamageRandomProvider CreateRandomProvider(int value)
    {
        var provider = Substitute.For<IDamageRandomProvider>();
        provider.Next().Returns(value);
        return provider;
    }
}
