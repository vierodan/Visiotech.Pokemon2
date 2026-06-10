using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonSpeciesDetail;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class GetPokemonSpeciesDetailQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Species_Detail_When_It_Exists()
    {
        var species = PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create("Dragonite"),
            PokemonTyping.Create([PokemonType.Dragon, PokemonType.Flying]),
            BaseStats.Create(91, 134, 95, 100, 100, 80));

        var handler = new GetPokemonSpeciesDetailQueryHandler(new FakePokemonSpeciesReadRepository(species));

        var result = await handler.Handle(new GetPokemonSpeciesDetailQuery(species.Id), CancellationToken.None);

        Assert.Equal(species.Id, result.Id);
        Assert.Equal("Dragonite", result.Name);
        Assert.Equal(["Dragon", "Flying"], result.Types);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_Species_Does_Not_Exist()
    {
        var handler = new GetPokemonSpeciesDetailQueryHandler(new FakePokemonSpeciesReadRepository(null));

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new GetPokemonSpeciesDetailQuery(Guid.NewGuid()),
            CancellationToken.None));

        Assert.Equal("id", exception.Target);
    }

    private sealed class FakePokemonSpeciesReadRepository(PokemonSpecies? species) : IPokemonSpeciesReadRepository
    {
        public Task<PokemonSpecies?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(species);

        public Task<PokemonSpeciesCatalogPage> SearchAsync(PokemonSpeciesCatalogFilter filter, CancellationToken cancellationToken) =>
            Task.FromResult(new PokemonSpeciesCatalogPage([], 0));
    }
}
