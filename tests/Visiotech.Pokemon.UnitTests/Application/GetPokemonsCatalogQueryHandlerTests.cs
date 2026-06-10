using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonsCatalog;
using Visiotech.Pokemon.Domain.Pokemons;
using PokemonAggregate = global::Visiotech.Pokemon.Domain.Pokemons.Pokemon;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class GetPokemonsCatalogQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Ordered_Catalog()
    {
        var repository = new FakePokemonReadRepository(
        [
            PokemonAggregate.Create(
                Guid.NewGuid(),
                Name.Create("Squirtle"),
                PokemonType.Water,
                Level.Create(12),
                BaseStats.Create(44, 48, 65, 50, 64, 43),
                [Move.Create("Water Gun", PokemonType.Water, 40)],
                [Ability.Create("Torrent")]),
            PokemonAggregate.Create(
                Guid.NewGuid(),
                Name.Create("Charmander"),
                PokemonType.Fire,
                Level.Create(12),
                BaseStats.Create(39, 52, 43, 60, 50, 65),
                [Move.Create("Ember", PokemonType.Fire, 40)],
                [Ability.Create("Blaze")])
        ]);

        var handler = new GetPokemonsCatalogQueryHandler(repository);

        var result = await handler.Handle(new GetPokemonsCatalogQuery(), CancellationToken.None);

        Assert.Collection(
            result,
            first =>
            {
                Assert.Equal("Charmander", first.Name);
                Assert.Single(first.Moves);
            },
            second => Assert.Equal("Squirtle", second.Name));
    }

    private sealed class FakePokemonReadRepository(IReadOnlyCollection<PokemonAggregate> pokemons)
        : IPokemonReadRepository
    {
        public Task<IReadOnlyCollection<PokemonAggregate>> GetAllAsync(CancellationToken cancellationToken) =>
            Task.FromResult(pokemons);
    }
}
