using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.UpdatePokemonSpeciesLearnableMoves;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class UpdatePokemonSpeciesLearnableMovesCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Add_And_Remove_Learnable_Moves_When_Request_Is_Valid()
    {
        var retainedMoveId = Guid.NewGuid();
        var removedMoveId = Guid.NewGuid();
        var addedMoveId = Guid.NewGuid();

        var species = PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create("Blastoise"),
            PokemonTyping.Create([PokemonType.Water]),
            BaseStats.Create(79, 83, 100, 85, 105, 78));
        species.AddLearnableMove(retainedMoveId);
        species.AddLearnableMove(removedMoveId);

        var speciesRepository = Substitute.For<IPokemonSpeciesWriteRepository>();
        speciesRepository.GetForUpdateWithLearnableMovesAsync(species.Id, Arg.Any<CancellationToken>())
            .Returns(species);

        var retainedMove = PokemonMove.Create(retainedMoveId, Move.Create("Surf", PokemonType.Water, MoveCategory.Special, 90));
        var removedMove = PokemonMove.Create(removedMoveId, Move.Create("Ice Beam", PokemonType.Ice, MoveCategory.Special, 90));
        var addedMove = PokemonMove.Create(addedMoveId, Move.Create("Protect", PokemonType.Normal, MoveCategory.Status, 0));

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        moveRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 2 && ids.Contains(removedMoveId) && ids.Contains(addedMoveId)),
                Arg.Any<CancellationToken>())
            .Returns([removedMove, addedMove]);
        moveRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 2 && ids.Contains(retainedMoveId) && ids.Contains(addedMoveId)),
                Arg.Any<CancellationToken>())
            .Returns([retainedMove, addedMove]);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdatePokemonSpeciesLearnableMovesCommandHandler(speciesRepository, moveRepository, unitOfWork);

        var result = await handler.Handle(
            new UpdatePokemonSpeciesLearnableMovesCommand(species.Id, [addedMoveId], [removedMoveId]),
            CancellationToken.None);

        Assert.Equal(species.Id, result.PokemonSpeciesId);
        Assert.Equal("Blastoise", result.PokemonSpeciesName);
        Assert.Equal(2, result.Moves.Count);
        Assert.Contains(result.Moves, move => move.Id == retainedMoveId);
        Assert.Contains(result.Moves, move => move.Id == addedMoveId);
        Assert.DoesNotContain(species.LearnableMoveIds, moveId => moveId == removedMoveId);
        Assert.Contains(species.LearnableMoveIds, moveId => moveId == addedMoveId);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_Species_Does_Not_Exist()
    {
        var speciesRepository = Substitute.For<IPokemonSpeciesWriteRepository>();
        speciesRepository.GetForUpdateWithLearnableMovesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((PokemonSpecies?)null);

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdatePokemonSpeciesLearnableMovesCommandHandler(speciesRepository, moveRepository, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new UpdatePokemonSpeciesLearnableMovesCommand(Guid.NewGuid(), [Guid.NewGuid()], []),
            CancellationToken.None));

        Assert.Equal("id", exception.Target);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Duplicate_Association_When_Move_Is_Already_Learnable()
    {
        var moveId = Guid.NewGuid();
        var species = PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create("Pikachu"),
            PokemonTyping.Create([PokemonType.Electric]),
            BaseStats.Create(35, 55, 40, 50, 50, 90));
        species.AddLearnableMove(moveId);

        var speciesRepository = Substitute.For<IPokemonSpeciesWriteRepository>();
        speciesRepository.GetForUpdateWithLearnableMovesAsync(species.Id, Arg.Any<CancellationToken>())
            .Returns(species);

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        moveRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([PokemonMove.Create(moveId, Move.Create("Protect", PokemonType.Normal, MoveCategory.Status, 0))]);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdatePokemonSpeciesLearnableMovesCommandHandler(speciesRepository, moveRepository, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new UpdatePokemonSpeciesLearnableMovesCommand(species.Id, [moveId], []),
            CancellationToken.None));

        Assert.Contains("addMoveIds", exception.Errors.Keys);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_When_Referenced_Move_Does_Not_Exist()
    {
        var species = PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create("Dragonite"),
            PokemonTyping.Create([PokemonType.Dragon, PokemonType.Flying]),
            BaseStats.Create(91, 134, 95, 100, 100, 80));
        var missingMoveId = Guid.NewGuid();

        var speciesRepository = Substitute.For<IPokemonSpeciesWriteRepository>();
        speciesRepository.GetForUpdateWithLearnableMovesAsync(species.Id, Arg.Any<CancellationToken>())
            .Returns(species);

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        moveRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Array.Empty<PokemonMove>());

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdatePokemonSpeciesLearnableMovesCommandHandler(speciesRepository, moveRepository, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new UpdatePokemonSpeciesLearnableMovesCommand(species.Id, [missingMoveId], []),
            CancellationToken.None));

        Assert.Contains("moveIds", exception.Errors.Keys);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
