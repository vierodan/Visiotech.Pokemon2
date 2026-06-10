using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Moves.Queries.GetPokemonMoveDetail;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class GetPokemonMoveDetailQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Move_Detail_When_It_Exists()
    {
        var move = PokemonMove.Create(
            Guid.NewGuid(),
            Move.Create("Thunderbolt", PokemonType.Electric, MoveCategory.Special, 90));

        var handler = new GetPokemonMoveDetailQueryHandler(new FakePokemonMoveReadRepository(move));

        var result = await handler.Handle(new GetPokemonMoveDetailQuery(move.Id), CancellationToken.None);

        Assert.Equal(move.Id, result.Id);
        Assert.Equal("Thunderbolt", result.Name);
        Assert.Equal("Electric", result.Type);
        Assert.Equal("Special", result.Category);
        Assert.Equal(90, result.Power);
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_Move_Does_Not_Exist()
    {
        var handler = new GetPokemonMoveDetailQueryHandler(new FakePokemonMoveReadRepository(null));

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new GetPokemonMoveDetailQuery(Guid.NewGuid()),
            CancellationToken.None));

        Assert.Equal("id", exception.Target);
    }

    private sealed class FakePokemonMoveReadRepository(PokemonMove? move) : IPokemonMoveReadRepository
    {
        public Task<PokemonMove?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(move);

        public Task<IReadOnlyCollection<PokemonMove>> GetByIdsAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyCollection<PokemonMove>>(
                move is null || !ids.Contains(move.Id)
                    ? []
                    : [move]);

        public Task<PokemonMoveCatalogPage> SearchAsync(PokemonMoveCatalogFilter filter, CancellationToken cancellationToken) =>
            Task.FromResult(new PokemonMoveCatalogPage([], 0));
    }
}
