using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.Battles.Queries.GetBattleState;
using Visiotech.Pokemon.Domain.Battles;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class GetBattleStateQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Created_Battle_State()
    {
        var battle = Battle.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            140,
            160,
            Guid.NewGuid(),
            180,
            200);

        var repository = Substitute.For<IBattleReadRepository>();
        repository.GetByIdAsync(battle.Id, Arg.Any<CancellationToken>()).Returns(battle);

        var handler = new GetBattleStateQueryHandler(repository);

        var result = await handler.Handle(new GetBattleStateQuery(battle.Id), CancellationToken.None);

        Assert.Equal("Created", result.Status);
        Assert.Equal(1, result.CurrentTurnNumber);
        Assert.NotNull(result.NextAttackerMyPokemonId);
        Assert.Empty(result.History);
        Assert.Equal(2, result.Combatants.Count);
    }

    [Fact]
    public async Task Handle_Should_Return_InProgress_Battle_State_With_History()
    {
        var firstMyPokemonId = Guid.NewGuid();
        var secondMyPokemonId = Guid.NewGuid();
        var moveId = Guid.NewGuid();
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
            moveId,
            "Close Combat",
            91,
            [new BattlePhaseEffectivenessInput(PokemonType.Normal, 2m)],
            2m,
            80,
            140,
            100,
            secondMyPokemonId,
            false));

        var repository = Substitute.For<IBattleReadRepository>();
        repository.GetByIdAsync(battle.Id, Arg.Any<CancellationToken>()).Returns(battle);

        var handler = new GetBattleStateQueryHandler(repository);

        var result = await handler.Handle(new GetBattleStateQuery(battle.Id), CancellationToken.None);

        Assert.Equal("InProgress", result.Status);
        Assert.Equal(2, result.CurrentTurnNumber);
        Assert.Equal(secondMyPokemonId, result.NextAttackerMyPokemonId);
        var phase = Assert.Single(result.History);
        Assert.Equal(1, phase.SequenceNumber);
        Assert.Equal(moveId, phase.MoveId);
        Assert.Equal("Close Combat", phase.MoveName);
        Assert.Equal(91, phase.RandomFactor);
        Assert.Equal(2m, phase.TotalEffectiveness);
    }

    [Fact]
    public async Task Handle_Should_Return_Finished_Battle_State_Without_Next_Attacker()
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

        var repository = Substitute.For<IBattleReadRepository>();
        repository.GetByIdAsync(battle.Id, Arg.Any<CancellationToken>()).Returns(battle);

        var handler = new GetBattleStateQueryHandler(repository);

        var result = await handler.Handle(new GetBattleStateQuery(battle.Id), CancellationToken.None);

        Assert.Equal("Finished", result.Status);
        Assert.Null(result.NextAttackerMyPokemonId);
        Assert.Single(result.History);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_Battle_Does_Not_Exist()
    {
        var repository = Substitute.For<IBattleReadRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Battle?)null);

        var handler = new GetBattleStateQueryHandler(repository);

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new GetBattleStateQuery(Guid.NewGuid()),
            CancellationToken.None));

        Assert.Equal("id", exception.Target);
    }
}
