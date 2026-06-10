using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonsCatalog;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class GetPokemonsCatalogQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Ordered_Catalog_With_Pagination_Metadata()
    {
        var repository = new FakePokemonSpeciesReadRepository(
            (_, _) => Task.FromResult(new PokemonSpeciesCatalogPage(
            [
                CreateSpecies("Squirtle", [PokemonType.Water], 44, 48, 65, 50, 64, 43),
                CreateSpecies("Charmander", [PokemonType.Fire], 39, 52, 43, 60, 50, 65)
            ],
            2)));

        var handler = new GetPokemonsCatalogQueryHandler(repository);

        var result = await handler.Handle(new GetPokemonsCatalogQuery(null, null, 1, 20), CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(1, result.TotalPages);
        Assert.Collection(
            result.Items,
            first => Assert.Equal("Squirtle", first.Name),
            second => Assert.Equal("Charmander", second.Name));
    }

    [Fact]
    public async Task Handle_Should_Pass_Normalized_Filters_To_Repository()
    {
        PokemonSpeciesCatalogFilter? capturedFilter = null;
        var repository = new FakePokemonSpeciesReadRepository((filter, _) =>
        {
            capturedFilter = filter;
            return Task.FromResult(new PokemonSpeciesCatalogPage([], 0));
        });

        var handler = new GetPokemonsCatalogQueryHandler(repository);

        await handler.Handle(new GetPokemonsCatalogQuery(" char ", "fire", 2, 5), CancellationToken.None);

        Assert.NotNull(capturedFilter);
        Assert.Equal("CHAR", capturedFilter.NormalizedName);
        Assert.Equal(PokemonType.Fire, capturedFilter.Type);
        Assert.Equal(2, capturedFilter.Page);
        Assert.Equal(5, capturedFilter.PageSize);
    }

    [Fact]
    public async Task Handle_Should_Reject_Invalid_Pagination()
    {
        var handler = new GetPokemonsCatalogQueryHandler(new FakePokemonSpeciesReadRepository((_, _) =>
            Task.FromResult(new PokemonSpeciesCatalogPage([], 0))));

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new GetPokemonsCatalogQuery(null, null, 0, 101),
            CancellationToken.None));

        Assert.Contains("page", exception.Errors.Keys);
        Assert.Contains("pageSize", exception.Errors.Keys);
    }

    private static PokemonSpecies CreateSpecies(
        string name,
        IReadOnlyCollection<PokemonType> types,
        int health,
        int attack,
        int defense,
        int specialAttack,
        int specialDefense,
        int speed) =>
        PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create(name),
            PokemonTyping.Create(types),
            BaseStats.Create(health, attack, defense, specialAttack, specialDefense, speed));

    private sealed class FakePokemonSpeciesReadRepository(
        Func<PokemonSpeciesCatalogFilter, CancellationToken, Task<PokemonSpeciesCatalogPage>> search)
        : IPokemonSpeciesReadRepository
    {
        public Task<PokemonSpecies?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<PokemonSpecies?>(null);

        public Task<PokemonSpecies?> GetByIdWithLearnableMovesAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<PokemonSpecies?>(null);

        public Task<IReadOnlyCollection<PokemonSpecies>> GetByLearnableMoveIdAsync(Guid moveId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<PokemonSpecies>>([]);

        public Task<PokemonSpeciesCatalogPage> SearchAsync(PokemonSpeciesCatalogFilter filter, CancellationToken cancellationToken) =>
            search(filter, cancellationToken);
    }
}
