using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.Moves.Commands.DeletePokemonMove;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class DeletePokemonMoveCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Delete_Move_When_There_Are_No_Dependencies()
    {
        var existingMove = PokemonMove.Create(
            Guid.NewGuid(),
            Move.Create("Thunderbolt", PokemonType.Electric, MoveCategory.Special, 90));

        var repository = Substitute.For<IPokemonMoveWriteRepository>();
        repository.GetForUpdateAsync(existingMove.Id, Arg.Any<CancellationToken>()).Returns(existingMove);

        var dependencyChecker = Substitute.For<IPokemonMoveDeletionDependencyChecker>();
        dependencyChecker.GetBlockingReasonsAsync(existingMove.Id, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<string>());

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeletePokemonMoveCommandHandler(repository, dependencyChecker, unitOfWork);

        var result = await handler.Handle(new DeletePokemonMoveCommand(existingMove.Id), CancellationToken.None);

        Assert.Equal(existingMove.Id, result);
        repository.Received(1).Remove(existingMove);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_Move_Does_Not_Exist()
    {
        var repository = Substitute.For<IPokemonMoveWriteRepository>();
        repository.GetForUpdateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((PokemonMove?)null);

        var dependencyChecker = Substitute.For<IPokemonMoveDeletionDependencyChecker>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeletePokemonMoveCommandHandler(repository, dependencyChecker, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new DeletePokemonMoveCommand(Guid.NewGuid()),
            CancellationToken.None));

        Assert.Equal("id", exception.Target);
        await dependencyChecker.DidNotReceive().GetBlockingReasonsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        repository.DidNotReceive().Remove(Arg.Any<PokemonMove>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Delete_When_Dependencies_Exist()
    {
        var existingMove = PokemonMove.Create(
            Guid.NewGuid(),
            Move.Create("Protect", PokemonType.Normal, MoveCategory.Status, 0));

        var repository = Substitute.For<IPokemonMoveWriteRepository>();
        repository.GetForUpdateAsync(existingMove.Id, Arg.Any<CancellationToken>()).Returns(existingMove);

        var dependencyChecker = Substitute.For<IPokemonMoveDeletionDependencyChecker>();
        dependencyChecker.GetBlockingReasonsAsync(existingMove.Id, Arg.Any<CancellationToken>())
            .Returns(["Pokemon move cannot be deleted because it is referenced by 'MyPokemonMoveSlot'."]);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeletePokemonMoveCommandHandler(repository, dependencyChecker, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new DeletePokemonMoveCommand(existingMove.Id),
            CancellationToken.None));

        Assert.Contains("dependencies", exception.Errors.Keys);
        repository.DidNotReceive().Remove(Arg.Any<PokemonMove>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
