using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.Moves.Queries.GetPokemonMoveSharedSpecies;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class GetPokemonMoveSharedSpeciesQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Pokemon_Species_Sharing_The_Move()
    {
        var move = PokemonMove.Create(
            Guid.NewGuid(),
            Move.Create("Protect", PokemonType.Normal, MoveCategory.Status, 0));

        var charizard = PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create("Charizard"),
            PokemonTyping.Create([PokemonType.Fire, PokemonType.Flying]),
            BaseStats.Create(78, 84, 78, 109, 85, 100));
        var blastoise = PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create("Blastoise"),
            PokemonTyping.Create([PokemonType.Water]),
            BaseStats.Create(79, 83, 100, 85, 105, 78));

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        moveRepository.GetByIdAsync(move.Id, Arg.Any<CancellationToken>())
            .Returns(move);

        var speciesRepository = Substitute.For<IPokemonSpeciesReadRepository>();
        speciesRepository.GetByLearnableMoveIdAsync(move.Id, Arg.Any<CancellationToken>())
            .Returns([charizard, blastoise]);

        var handler = new GetPokemonMoveSharedSpeciesQueryHandler(moveRepository, speciesRepository);

        var result = await handler.Handle(new GetPokemonMoveSharedSpeciesQuery(move.Id), CancellationToken.None);

        Assert.Equal(move.Id, result.PokemonMoveId);
        Assert.Equal("Protect", result.PokemonMoveName);
        Assert.Equal(2, result.PokemonSpecies.Count);
        Assert.Collection(
            result.PokemonSpecies,
            species => Assert.Equal("Charizard", species.Name),
            species => Assert.Equal("Blastoise", species.Name));
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_Move_Does_Not_Exist()
    {
        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        moveRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((PokemonMove?)null);

        var speciesRepository = Substitute.For<IPokemonSpeciesReadRepository>();
        var handler = new GetPokemonMoveSharedSpeciesQueryHandler(moveRepository, speciesRepository);

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new GetPokemonMoveSharedSpeciesQuery(Guid.NewGuid()),
            CancellationToken.None));

        Assert.Equal("id", exception.Target);
        await speciesRepository.DidNotReceive().GetByLearnableMoveIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Species_Can_Learn_The_Move()
    {
        var move = PokemonMove.Create(
            Guid.NewGuid(),
            Move.Create("Protect", PokemonType.Normal, MoveCategory.Status, 0));

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        moveRepository.GetByIdAsync(move.Id, Arg.Any<CancellationToken>())
            .Returns(move);

        var speciesRepository = Substitute.For<IPokemonSpeciesReadRepository>();
        speciesRepository.GetByLearnableMoveIdAsync(move.Id, Arg.Any<CancellationToken>())
            .Returns([]);

        var handler = new GetPokemonMoveSharedSpeciesQueryHandler(moveRepository, speciesRepository);

        var result = await handler.Handle(new GetPokemonMoveSharedSpeciesQuery(move.Id), CancellationToken.None);

        Assert.Equal(move.Id, result.PokemonMoveId);
        Assert.Empty(result.PokemonSpecies);
    }
}
