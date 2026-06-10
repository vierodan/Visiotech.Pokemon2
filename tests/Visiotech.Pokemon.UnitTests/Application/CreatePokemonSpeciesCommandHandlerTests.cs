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
        var repository = new FakePokemonSpeciesWriteRepository();
        var unitOfWork = new FakeUnitOfWork();
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
        Assert.True(unitOfWork.SaveChangesCalled);
        Assert.Single(repository.AddedSpecies);
    }

    [Fact]
    public async Task Handle_Should_Reject_Invalid_Request_With_Explicit_Errors()
    {
        var handler = new CreatePokemonSpeciesCommandHandler(
            new FakePokemonSpeciesWriteRepository(),
            new FakeUnitOfWork());

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
    }

    [Fact]
    public async Task Handle_Should_Reject_Duplicate_Name()
    {
        var repository = new FakePokemonSpeciesWriteRepository(existsByName: true);
        var handler = new CreatePokemonSpeciesCommandHandler(repository, new FakeUnitOfWork());

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
    }

    private sealed class FakePokemonSpeciesWriteRepository(bool existsByName = false) : IPokemonSpeciesWriteRepository
    {
        public List<PokemonSpecies> AddedSpecies { get; } = [];

        public Task AddAsync(PokemonSpecies pokemonSpecies, CancellationToken cancellationToken)
        {
            AddedSpecies.Add(pokemonSpecies);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken) =>
            Task.FromResult(existsByName);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public bool SaveChangesCalled { get; private set; }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCalled = true;
            return Task.CompletedTask;
        }
    }
}
