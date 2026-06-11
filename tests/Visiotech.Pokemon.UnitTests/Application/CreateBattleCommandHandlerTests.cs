using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.Battles.Commands.CreateBattle;
using Visiotech.Pokemon.Domain.Battles;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class CreateBattleCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Battle_When_Command_Is_Valid()
    {
        var firstMyPokemon = MyPokemon.Create(Guid.NewGuid(), Guid.NewGuid(), Level.Create(50), 140, 160, [Guid.NewGuid()]);
        var secondMyPokemon = MyPokemon.Create(Guid.NewGuid(), Guid.NewGuid(), Level.Create(55), 180, 200, [Guid.NewGuid()]);

        var repository = Substitute.For<IBattleWriteRepository>();
        var myPokemonRepository = Substitute.For<IMyPokemonReadRepository>();
        myPokemonRepository.GetByIdAsync(firstMyPokemon.Id, Arg.Any<CancellationToken>()).Returns(firstMyPokemon);
        myPokemonRepository.GetByIdAsync(secondMyPokemon.Id, Arg.Any<CancellationToken>()).Returns(secondMyPokemon);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateBattleCommandHandler(repository, myPokemonRepository, unitOfWork);

        var result = await handler.Handle(
            new CreateBattleCommand(firstMyPokemon.Id, secondMyPokemon.Id),
            CancellationToken.None);

        Assert.Equal("Created", result.Status);
        Assert.Equal(1, result.CurrentTurnNumber);
        Assert.Equal(firstMyPokemon.Id, result.NextAttackerMyPokemonId);
        Assert.Collection(
            result.Combatants,
            first => Assert.Equal(firstMyPokemon.Id, first.MyPokemonId),
            second => Assert.Equal(secondMyPokemon.Id, second.MyPokemonId));

        await repository.Received(1).AddAsync(
            Arg.Is<Battle>(battle =>
                battle.Status == BattleStatus.Created &&
                battle.CurrentTurnNumber == 1 &&
                battle.NextAttackerMyPokemonId == firstMyPokemon.Id &&
                battle.Combatants.Count == 2),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_First_MyPokemon_Does_Not_Exist()
    {
        var myPokemonRepository = Substitute.For<IMyPokemonReadRepository>();
        myPokemonRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((MyPokemon?)null);

        var handler = new CreateBattleCommandHandler(
            Substitute.For<IBattleWriteRepository>(),
            myPokemonRepository,
            Substitute.For<IUnitOfWork>());

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new CreateBattleCommand(Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None));

        Assert.Equal("firstMyPokemonId", exception.Target);
    }

    [Fact]
    public async Task Handle_Should_Reject_Repeated_MyPokemon_Ids()
    {
        var myPokemonId = Guid.NewGuid();

        var handler = new CreateBattleCommandHandler(
            Substitute.For<IBattleWriteRepository>(),
            Substitute.For<IMyPokemonReadRepository>(),
            Substitute.For<IUnitOfWork>());

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new CreateBattleCommand(myPokemonId, myPokemonId),
            CancellationToken.None));

        Assert.Contains("secondMyPokemonId", exception.Errors.Keys);
    }

    [Fact]
    public async Task Handle_Should_Reject_Combatants_With_Zero_Current_Health_Points()
    {
        var firstMyPokemon = MyPokemon.Create(Guid.NewGuid(), Guid.NewGuid(), Level.Create(50), 0, 160, [Guid.NewGuid()]);
        var secondMyPokemon = MyPokemon.Create(Guid.NewGuid(), Guid.NewGuid(), Level.Create(55), 180, 200, [Guid.NewGuid()]);

        var myPokemonRepository = Substitute.For<IMyPokemonReadRepository>();
        myPokemonRepository.GetByIdAsync(firstMyPokemon.Id, Arg.Any<CancellationToken>()).Returns(firstMyPokemon);
        myPokemonRepository.GetByIdAsync(secondMyPokemon.Id, Arg.Any<CancellationToken>()).Returns(secondMyPokemon);

        var handler = new CreateBattleCommandHandler(
            Substitute.For<IBattleWriteRepository>(),
            myPokemonRepository,
            Substitute.For<IUnitOfWork>());

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new CreateBattleCommand(firstMyPokemon.Id, secondMyPokemon.Id),
            CancellationToken.None));

        Assert.Contains("firstMyPokemonId", exception.Errors.Keys);
    }
}
