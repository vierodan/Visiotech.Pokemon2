using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.DeletePokemonSpecies;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class DeletePokemonSpeciesCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Delete_Species_When_There_Are_No_Dependencies()
    {
        var existingSpecies = PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create("Charizard"),
            PokemonTyping.Create([PokemonType.Fire, PokemonType.Flying]),
            BaseStats.Create(78, 84, 78, 109, 85, 100));

        var repository = Substitute.For<IPokemonSpeciesWriteRepository>();
        repository.GetForUpdateAsync(existingSpecies.Id, Arg.Any<CancellationToken>()).Returns(existingSpecies);

        var dependencyChecker = Substitute.For<IPokemonSpeciesDeletionDependencyChecker>();
        dependencyChecker.GetBlockingReasonsAsync(existingSpecies.Id, Arg.Any<CancellationToken>())
            .Returns(Array.Empty<string>());

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeletePokemonSpeciesCommandHandler(repository, dependencyChecker, unitOfWork);

        var result = await handler.Handle(new DeletePokemonSpeciesCommand(existingSpecies.Id), CancellationToken.None);

        Assert.Equal(existingSpecies.Id, result);
        repository.Received(1).Remove(existingSpecies);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_Species_Does_Not_Exist()
    {
        var repository = Substitute.For<IPokemonSpeciesWriteRepository>();
        repository.GetForUpdateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((PokemonSpecies?)null);

        var dependencyChecker = Substitute.For<IPokemonSpeciesDeletionDependencyChecker>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeletePokemonSpeciesCommandHandler(repository, dependencyChecker, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new DeletePokemonSpeciesCommand(Guid.NewGuid()),
            CancellationToken.None));

        Assert.Equal("id", exception.Target);
        await dependencyChecker.DidNotReceive().GetBlockingReasonsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        repository.DidNotReceive().Remove(Arg.Any<PokemonSpecies>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Delete_When_Dependencies_Exist()
    {
        var existingSpecies = PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create("Blastoise"),
            PokemonTyping.Create([PokemonType.Water]),
            BaseStats.Create(79, 83, 100, 85, 105, 78));

        var repository = Substitute.For<IPokemonSpeciesWriteRepository>();
        repository.GetForUpdateAsync(existingSpecies.Id, Arg.Any<CancellationToken>()).Returns(existingSpecies);

        var dependencyChecker = Substitute.For<IPokemonSpeciesDeletionDependencyChecker>();
        dependencyChecker.GetBlockingReasonsAsync(existingSpecies.Id, Arg.Any<CancellationToken>())
            .Returns(["Pokemon species cannot be deleted because it is referenced by 'MyPokemon'."]);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeletePokemonSpeciesCommandHandler(repository, dependencyChecker, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new DeletePokemonSpeciesCommand(existingSpecies.Id),
            CancellationToken.None));

        Assert.Contains("dependencies", exception.Errors.Keys);
        repository.DidNotReceive().Remove(Arg.Any<PokemonSpecies>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
