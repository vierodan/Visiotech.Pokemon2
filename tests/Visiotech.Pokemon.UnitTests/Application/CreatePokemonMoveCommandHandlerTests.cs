using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.Moves.Commands.CreatePokemonMove;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class CreatePokemonMoveCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Move_When_Command_Is_Valid()
    {
        var repository = Substitute.For<IPokemonMoveWriteRepository>();
        repository.ExistsByNormalizedNameAsync("THUNDERBOLT", Arg.Any<CancellationToken>()).Returns(false);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreatePokemonMoveCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new CreatePokemonMoveCommand("Thunderbolt", "Electric", "Special", 90),
            CancellationToken.None);

        Assert.Equal("Thunderbolt", result.Name);
        Assert.Equal("Electric", result.Type);
        Assert.Equal("Special", result.Category);
        Assert.Equal(90, result.Power);
        await repository.Received(1).AddAsync(
            Arg.Is<PokemonMove>(move =>
                move.Name.Value == "Thunderbolt"
                && move.Type == PokemonType.Electric
                && move.Category == MoveCategory.Special
                && move.Power == 90),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Duplicate_Name()
    {
        var repository = Substitute.For<IPokemonMoveWriteRepository>();
        repository.ExistsByNormalizedNameAsync("SURF", Arg.Any<CancellationToken>()).Returns(true);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreatePokemonMoveCommandHandler(repository, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationConflictException>(() => handler.Handle(
            new CreatePokemonMoveCommand("Surf", "Water", "Special", 90),
            CancellationToken.None));

        Assert.Equal("name", exception.Target);
        await repository.DidNotReceive().AddAsync(Arg.Any<PokemonMove>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Invalid_Type()
    {
        var repository = Substitute.For<IPokemonMoveWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreatePokemonMoveCommandHandler(repository, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new CreatePokemonMoveCommand("Thunderbolt", "Light", "Special", 90),
            CancellationToken.None));

        Assert.Contains("type", exception.Errors.Keys);
        await repository.DidNotReceive().AddAsync(Arg.Any<PokemonMove>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Invalid_Category()
    {
        var repository = Substitute.For<IPokemonMoveWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreatePokemonMoveCommandHandler(repository, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new CreatePokemonMoveCommand("Thunderbolt", "Electric", "Support", 90),
            CancellationToken.None));

        Assert.Contains("category", exception.Errors.Keys);
        await repository.DidNotReceive().AddAsync(Arg.Any<PokemonMove>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Inconsistent_Power()
    {
        var repository = Substitute.For<IPokemonMoveWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreatePokemonMoveCommandHandler(repository, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new CreatePokemonMoveCommand("Thunderbolt", "Electric", "Special", 0),
            CancellationToken.None));

        Assert.Contains("power", exception.Errors.Keys);
        await repository.DidNotReceive().AddAsync(Arg.Any<PokemonMove>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
