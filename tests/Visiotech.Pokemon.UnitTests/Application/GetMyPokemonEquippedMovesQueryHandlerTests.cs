using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.MyPokemons.Queries.GetMyPokemonEquippedMoves;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class GetMyPokemonEquippedMovesQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Equipped_Moves_For_Existing_MyPokemon()
    {
        var flamethrower = CreateMove("Flamethrower", PokemonType.Fire, MoveCategory.Special, 90);
        var fly = CreateMove("Fly", PokemonType.Flying, MoveCategory.Physical, 90);
        var myPokemon = MyPokemon.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Level.Create(50),
            120,
            150,
            [flamethrower.Id, fly.Id]);

        var repository = Substitute.For<IMyPokemonReadRepository>();
        repository.GetByIdAsync(myPokemon.Id, Arg.Any<CancellationToken>()).Returns(myPokemon);

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        moveRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 2 && ids.Contains(flamethrower.Id) && ids.Contains(fly.Id)),
                Arg.Any<CancellationToken>())
            .Returns([flamethrower, fly]);

        var handler = new GetMyPokemonEquippedMovesQueryHandler(repository, moveRepository);

        var result = await handler.Handle(new GetMyPokemonEquippedMovesQuery(myPokemon.Id), CancellationToken.None);

        Assert.Equal(myPokemon.Id, result.MyPokemonId);
        Assert.Collection(
            result.Moves,
            move => Assert.Equal("Flamethrower", move.Name),
            move => Assert.Equal("Fly", move.Name));
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_MyPokemon_Does_Not_Exist()
    {
        var repository = Substitute.For<IMyPokemonReadRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((MyPokemon?)null);

        var handler = new GetMyPokemonEquippedMovesQueryHandler(
            repository,
            Substitute.For<IPokemonMoveReadRepository>());

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new GetMyPokemonEquippedMovesQuery(Guid.NewGuid()),
            CancellationToken.None));

        Assert.Equal("id", exception.Target);
    }

    [Fact]
    public async Task Handle_Should_Return_Up_To_Four_Equipped_Moves_In_Slot_Order()
    {
        var hyperBeam = CreateMove("Hyper Beam", PokemonType.Normal, MoveCategory.Special, 150);
        var earthquake = CreateMove("Earthquake", PokemonType.Ground, MoveCategory.Physical, 100);
        var airSlash = CreateMove("Air Slash", PokemonType.Flying, MoveCategory.Special, 75);
        var thunderPunch = CreateMove("Thunder Punch", PokemonType.Electric, MoveCategory.Physical, 75);
        var myPokemon = MyPokemon.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Level.Create(55),
            140,
            160,
            [hyperBeam.Id, earthquake.Id, airSlash.Id, thunderPunch.Id]);

        var repository = Substitute.For<IMyPokemonReadRepository>();
        repository.GetByIdAsync(myPokemon.Id, Arg.Any<CancellationToken>()).Returns(myPokemon);

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        moveRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([thunderPunch, airSlash, hyperBeam, earthquake]);

        var handler = new GetMyPokemonEquippedMovesQueryHandler(repository, moveRepository);

        var result = await handler.Handle(new GetMyPokemonEquippedMovesQuery(myPokemon.Id), CancellationToken.None);

        Assert.Equal(4, result.Moves.Count);
        Assert.Collection(
            result.Moves,
            move => Assert.Equal("Hyper Beam", move.Name),
            move => Assert.Equal("Earthquake", move.Name),
            move => Assert.Equal("Air Slash", move.Name),
            move => Assert.Equal("Thunder Punch", move.Name));
    }

    private static PokemonMove CreateMove(string name, PokemonType type, MoveCategory category, int power) =>
        PokemonMove.Create(Guid.NewGuid(), Move.Create(name, type, category, power));
}
