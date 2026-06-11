using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Abstractions.Services;
using Visiotech.Pokemon.Application.Abstractions.Randomization;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Damage;
using Visiotech.Pokemon.Application.Features.Damage.Queries.CalculateMoveDamage;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class MoveDamageCalculationServiceTests
{
    [Fact]
    public async Task CalculateAsync_Should_Throw_NotFound_When_Defender_Does_Not_Exist()
    {
        var attacker = MyPokemon.Create(Guid.NewGuid(), Guid.NewGuid(), Level.Create(50), 120, 150, [Guid.NewGuid()]);

        var myPokemonRepository = Substitute.For<IMyPokemonReadRepository>();
        myPokemonRepository.GetByIdAsync(attacker.Id, Arg.Any<CancellationToken>()).Returns(attacker);
        myPokemonRepository.GetByIdAsync(Arg.Is<Guid>(id => id != attacker.Id), Arg.Any<CancellationToken>())
            .Returns((MyPokemon?)null);

        var service = new MoveDamageCalculationService(
            myPokemonRepository,
            Substitute.For<IPokemonSpeciesReadRepository>(),
            Substitute.For<IPokemonMoveReadRepository>(),
            CreateRandomProvider(100));

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => service.CalculateAsync(
            new MoveDamageCalculationRequest(attacker.Id, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None));

        Assert.Equal("defenderMyPokemonId", exception.Target);
    }

    [Fact]
    public async Task CalculateAsync_Should_Throw_NotFound_When_Move_Does_Not_Exist()
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

        var service = new MoveDamageCalculationService(
            myPokemonRepository,
            Substitute.For<IPokemonSpeciesReadRepository>(),
            moveRepository,
            CreateRandomProvider(100));

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => service.CalculateAsync(
            new MoveDamageCalculationRequest(attacker.Id, defender.Id, moveId),
            CancellationToken.None));

        Assert.Equal("moveId", exception.Target);
    }

    [Fact]
    public async Task CalculateAsync_Should_Reject_Status_Moves()
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

        var service = new MoveDamageCalculationService(
            myPokemonRepository,
            Substitute.For<IPokemonSpeciesReadRepository>(),
            moveRepository,
            CreateRandomProvider(100));

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => service.CalculateAsync(
            new MoveDamageCalculationRequest(attacker.Id, defender.Id, moveId),
            CancellationToken.None));

        Assert.Contains("moveId", exception.Errors.Keys);
    }

    [Fact]
    public async Task Handle_Should_Delegate_To_Move_Damage_Calculation_Service()
    {
        var expected = new MoveDamageCalculationResponse(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Thunderbolt",
            "Electric",
            "Special",
            50,
            90,
            "SpecialAttack",
            50,
            "SpecialDefense",
            105,
            140,
            100,
            18.5m,
            [new MoveDamageCalculationEffectivenessResponse("Water", 2m)],
            2m,
            37,
            37,
            103);

        var service = Substitute.For<IMoveDamageCalculationService>();
        service.CalculateAsync(
                Arg.Is<MoveDamageCalculationRequest>(request =>
                    request.AttackerMyPokemonId == expected.AttackerMyPokemonId &&
                    request.DefenderMyPokemonId == expected.DefenderMyPokemonId &&
                    request.MoveId == expected.MoveId &&
                    request.AttackerCurrentHealthPointsOverride == null &&
                    request.DefenderCurrentHealthPointsOverride == null),
                Arg.Any<CancellationToken>())
            .Returns(expected);

        var handler = new CalculateMoveDamageQueryHandler(service);

        var result = await handler.Handle(
            new CalculateMoveDamageQuery(
                expected.AttackerMyPokemonId,
                expected.DefenderMyPokemonId,
                expected.MoveId),
            CancellationToken.None);

        Assert.Same(expected, result);
    }

    private static IDamageRandomProvider CreateRandomProvider(int value)
    {
        var provider = Substitute.For<IDamageRandomProvider>();
        provider.Next().Returns(value);
        return provider;
    }
}
