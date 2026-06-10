using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.UpdatePokemonSpecies;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class UpdatePokemonSpeciesCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Update_Species_When_Command_Is_Valid()
    {
        var existingSpecies = PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create("Charizard"),
            PokemonTyping.Create([PokemonType.Fire, PokemonType.Flying]),
            BaseStats.Create(78, 84, 78, 109, 85, 100));

        var repository = Substitute.For<IPokemonSpeciesWriteRepository>();
        repository.GetForUpdateAsync(existingSpecies.Id, Arg.Any<CancellationToken>()).Returns(existingSpecies);
        repository.ExistsByNormalizedNameAsync("CHARIZARD APEX", existingSpecies.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdatePokemonSpeciesCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new UpdatePokemonSpeciesCommand(
                existingSpecies.Id,
                "Charizard Apex",
                ["Fire", "Dragon"],
                80,
                90,
                82,
                120,
                90,
                105),
            CancellationToken.None);

        Assert.Equal(existingSpecies.Id, result.Id);
        Assert.Equal("Charizard Apex", result.Name);
        Assert.Equal(["Fire", "Dragon"], result.Types);
        Assert.Equal("Charizard Apex", existingSpecies.Name.Value);
        Assert.Equal(["Fire", "Dragon"], existingSpecies.Types.Select(type => type.ToString()).ToArray());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_Species_Does_Not_Exist()
    {
        var repository = Substitute.For<IPokemonSpeciesWriteRepository>();
        repository.GetForUpdateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((PokemonSpecies?)null);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdatePokemonSpeciesCommandHandler(repository, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new UpdatePokemonSpeciesCommand(
                Guid.NewGuid(),
                "Charizard",
                ["Fire", "Flying"],
                78,
                84,
                78,
                109,
                85,
                100),
            CancellationToken.None));

        Assert.Equal("id", exception.Target);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Duplicate_Name()
    {
        var existingSpecies = PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create("Charizard"),
            PokemonTyping.Create([PokemonType.Fire, PokemonType.Flying]),
            BaseStats.Create(78, 84, 78, 109, 85, 100));

        var repository = Substitute.For<IPokemonSpeciesWriteRepository>();
        repository.GetForUpdateAsync(existingSpecies.Id, Arg.Any<CancellationToken>()).Returns(existingSpecies);
        repository.ExistsByNormalizedNameAsync("BLASTOISE", existingSpecies.Id, Arg.Any<CancellationToken>())
            .Returns(true);

        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdatePokemonSpeciesCommandHandler(repository, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationConflictException>(() => handler.Handle(
            new UpdatePokemonSpeciesCommand(
                existingSpecies.Id,
                "Blastoise",
                ["Water"],
                79,
                83,
                100,
                85,
                105,
                78),
            CancellationToken.None));

        Assert.Equal("name", exception.Target);
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Reject_Invalid_Types()
    {
        var repository = Substitute.For<IPokemonSpeciesWriteRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdatePokemonSpeciesCommandHandler(repository, unitOfWork);

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new UpdatePokemonSpeciesCommand(
                Guid.NewGuid(),
                "Golem",
                ["Rock", "Rock", "Ground"],
                80,
                120,
                130,
                55,
                65,
                45),
            CancellationToken.None));

        Assert.Contains("types", exception.Errors.Keys);
        await repository.DidNotReceive().GetForUpdateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
