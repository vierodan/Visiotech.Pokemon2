using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.MyPokemons.Commands.CreateMyPokemon;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class CreateMyPokemonCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_MyPokemon_When_Command_Is_Valid()
    {
        var species = PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create("Charizard"),
            PokemonTyping.Create([PokemonType.Fire, PokemonType.Flying]),
            BaseStats.Create(78, 84, 78, 109, 85, 100));
        var flamethrower = PokemonMove.Create(Guid.NewGuid(), Move.Create("Flamethrower", PokemonType.Fire, MoveCategory.Special, 90));
        var fly = PokemonMove.Create(Guid.NewGuid(), Move.Create("Fly", PokemonType.Flying, MoveCategory.Physical, 90));
        species.AddLearnableMove(flamethrower.Id);
        species.AddLearnableMove(fly.Id);

        var repository = Substitute.For<IMyPokemonWriteRepository>();
        var speciesRepository = Substitute.For<IPokemonSpeciesReadRepository>();
        speciesRepository.GetByIdWithLearnableMovesAsync(species.Id, Arg.Any<CancellationToken>())
            .Returns(species);

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        moveRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 2 && ids.Contains(flamethrower.Id) && ids.Contains(fly.Id)),
                Arg.Any<CancellationToken>())
            .Returns([flamethrower, fly]);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateMyPokemonCommandHandler(repository, speciesRepository, moveRepository, unitOfWork);

        var result = await handler.Handle(
            new CreateMyPokemonCommand(species.Id, 50, 120, 150, [flamethrower.Id, fly.Id]),
            CancellationToken.None);

        Assert.Equal("Charizard", result.Species.Name);
        Assert.Equal(50, result.Level);
        Assert.Equal(2, result.EquippedMoves.Count);
        Assert.Collection(
            result.EquippedMoves,
            move => Assert.Equal("Flamethrower", move.Name),
            move => Assert.Equal("Fly", move.Name));
        await repository.Received(1).AddAsync(
            Arg.Is<MyPokemon>(myPokemon =>
                myPokemon.PokemonSpeciesId == species.Id &&
                myPokemon.Level.Value == 50 &&
                myPokemon.EquippedMoveIds.SequenceEqual(new[] { flamethrower.Id, fly.Id })),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_Species_Does_Not_Exist()
    {
        var repository = Substitute.For<IMyPokemonWriteRepository>();
        var speciesRepository = Substitute.For<IPokemonSpeciesReadRepository>();
        speciesRepository.GetByIdWithLearnableMovesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((PokemonSpecies?)null);

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateMyPokemonCommandHandler(repository, speciesRepository, moveRepository, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new CreateMyPokemonCommand(Guid.NewGuid(), 50, 100, 120, [Guid.NewGuid()]),
            CancellationToken.None));

        Assert.Equal("pokemonSpeciesId", exception.Target);
        await repository.DidNotReceive().AddAsync(Arg.Any<MyPokemon>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Invalid_Level()
    {
        var handler = new CreateMyPokemonCommandHandler(
            Substitute.For<IMyPokemonWriteRepository>(),
            Substitute.For<IPokemonSpeciesReadRepository>(),
            Substitute.For<IPokemonMoveReadRepository>(),
            Substitute.For<IUnitOfWork>());

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new CreateMyPokemonCommand(Guid.NewGuid(), 0, 100, 120, [Guid.NewGuid()]),
            CancellationToken.None));

        Assert.Contains("level", exception.Errors.Keys);
    }

    [Fact]
    public async Task Handle_Should_Reject_Inconsistent_Health_Points()
    {
        var handler = new CreateMyPokemonCommandHandler(
            Substitute.For<IMyPokemonWriteRepository>(),
            Substitute.For<IPokemonSpeciesReadRepository>(),
            Substitute.For<IPokemonMoveReadRepository>(),
            Substitute.For<IUnitOfWork>());

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new CreateMyPokemonCommand(Guid.NewGuid(), 25, 150, 100, [Guid.NewGuid()]),
            CancellationToken.None));

        Assert.Contains("currentHealthPoints", exception.Errors.Keys);
    }

    [Fact]
    public async Task Handle_Should_Reject_Move_That_Is_Not_Learnable_By_Species()
    {
        var species = PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create("Blastoise"),
            PokemonTyping.Create([PokemonType.Water]),
            BaseStats.Create(79, 83, 100, 85, 105, 78));
        var surf = PokemonMove.Create(Guid.NewGuid(), Move.Create("Surf", PokemonType.Water, MoveCategory.Special, 90));
        var thunderbolt = PokemonMove.Create(Guid.NewGuid(), Move.Create("Thunderbolt", PokemonType.Electric, MoveCategory.Special, 90));
        species.AddLearnableMove(surf.Id);

        var repository = Substitute.For<IMyPokemonWriteRepository>();
        var speciesRepository = Substitute.For<IPokemonSpeciesReadRepository>();
        speciesRepository.GetByIdWithLearnableMovesAsync(species.Id, Arg.Any<CancellationToken>())
            .Returns(species);

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        moveRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([surf, thunderbolt]);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateMyPokemonCommandHandler(repository, speciesRepository, moveRepository, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new CreateMyPokemonCommand(species.Id, 40, 110, 120, [surf.Id, thunderbolt.Id]),
            CancellationToken.None));

        Assert.Contains("equippedMoveIds", exception.Errors.Keys);
        await repository.DidNotReceive().AddAsync(Arg.Any<MyPokemon>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Duplicate_Moves()
    {
        var handler = new CreateMyPokemonCommandHandler(
            Substitute.For<IMyPokemonWriteRepository>(),
            Substitute.For<IPokemonSpeciesReadRepository>(),
            Substitute.For<IPokemonMoveReadRepository>(),
            Substitute.For<IUnitOfWork>());

        var moveId = Guid.NewGuid();
        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new CreateMyPokemonCommand(Guid.NewGuid(), 25, 80, 100, [moveId, moveId]),
            CancellationToken.None));

        Assert.Contains("equippedMoveIds", exception.Errors.Keys);
    }

    [Fact]
    public async Task Handle_Should_Reject_More_Than_Four_Moves()
    {
        var handler = new CreateMyPokemonCommandHandler(
            Substitute.For<IMyPokemonWriteRepository>(),
            Substitute.For<IPokemonSpeciesReadRepository>(),
            Substitute.For<IPokemonMoveReadRepository>(),
            Substitute.For<IUnitOfWork>());

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new CreateMyPokemonCommand(Guid.NewGuid(), 25, 80, 100, [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]),
            CancellationToken.None));

        Assert.Contains("equippedMoveIds", exception.Errors.Keys);
    }
}
