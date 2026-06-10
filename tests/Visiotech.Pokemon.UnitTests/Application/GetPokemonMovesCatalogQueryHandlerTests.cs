using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Moves.Queries.GetPokemonMovesCatalog;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class GetPokemonMovesCatalogQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Ordered_Catalog_With_Pagination_Metadata()
    {
        var repository = new FakePokemonMoveReadRepository(
            (_, _) => Task.FromResult(new PokemonMoveCatalogPage(
            [
                CreateMove("Thunderbolt", PokemonType.Electric, MoveCategory.Special, 90),
                CreateMove("Body Slam", PokemonType.Normal, MoveCategory.Physical, 85)
            ],
            2)));

        var handler = new GetPokemonMovesCatalogQueryHandler(repository);

        var result = await handler.Handle(new GetPokemonMovesCatalogQuery(null, null, null, 1, 20), CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
        Assert.Equal(1, result.TotalPages);
        Assert.Collection(
            result.Items,
            first => Assert.Equal("Thunderbolt", first.Name),
            second => Assert.Equal("Body Slam", second.Name));
    }

    [Fact]
    public async Task Handle_Should_Pass_Normalized_Filters_To_Repository()
    {
        PokemonMoveCatalogFilter? capturedFilter = null;
        var repository = new FakePokemonMoveReadRepository((filter, _) =>
        {
            capturedFilter = filter;
            return Task.FromResult(new PokemonMoveCatalogPage([], 0));
        });

        var handler = new GetPokemonMovesCatalogQueryHandler(repository);

        await handler.Handle(new GetPokemonMovesCatalogQuery(" thunder ", "electric", "special", 2, 5), CancellationToken.None);

        Assert.NotNull(capturedFilter);
        Assert.Equal("THUNDER", capturedFilter.NormalizedName);
        Assert.Equal(PokemonType.Electric, capturedFilter.Type);
        Assert.Equal(MoveCategory.Special, capturedFilter.Category);
        Assert.Equal(2, capturedFilter.Page);
        Assert.Equal(5, capturedFilter.PageSize);
    }

    [Fact]
    public async Task Handle_Should_Reject_Invalid_Pagination_And_Filters()
    {
        var handler = new GetPokemonMovesCatalogQueryHandler(new FakePokemonMoveReadRepository((_, _) =>
            Task.FromResult(new PokemonMoveCatalogPage([], 0))));

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new GetPokemonMovesCatalogQuery(null, "light", "support", 0, 101),
            CancellationToken.None));

        Assert.Contains("page", exception.Errors.Keys);
        Assert.Contains("pageSize", exception.Errors.Keys);
        Assert.Contains("type", exception.Errors.Keys);
        Assert.Contains("category", exception.Errors.Keys);
    }

    private static PokemonMove CreateMove(string name, PokemonType type, MoveCategory category, int power) =>
        PokemonMove.Create(Guid.NewGuid(), Move.Create(name, type, category, power));

    private sealed class FakePokemonMoveReadRepository(
        Func<PokemonMoveCatalogFilter, CancellationToken, Task<PokemonMoveCatalogPage>> search)
        : IPokemonMoveReadRepository
    {
        public Task<PokemonMove?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<PokemonMove?>(null);

        public Task<PokemonMoveCatalogPage> SearchAsync(PokemonMoveCatalogFilter filter, CancellationToken cancellationToken) =>
            search(filter, cancellationToken);
    }
}
