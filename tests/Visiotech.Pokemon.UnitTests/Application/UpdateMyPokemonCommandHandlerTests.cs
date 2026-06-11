using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.MyPokemons.Commands.UpdateMyPokemon;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class UpdateMyPokemonCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Update_MyPokemon_When_Command_Is_Valid()
    {
        var species = CreateSpecies("Charizard", [PokemonType.Fire, PokemonType.Flying], 78, 84, 78, 109, 85, 100);
        var flamethrower = CreateMove("Flamethrower", PokemonType.Fire, MoveCategory.Special, 90);
        var fly = CreateMove("Fly", PokemonType.Flying, MoveCategory.Physical, 90);
        var airSlash = CreateMove("Air Slash", PokemonType.Flying, MoveCategory.Special, 75);
        var protect = CreateMove("Protect", PokemonType.Normal, MoveCategory.Status, 0);
        species.AddLearnableMove(flamethrower.Id);
        species.AddLearnableMove(fly.Id);
        species.AddLearnableMove(airSlash.Id);
        species.AddLearnableMove(protect.Id);

        var myPokemon = MyPokemon.Create(
            Guid.NewGuid(),
            species.Id,
            Level.Create(50),
            120,
            150,
            [flamethrower.Id, fly.Id]);

        var repository = Substitute.For<IMyPokemonWriteRepository>();
        repository.GetForUpdateAsync(myPokemon.Id, Arg.Any<CancellationToken>())
            .Returns(myPokemon);

        var speciesRepository = Substitute.For<IPokemonSpeciesReadRepository>();
        speciesRepository.GetByIdWithLearnableMovesAsync(species.Id, Arg.Any<CancellationToken>())
            .Returns(species);

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        moveRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids =>
                    ids.Count == 4 &&
                    ids.Contains(airSlash.Id) &&
                    ids.Contains(protect.Id) &&
                    ids.Contains(flamethrower.Id) &&
                    ids.Contains(fly.Id)),
                Arg.Any<CancellationToken>())
            .Returns([airSlash, protect, flamethrower, fly]);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateMyPokemonCommandHandler(repository, speciesRepository, moveRepository, unitOfWork);

        var result = await handler.Handle(
            new UpdateMyPokemonCommand(
                myPokemon.Id,
                55,
                140,
                170,
                [airSlash.Id, protect.Id, flamethrower.Id, fly.Id]),
            CancellationToken.None);

        Assert.Equal(myPokemon.Id, result.Id);
        Assert.Equal("Charizard", result.Species.Name);
        Assert.Equal(55, result.Level);
        Assert.Equal(140, result.CurrentHealthPoints);
        Assert.Equal(170, result.TotalHealthPoints);
        Assert.Collection(
            result.EquippedMoves,
            move => Assert.Equal("Air Slash", move.Name),
            move => Assert.Equal("Protect", move.Name),
            move => Assert.Equal("Flamethrower", move.Name),
            move => Assert.Equal("Fly", move.Name));
        Assert.Equal(55, myPokemon.Level.Value);
        Assert.Equal(140, myPokemon.CurrentHealthPoints);
        Assert.Equal(170, myPokemon.TotalHealthPoints);
        Assert.Equal([airSlash.Id, protect.Id, flamethrower.Id, fly.Id], myPokemon.EquippedMoveIds);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_MyPokemon_Does_Not_Exist()
    {
        var repository = Substitute.For<IMyPokemonWriteRepository>();
        repository.GetForUpdateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((MyPokemon?)null);

        var handler = new UpdateMyPokemonCommandHandler(
            repository,
            Substitute.For<IPokemonSpeciesReadRepository>(),
            Substitute.For<IPokemonMoveReadRepository>(),
            Substitute.For<IUnitOfWork>());

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new UpdateMyPokemonCommand(Guid.NewGuid(), 55, 120, 150, [Guid.NewGuid()]),
            CancellationToken.None));

        Assert.Equal("id", exception.Target);
    }

    [Fact]
    public async Task Handle_Should_Reject_Invalid_Level()
    {
        var handler = new UpdateMyPokemonCommandHandler(
            Substitute.For<IMyPokemonWriteRepository>(),
            Substitute.For<IPokemonSpeciesReadRepository>(),
            Substitute.For<IPokemonMoveReadRepository>(),
            Substitute.For<IUnitOfWork>());

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new UpdateMyPokemonCommand(Guid.NewGuid(), 0, 100, 120, [Guid.NewGuid()]),
            CancellationToken.None));

        Assert.Contains("level", exception.Errors.Keys);
    }

    [Fact]
    public async Task Handle_Should_Reject_Move_That_Is_Not_Learnable_By_Species()
    {
        var species = CreateSpecies("Blastoise", [PokemonType.Water], 79, 83, 100, 85, 105, 78);
        var surf = CreateMove("Surf", PokemonType.Water, MoveCategory.Special, 90);
        var thunderbolt = CreateMove("Thunderbolt", PokemonType.Electric, MoveCategory.Special, 90);
        species.AddLearnableMove(surf.Id);

        var myPokemon = MyPokemon.Create(
            Guid.NewGuid(),
            species.Id,
            Level.Create(45),
            110,
            140,
            [surf.Id]);

        var repository = Substitute.For<IMyPokemonWriteRepository>();
        repository.GetForUpdateAsync(myPokemon.Id, Arg.Any<CancellationToken>())
            .Returns(myPokemon);

        var speciesRepository = Substitute.For<IPokemonSpeciesReadRepository>();
        speciesRepository.GetByIdWithLearnableMovesAsync(species.Id, Arg.Any<CancellationToken>())
            .Returns(species);

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        moveRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([surf, thunderbolt]);

        var handler = new UpdateMyPokemonCommandHandler(
            repository,
            speciesRepository,
            moveRepository,
            Substitute.For<IUnitOfWork>());

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new UpdateMyPokemonCommand(myPokemon.Id, 48, 115, 140, [surf.Id, thunderbolt.Id]),
            CancellationToken.None));

        Assert.Contains("equippedMoveIds", exception.Errors.Keys);
    }

    [Fact]
    public async Task Handle_Should_Reject_More_Than_Four_Moves()
    {
        var handler = new UpdateMyPokemonCommandHandler(
            Substitute.For<IMyPokemonWriteRepository>(),
            Substitute.For<IPokemonSpeciesReadRepository>(),
            Substitute.For<IPokemonMoveReadRepository>(),
            Substitute.For<IUnitOfWork>());

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new UpdateMyPokemonCommand(
                Guid.NewGuid(),
                55,
                140,
                170,
                [Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()]),
            CancellationToken.None));

        Assert.Contains("equippedMoveIds", exception.Errors.Keys);
    }

    private static PokemonSpecies CreateSpecies(
        string name,
        IReadOnlyCollection<PokemonType> types,
        int health,
        int attack,
        int defense,
        int specialAttack,
        int specialDefense,
        int speed) =>
        PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create(name),
            PokemonTyping.Create(types),
            BaseStats.Create(health, attack, defense, specialAttack, specialDefense, speed));

    private static PokemonMove CreateMove(string name, PokemonType type, MoveCategory category, int power) =>
        PokemonMove.Create(Guid.NewGuid(), Move.Create(name, type, category, power));
}
