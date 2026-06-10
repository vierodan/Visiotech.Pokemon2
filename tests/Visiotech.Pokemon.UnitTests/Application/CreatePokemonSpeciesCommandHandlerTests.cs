using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.CreatePokemonSpecies;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class CreatePokemonSpeciesCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Create_Species_When_Command_Is_Valid()
    {
        var repository = Substitute.For<IPokemonSpeciesWriteRepository>();
        repository.ExistsByNormalizedNameAsync("BLASTOISE", Arg.Any<CancellationToken>()).Returns(false);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreatePokemonSpeciesCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new CreatePokemonSpeciesCommand(
                "Blastoise",
                ["Water"],
                79,
                83,
                100,
                85,
                105,
                78),
            CancellationToken.None);

        Assert.Equal("Blastoise", result.Name);
        Assert.Equal(["Water"], result.Types);
        await repository.Received(1).AddAsync(
            Arg.Is<PokemonSpecies>(species => species.Name.Value == "Blastoise" && species.Types.Count == 1),
            Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Invalid_Request_With_Explicit_Errors()
    {
        var repository = Substitute.For<IPokemonSpeciesWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreatePokemonSpeciesCommandHandler(repository, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new CreatePokemonSpeciesCommand(
                "",
                ["Fire", "Fire", "Flying"],
                0,
                -1,
                0,
                0,
                0,
                0),
            CancellationToken.None));

        Assert.Contains("name", exception.Errors.Keys);
        Assert.Contains("types", exception.Errors.Keys);
        Assert.Contains("baseStats.health", exception.Errors.Keys);
        await repository.DidNotReceive().AddAsync(Arg.Any<PokemonSpecies>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Duplicate_Name()
    {
        var repository = Substitute.For<IPokemonSpeciesWriteRepository>();
        repository.ExistsByNormalizedNameAsync("PIKACHU", Arg.Any<CancellationToken>()).Returns(true);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreatePokemonSpeciesCommandHandler(repository, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationConflictException>(() => handler.Handle(
            new CreatePokemonSpeciesCommand(
                "Pikachu",
                ["Electric"],
                35,
                55,
                40,
                50,
                50,
                90),
            CancellationToken.None));

        Assert.Equal("name", exception.Target);
        await repository.DidNotReceive().AddAsync(Arg.Any<PokemonSpecies>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
