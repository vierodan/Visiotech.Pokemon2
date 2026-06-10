using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonSpeciesLearnableMoves;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class GetPokemonSpeciesLearnableMovesQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Learnable_Moves_When_Species_Exists()
    {
        var surfId = Guid.NewGuid();
        var protectId = Guid.NewGuid();

        var species = PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create("Blastoise"),
            PokemonTyping.Create([PokemonType.Water]),
            BaseStats.Create(79, 83, 100, 85, 105, 78));
        species.AddLearnableMove(protectId);
        species.AddLearnableMove(surfId);

        var speciesRepository = Substitute.For<IPokemonSpeciesReadRepository>();
        speciesRepository.GetByIdWithLearnableMovesAsync(species.Id, Arg.Any<CancellationToken>())
            .Returns(species);

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        moveRepository.GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(
            [
                PokemonMove.Create(surfId, Move.Create("Surf", PokemonType.Water, MoveCategory.Special, 90)),
                PokemonMove.Create(protectId, Move.Create("Protect", PokemonType.Normal, MoveCategory.Status, 0))
            ]);

        var handler = new GetPokemonSpeciesLearnableMovesQueryHandler(speciesRepository, moveRepository);

        var result = await handler.Handle(new GetPokemonSpeciesLearnableMovesQuery(species.Id), CancellationToken.None);

        Assert.Equal(species.Id, result.PokemonSpeciesId);
        Assert.Equal("Blastoise", result.PokemonSpeciesName);
        Assert.Equal(2, result.Moves.Count);
        Assert.Collection(
            result.Moves,
            move => Assert.Equal("Protect", move.Name),
            move => Assert.Equal("Surf", move.Name));
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_Species_Has_No_Learnable_Moves()
    {
        var species = PokemonSpecies.Create(
            Guid.NewGuid(),
            Name.Create("Ditto"),
            PokemonTyping.Create([PokemonType.Normal]),
            BaseStats.Create(48, 48, 48, 48, 48, 48));

        var speciesRepository = Substitute.For<IPokemonSpeciesReadRepository>();
        speciesRepository.GetByIdWithLearnableMovesAsync(species.Id, Arg.Any<CancellationToken>())
            .Returns(species);

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        var handler = new GetPokemonSpeciesLearnableMovesQueryHandler(speciesRepository, moveRepository);

        var result = await handler.Handle(new GetPokemonSpeciesLearnableMovesQuery(species.Id), CancellationToken.None);

        Assert.Equal(species.Id, result.PokemonSpeciesId);
        Assert.Empty(result.Moves);
        await moveRepository.DidNotReceive().GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_Species_Does_Not_Exist()
    {
        var speciesRepository = Substitute.For<IPokemonSpeciesReadRepository>();
        speciesRepository.GetByIdWithLearnableMovesAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((PokemonSpecies?)null);

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        var handler = new GetPokemonSpeciesLearnableMovesQueryHandler(speciesRepository, moveRepository);

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new GetPokemonSpeciesLearnableMovesQuery(Guid.NewGuid()),
            CancellationToken.None));

        Assert.Equal("id", exception.Target);
        await moveRepository.DidNotReceive().GetByIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }
}
