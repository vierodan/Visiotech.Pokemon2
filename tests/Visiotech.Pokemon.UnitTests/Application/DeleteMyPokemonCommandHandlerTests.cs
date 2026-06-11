using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.MyPokemons.Commands.DeleteMyPokemon;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class DeleteMyPokemonCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Delete_MyPokemon_When_There_Are_No_Dependencies()
    {
        var existingMyPokemon = MyPokemon.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Level.Create(50),
            120,
            150,
            [Guid.NewGuid(), Guid.NewGuid()]);

        var repository = Substitute.For<IMyPokemonWriteRepository>();
        repository.GetForUpdateAsync(existingMyPokemon.Id, Arg.Any<CancellationToken>())
            .Returns(existingMyPokemon);

        var dependencyChecker = Substitute.For<IMyPokemonDeletionDependencyChecker>();
        dependencyChecker.GetBlockingReasonsAsync(existingMyPokemon.Id, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<string>());

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeleteMyPokemonCommandHandler(repository, dependencyChecker, unitOfWork);

        var result = await handler.Handle(new DeleteMyPokemonCommand(existingMyPokemon.Id), CancellationToken.None);

        Assert.Equal(existingMyPokemon.Id, result);
        repository.Received(1).Remove(existingMyPokemon);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_MyPokemon_Does_Not_Exist()
    {
        var repository = Substitute.For<IMyPokemonWriteRepository>();
        repository.GetForUpdateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((MyPokemon?)null);

        var dependencyChecker = Substitute.For<IMyPokemonDeletionDependencyChecker>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeleteMyPokemonCommandHandler(repository, dependencyChecker, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new DeleteMyPokemonCommand(Guid.NewGuid()),
            CancellationToken.None));

        Assert.Equal("id", exception.Target);
        await dependencyChecker.DidNotReceive().GetBlockingReasonsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        repository.DidNotReceive().Remove(Arg.Any<MyPokemon>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Delete_When_Active_Battle_Dependencies_Exist()
    {
        var existingMyPokemon = MyPokemon.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Level.Create(45),
            100,
            120,
            [Guid.NewGuid()]);

        var repository = Substitute.For<IMyPokemonWriteRepository>();
        repository.GetForUpdateAsync(existingMyPokemon.Id, Arg.Any<CancellationToken>())
            .Returns(existingMyPokemon);

        var dependencyChecker = Substitute.For<IMyPokemonDeletionDependencyChecker>();
        dependencyChecker.GetBlockingReasonsAsync(existingMyPokemon.Id, Arg.Any<CancellationToken>())
            .Returns(["My pokemon cannot be deleted because it participates in active battle 'battle-123'."]);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeleteMyPokemonCommandHandler(repository, dependencyChecker, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new DeleteMyPokemonCommand(existingMyPokemon.Id),
            CancellationToken.None));

        Assert.Contains("dependencies", exception.Errors.Keys);
        repository.DidNotReceive().Remove(Arg.Any<MyPokemon>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
