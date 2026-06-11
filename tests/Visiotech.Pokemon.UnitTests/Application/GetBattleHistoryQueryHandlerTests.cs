using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.Battles.Queries.GetBattleHistory;
using Visiotech.Pokemon.Domain.Battles;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class GetBattleHistoryQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Empty_History_When_Battle_Has_No_Phases()
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

        var handler = new GetBattleHistoryQueryHandler(repository);

        var result = await handler.Handle(new GetBattleHistoryQuery(battle.Id), CancellationToken.None);

        Assert.Equal(battle.Id, result.BattleId);
        Assert.Empty(result.Phases);
    }

    [Fact]
    public async Task Handle_Should_Return_Phases_Ordered_By_Sequence_With_Effectiveness_Snapshots()
    {
        var firstMyPokemonId = Guid.NewGuid();
        var secondMyPokemonId = Guid.NewGuid();
        var firstMoveId = Guid.NewGuid();
        var secondMoveId = Guid.NewGuid();
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
            firstMoveId,
            "Solar Beam",
            100,
            [
                new BattlePhaseEffectivenessInput(PokemonType.Fire, 0.5m),
                new BattlePhaseEffectivenessInput(PokemonType.Flying, 0.5m)
            ],
            0.25m,
            15,
            140,
            165,
            secondMyPokemonId,
            false));

        battle.RecordPhase(new BattlePhaseRegistration(
            2,
            secondMyPokemonId,
            firstMyPokemonId,
            secondMoveId,
            "Flamethrower",
            91,
            [
                new BattlePhaseEffectivenessInput(PokemonType.Grass, 2m),
                new BattlePhaseEffectivenessInput(PokemonType.Poison, 1m)
            ],
            2m,
            70,
            165,
            70,
            firstMyPokemonId,
            false));

        var repository = Substitute.For<IBattleReadRepository>();
        repository.GetByIdAsync(battle.Id, Arg.Any<CancellationToken>()).Returns(battle);

        var handler = new GetBattleHistoryQueryHandler(repository);

        var result = await handler.Handle(new GetBattleHistoryQuery(battle.Id), CancellationToken.None);

        Assert.Equal(battle.Id, result.BattleId);
        Assert.Collection(
            result.Phases,
            first =>
            {
                Assert.Equal(1, first.SequenceNumber);
                Assert.Equal(firstMyPokemonId, first.AttackerMyPokemonId);
                Assert.Equal(firstMoveId, first.MoveId);
                Assert.Equal("Solar Beam", first.MoveName);
                Assert.Equal(100, first.RandomFactor);
                Assert.Equal(0.25m, first.TotalEffectiveness);
                Assert.Equal(15, first.Damage);
                Assert.Equal(165, first.DefenderRemainingHealthPoints);
                Assert.Collection(
                    first.EffectivenessBreakdown,
                    item =>
                    {
                        Assert.Equal("Fire", item.DefenderType);
                        Assert.Equal(0.5m, item.Multiplier);
                    },
                    item =>
                    {
                        Assert.Equal("Flying", item.DefenderType);
                        Assert.Equal(0.5m, item.Multiplier);
                    });
            },
            second =>
            {
                Assert.Equal(2, second.SequenceNumber);
                Assert.Equal(secondMyPokemonId, second.AttackerMyPokemonId);
                Assert.Equal(secondMoveId, second.MoveId);
                Assert.Equal("Flamethrower", second.MoveName);
                Assert.Equal(91, second.RandomFactor);
                Assert.Equal(2m, second.TotalEffectiveness);
                Assert.Equal(70, second.Damage);
                Assert.Equal(70, second.DefenderRemainingHealthPoints);
                Assert.Collection(
                    second.EffectivenessBreakdown,
                    item =>
                    {
                        Assert.Equal("Grass", item.DefenderType);
                        Assert.Equal(2m, item.Multiplier);
                    },
                    item =>
                    {
                        Assert.Equal("Poison", item.DefenderType);
                        Assert.Equal(1m, item.Multiplier);
                    });
            });
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_Battle_Does_Not_Exist()
    {
        var repository = Substitute.For<IBattleReadRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Battle?)null);

        var handler = new GetBattleHistoryQueryHandler(repository);

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new GetBattleHistoryQuery(Guid.NewGuid()),
            CancellationToken.None));

        Assert.Equal("id", exception.Target);
    }
}
