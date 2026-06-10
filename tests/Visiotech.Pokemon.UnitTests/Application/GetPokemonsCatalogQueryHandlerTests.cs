using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonsCatalog;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class GetPokemonsCatalogQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Ordered_Catalog()
    {
        var repository = new FakePokemonSpeciesReadRepository(
        [
            PokemonSpecies.Create(
                Guid.NewGuid(),
                Name.Create("Squirtle"),
                PokemonTyping.Create([PokemonType.Water]),
                BaseStats.Create(44, 48, 65, 50, 64, 43)),
            PokemonSpecies.Create(
                Guid.NewGuid(),
                Name.Create("Charmander"),
                PokemonTyping.Create([PokemonType.Fire]),
                BaseStats.Create(39, 52, 43, 60, 50, 65))
        ]);

        var handler = new GetPokemonsCatalogQueryHandler(repository);

        var result = await handler.Handle(new GetPokemonsCatalogQuery(), CancellationToken.None);

        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal("Charmander", first.Name);
                Assert.Equal(["Fire"], first.Types);
            },
            second => Assert.Equal("Squirtle", second.Name));
    }

    private sealed class FakePokemonSpeciesReadRepository(IReadOnlyCollection<PokemonSpecies> pokemonSpecies)
        : IPokemonSpeciesReadRepository
    {
        public Task<IReadOnlyCollection<PokemonSpecies>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult(pokemonSpecies);
    }
}
