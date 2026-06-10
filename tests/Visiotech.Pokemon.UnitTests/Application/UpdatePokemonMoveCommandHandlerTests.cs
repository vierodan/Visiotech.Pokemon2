using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.Moves.Commands.UpdatePokemonMove;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class UpdatePokemonMoveCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Update_Move_When_Command_Is_Valid()
    {
        var existingMove = PokemonMove.Create(
            Guid.NewGuid(),
            Move.Create("Thunderbolt", PokemonType.Electric, MoveCategory.Special, 90));

        var repository = Substitute.For<IPokemonMoveWriteRepository>();
        repository.GetForUpdateAsync(existingMove.Id, Arg.Any<CancellationToken>()).Returns(existingMove);
        repository.ExistsByNormalizedNameAsync("THUNDER STRIKE", existingMove.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdatePokemonMoveCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new UpdatePokemonMoveCommand(
                existingMove.Id,
                "Thunder Strike",
                "Electric",
                "Special",
                95),
            CancellationToken.None);

        Assert.Equal(existingMove.Id, result.Id);
        Assert.Equal("Thunder Strike", result.Name);
        Assert.Equal("Electric", result.Type);
        Assert.Equal("Special", result.Category);
        Assert.Equal(95, result.Power);
        Assert.Equal("Thunder Strike", existingMove.Name.Value);
        Assert.Equal(PokemonType.Electric, existingMove.Type);
        Assert.Equal(MoveCategory.Special, existingMove.Category);
        Assert.Equal(95, existingMove.Power);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_Move_Does_Not_Exist()
    {
        var repository = Substitute.For<IPokemonMoveWriteRepository>();
        repository.GetForUpdateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((PokemonMove?)null);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdatePokemonMoveCommandHandler(repository, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new UpdatePokemonMoveCommand(Guid.NewGuid(), "Surf", "Water", "Special", 90),
            CancellationToken.None));

        Assert.Equal("id", exception.Target);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Duplicate_Name()
    {
        var existingMove = PokemonMove.Create(
            Guid.NewGuid(),
            Move.Create("Thunderbolt", PokemonType.Electric, MoveCategory.Special, 90));

        var repository = Substitute.For<IPokemonMoveWriteRepository>();
        repository.GetForUpdateAsync(existingMove.Id, Arg.Any<CancellationToken>()).Returns(existingMove);
        repository.ExistsByNormalizedNameAsync("SURF", existingMove.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdatePokemonMoveCommandHandler(repository, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationConflictException>(() => handler.Handle(
            new UpdatePokemonMoveCommand(existingMove.Id, "Surf", "Water", "Special", 90),
            CancellationToken.None));

        Assert.Equal("name", exception.Target);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Invalid_Data()
    {
        var repository = Substitute.For<IPokemonMoveWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdatePokemonMoveCommandHandler(repository, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new UpdatePokemonMoveCommand(Guid.NewGuid(), "Protect", "Normal", "Status", 10),
            CancellationToken.None));

        Assert.Contains("power", exception.Errors.Keys);
        await repository.DidNotReceive().GetForUpdateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
