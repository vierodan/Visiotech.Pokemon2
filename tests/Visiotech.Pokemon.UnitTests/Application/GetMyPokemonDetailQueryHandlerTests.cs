using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Features.MyPokemons.Queries.GetMyPokemonDetail;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class GetMyPokemonDetailQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_MyPokemon_Detail_When_It_Exists()
    {
        var species = CreateSpecies("Charizard", [PokemonType.Fire, PokemonType.Flying], 78, 84, 78, 109, 85, 100);
        var flamethrower = CreateMove("Flamethrower", PokemonType.Fire, MoveCategory.Special, 90);
        var fly = CreateMove("Fly", PokemonType.Flying, MoveCategory.Physical, 90);
        var myPokemon = MyPokemon.Create(
            Guid.NewGuid(),
            species.Id,
            Level.Create(50),
            120,
            150,
            [flamethrower.Id, fly.Id]);

        var repository = Substitute.For<IMyPokemonReadRepository>();
        repository.GetByIdAsync(myPokemon.Id, Arg.Any<CancellationToken>()).Returns(myPokemon);

        var speciesRepository = Substitute.For<IPokemonSpeciesReadRepository>();
        speciesRepository.GetByIdAsync(species.Id, Arg.Any<CancellationToken>()).Returns(species);

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        moveRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 2 && ids.Contains(flamethrower.Id) && ids.Contains(fly.Id)),
                Arg.Any<CancellationToken>())
            .Returns([flamethrower, fly]);

        var handler = new GetMyPokemonDetailQueryHandler(repository, speciesRepository, moveRepository);

        var result = await handler.Handle(new GetMyPokemonDetailQuery(myPokemon.Id), CancellationToken.None);

        Assert.Equal(myPokemon.Id, result.Id);
        Assert.Equal("Charizard", result.Species.Name);
        Assert.Equal(["Fire", "Flying"], result.Species.Types);
        Assert.Equal(50, result.Level);
        Assert.Equal(120, result.CurrentHealthPoints);
        Assert.Equal(150, result.TotalHealthPoints);
        Assert.Collection(
            result.EquippedMoves,
            move => Assert.Equal("Flamethrower", move.Name),
            move => Assert.Equal("Fly", move.Name));
    }

    [Fact]
    public async Task Handle_Should_Throw_NotFound_When_MyPokemon_Does_Not_Exist()
    {
        var repository = Substitute.For<IMyPokemonReadRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((MyPokemon?)null);

        var handler = new GetMyPokemonDetailQueryHandler(
            repository,
            Substitute.For<IPokemonSpeciesReadRepository>(),
            Substitute.For<IPokemonMoveReadRepository>());

        var exception = await Assert.ThrowsAsync<ApplicationNotFoundException>(() => handler.Handle(
            new GetMyPokemonDetailQuery(Guid.NewGuid()),
            CancellationToken.None));

        Assert.Equal("id", exception.Target);
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

    private static PokemonMove CreateMove(string name, PokemonType type, MoveCategory category, int power) =>
        PokemonMove.Create(Guid.NewGuid(), Move.Create(name, type, category, power));
}
