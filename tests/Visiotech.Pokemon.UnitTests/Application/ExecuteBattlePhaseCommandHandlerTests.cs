using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Abstractions.Services;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Battles.Commands.ExecuteBattlePhase;
using Visiotech.Pokemon.Application.Features.Damage;
using Visiotech.Pokemon.Domain.Battles;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class ExecuteBattlePhaseCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Execute_Battle_Phase_And_Persist_Updated_State()
    {
        var moveId = Guid.NewGuid();
        var attacker = MyPokemon.Create(Guid.NewGuid(), Guid.NewGuid(), Level.Create(50), 150, 160, [moveId]);
        var defender = MyPokemon.Create(Guid.NewGuid(), Guid.NewGuid(), Level.Create(55), 190, 200, [Guid.NewGuid()]);
        var battle = Battle.Create(
            Guid.NewGuid(),
            attacker.Id,
            140,
            160,
            defender.Id,
            180,
            200);

        var battleRepository = Substitute.For<IBattleWriteRepository>();
        battleRepository.GetForUpdateAsync(battle.Id, Arg.Any<CancellationToken>()).Returns(battle);

        var myPokemonRepository = Substitute.For<IMyPokemonWriteRepository>();
        myPokemonRepository.GetForUpdateAsync(attacker.Id, Arg.Any<CancellationToken>()).Returns(attacker);
        myPokemonRepository.GetForUpdateAsync(defender.Id, Arg.Any<CancellationToken>()).Returns(defender);

        var damageService = Substitute.For<IMoveDamageCalculationService>();
        damageService.CalculateAsync(
                Arg.Is<MoveDamageCalculationRequest>(request =>
                    request.AttackerMyPokemonId == attacker.Id &&
                    request.DefenderMyPokemonId == defender.Id &&
                    request.MoveId == moveId &&
                    request.AttackerCurrentHealthPointsOverride == 140 &&
                    request.DefenderCurrentHealthPointsOverride == 180),
                Arg.Any<CancellationToken>())
            .Returns(new MoveDamageCalculationResponse(
                attacker.Id,
                defender.Id,
                moveId,
                "Close Combat",
                "Fighting",
                "Physical",
                attacker.Level.Value,
                120,
                "Attack",
                130,
                "Defense",
                65,
                180,
                100,
                52m,
                [new MoveDamageCalculationEffectivenessResponse("Normal", 2m)],
                2m,
                80,
                80,
                100));

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ExecuteBattlePhaseCommandHandler(
            battleRepository,
            myPokemonRepository,
            damageService,
            unitOfWork);

        var result = await handler.Handle(
            new ExecuteBattlePhaseCommand(battle.Id, attacker.Id, moveId),
            CancellationToken.None);

        Assert.Equal("InProgress", result.Battle.Status);
        Assert.Equal(2, result.Battle.CurrentTurnNumber);
        Assert.Equal(defender.Id, result.Battle.NextAttackerMyPokemonId);
        Assert.Null(result.Battle.WinnerMyPokemonId);
        Assert.Null(result.Battle.LoserMyPokemonId);
        Assert.Equal(140, attacker.CurrentHealthPoints);
        Assert.Equal(100, defender.CurrentHealthPoints);
        var phase = Assert.Single(result.Battle.History);
        Assert.Equal(moveId, phase.MoveId);
        Assert.Equal(80, phase.Damage);
        Assert.Equal(2m, phase.TotalEffectiveness);
        Assert.Equal(100, phase.DefenderRemainingHealthPoints);
        Assert.Equal(80, result.DamageCalculation.Damage);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Move_That_Is_Not_Equipped()
    {
        var moveId = Guid.NewGuid();
        var attacker = MyPokemon.Create(Guid.NewGuid(), Guid.NewGuid(), Level.Create(50), 140, 160, [Guid.NewGuid()]);
        var defender = MyPokemon.Create(Guid.NewGuid(), Guid.NewGuid(), Level.Create(55), 180, 200, [Guid.NewGuid()]);
        var battle = Battle.Create(
            Guid.NewGuid(),
            attacker.Id,
            140,
            160,
            defender.Id,
            180,
            200);

        var battleRepository = Substitute.For<IBattleWriteRepository>();
        battleRepository.GetForUpdateAsync(battle.Id, Arg.Any<CancellationToken>()).Returns(battle);

        var myPokemonRepository = Substitute.For<IMyPokemonWriteRepository>();
        myPokemonRepository.GetForUpdateAsync(attacker.Id, Arg.Any<CancellationToken>()).Returns(attacker);
        myPokemonRepository.GetForUpdateAsync(defender.Id, Arg.Any<CancellationToken>()).Returns(defender);

        var damageService = Substitute.For<IMoveDamageCalculationService>();
        damageService.CalculateAsync(Arg.Any<MoveDamageCalculationRequest>(), Arg.Any<CancellationToken>())
            .Returns<Task<MoveDamageCalculationResponse>>(_ => throw new ApplicationValidationException(new Dictionary<string, string[]>
            {
                ["moveId"] = [$"Pokemon move '{moveId}' is not equipped by my pokemon '{attacker.Id}'."]
            }));

        var handler = new ExecuteBattlePhaseCommandHandler(
            battleRepository,
            myPokemonRepository,
            damageService,
            Substitute.For<IUnitOfWork>());

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new ExecuteBattlePhaseCommand(battle.Id, attacker.Id, moveId),
            CancellationToken.None));

        Assert.Contains("moveId", exception.Errors.Keys);
    }

    [Fact]
    public async Task Handle_Should_Reject_Attacker_That_Does_Not_Belong_To_The_Battle()
    {
        var battle = Battle.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            140,
            160,
            Guid.NewGuid(),
            180,
            200);

        var battleRepository = Substitute.For<IBattleWriteRepository>();
        battleRepository.GetForUpdateAsync(battle.Id, Arg.Any<CancellationToken>()).Returns(battle);

        var damageService = Substitute.For<IMoveDamageCalculationService>();
        var handler = new ExecuteBattlePhaseCommandHandler(
            battleRepository,
            Substitute.For<IMyPokemonWriteRepository>(),
            damageService,
            Substitute.For<IUnitOfWork>());

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new ExecuteBattlePhaseCommand(battle.Id, Guid.NewGuid(), Guid.NewGuid()),
            CancellationToken.None));

        Assert.Contains("attackerMyPokemonId", exception.Errors.Keys);
        await damageService.DidNotReceive().CalculateAsync(Arg.Any<MoveDamageCalculationRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Finished_Battle()
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

        battle.RecordPhase(new BattlePhaseRegistration(
            1,
            firstMyPokemonId,
            secondMyPokemonId,
            Guid.NewGuid(),
            "Close Combat",
            100,
            [new BattlePhaseEffectivenessInput(PokemonType.Normal, 2m)],
            2m,
            180,
            140,
            0,
            null,
            true));

        var battleRepository = Substitute.For<IBattleWriteRepository>();
        battleRepository.GetForUpdateAsync(battle.Id, Arg.Any<CancellationToken>()).Returns(battle);

        var handler = new ExecuteBattlePhaseCommandHandler(
            battleRepository,
            Substitute.For<IMyPokemonWriteRepository>(),
            Substitute.For<IMoveDamageCalculationService>(),
            Substitute.For<IUnitOfWork>());

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new ExecuteBattlePhaseCommand(battle.Id, firstMyPokemonId, Guid.NewGuid()),
            CancellationToken.None));

        Assert.Contains("id", exception.Errors.Keys);
    }

    [Fact]
    public async Task Handle_Should_Finish_Battle_When_Defender_Reaches_Zero_Health()
    {
        var moveId = Guid.NewGuid();
        var attacker = MyPokemon.Create(Guid.NewGuid(), Guid.NewGuid(), Level.Create(50), 140, 160, [moveId]);
        var defender = MyPokemon.Create(Guid.NewGuid(), Guid.NewGuid(), Level.Create(55), 180, 200, [Guid.NewGuid()]);
        var battle = Battle.Create(
            Guid.NewGuid(),
            attacker.Id,
            140,
            160,
            defender.Id,
            180,
            200);

        var battleRepository = Substitute.For<IBattleWriteRepository>();
        battleRepository.GetForUpdateAsync(battle.Id, Arg.Any<CancellationToken>()).Returns(battle);

        var myPokemonRepository = Substitute.For<IMyPokemonWriteRepository>();
        myPokemonRepository.GetForUpdateAsync(attacker.Id, Arg.Any<CancellationToken>()).Returns(attacker);
        myPokemonRepository.GetForUpdateAsync(defender.Id, Arg.Any<CancellationToken>()).Returns(defender);

        var damageService = Substitute.For<IMoveDamageCalculationService>();
        damageService.CalculateAsync(Arg.Any<MoveDamageCalculationRequest>(), Arg.Any<CancellationToken>())
            .Returns(new MoveDamageCalculationResponse(
                attacker.Id,
                defender.Id,
                moveId,
                "Close Combat",
                "Fighting",
                "Physical",
                attacker.Level.Value,
                120,
                "Attack",
                130,
                "Defense",
                65,
                180,
                100,
                52m,
                [new MoveDamageCalculationEffectivenessResponse("Normal", 2m)],
                2m,
                180,
                180,
                0));

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ExecuteBattlePhaseCommandHandler(
            battleRepository,
            myPokemonRepository,
            damageService,
            unitOfWork);

        var result = await handler.Handle(
            new ExecuteBattlePhaseCommand(battle.Id, attacker.Id, moveId),
            CancellationToken.None);

        Assert.Equal("Finished", result.Battle.Status);
        Assert.Equal(1, result.Battle.CurrentTurnNumber);
        Assert.Null(result.Battle.NextAttackerMyPokemonId);
        Assert.Equal(attacker.Id, result.Battle.WinnerMyPokemonId);
        Assert.Equal(defender.Id, result.Battle.LoserMyPokemonId);
        Assert.Equal(0, defender.CurrentHealthPoints);
        Assert.Equal(0, result.Battle.Combatants.Single(item => item.MyPokemonId == defender.Id).CurrentHealthPoints);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
