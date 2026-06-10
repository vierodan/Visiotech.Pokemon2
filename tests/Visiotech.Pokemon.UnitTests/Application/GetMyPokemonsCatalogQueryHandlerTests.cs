using NSubstitute;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.MyPokemons.Queries.GetMyPokemonsCatalog;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.UnitTests.Application;

public sealed class GetMyPokemonsCatalogQueryHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Paginated_MyPokemon_Catalog()
    {
        var charizard = CreateSpecies("Charizard", [PokemonType.Fire, PokemonType.Flying], 78, 84, 78, 109, 85, 100);
        var blastoise = CreateSpecies("Blastoise", [PokemonType.Water], 79, 83, 100, 85, 105, 78);
        var flamethrower = CreateMove("Flamethrower", PokemonType.Fire, MoveCategory.Special, 90);
        var fly = CreateMove("Fly", PokemonType.Flying, MoveCategory.Physical, 90);
        var surf = CreateMove("Surf", PokemonType.Water, MoveCategory.Special, 90);
        var protect = CreateMove("Protect", PokemonType.Normal, MoveCategory.Status, 0);

        var firstMyPokemon = MyPokemon.Create(
            Guid.NewGuid(),
            charizard.Id,
            Level.Create(50),
            120,
            150,
            [flamethrower.Id, fly.Id]);
        var secondMyPokemon = MyPokemon.Create(
            Guid.NewGuid(),
            blastoise.Id,
            Level.Create(45),
            110,
            140,
            [surf.Id, protect.Id]);

        var repository = Substitute.For<IMyPokemonReadRepository>();
        repository.SearchAsync(
                Arg.Is<MyPokemonCatalogFilter>(filter => filter.Page == 2 && filter.PageSize == 1),
                Arg.Any<CancellationToken>())
            .Returns(new MyPokemonCatalogPage([firstMyPokemon, secondMyPokemon], 2));

        var speciesRepository = Substitute.For<IPokemonSpeciesReadRepository>();
        speciesRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids =>
                    ids.Count == 2 &&
                    ids.Contains(charizard.Id) &&
                    ids.Contains(blastoise.Id)),
                Arg.Any<CancellationToken>())
            .Returns([charizard, blastoise]);

        var moveRepository = Substitute.For<IPokemonMoveReadRepository>();
        moveRepository.GetByIdsAsync(
                Arg.Is<IReadOnlyCollection<Guid>>(ids =>
                    ids.Count == 4 &&
                    ids.Contains(flamethrower.Id) &&
                    ids.Contains(fly.Id) &&
                    ids.Contains(surf.Id) &&
                    ids.Contains(protect.Id)),
                Arg.Any<CancellationToken>())
            .Returns([flamethrower, fly, surf, protect]);

        var handler = new GetMyPokemonsCatalogQueryHandler(repository, speciesRepository, moveRepository);

        var result = await handler.Handle(new GetMyPokemonsCatalogQuery(2, 1), CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(2, result.TotalPages);
        Assert.Collection(
            result.Items,
            first =>
            {
                Assert.Equal(firstMyPokemon.Id, first.Id);
                Assert.Equal("Charizard", first.Species.Name);
                Assert.Equal(2, first.EquippedMoves.Count);
            },
            second =>
            {
                Assert.Equal(secondMyPokemon.Id, second.Id);
                Assert.Equal("Blastoise", second.Species.Name);
                Assert.Equal(2, second.EquippedMoves.Count);
            });
    }

    [Fact]
    public async Task Handle_Should_Reject_Invalid_Pagination()
    {
        var handler = new GetMyPokemonsCatalogQueryHandler(
            Substitute.For<IMyPokemonReadRepository>(),
            Substitute.For<IPokemonSpeciesReadRepository>(),
            Substitute.For<IPokemonMoveReadRepository>());

        var exception = await Assert.ThrowsAsync<ApplicationValidationException>(() => handler.Handle(
            new GetMyPokemonsCatalogQuery(0, 101),
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

    private static PokemonMove CreateMove(string name, PokemonType type, MoveCategory category, int power) =>
        PokemonMove.Create(Guid.NewGuid(), Move.Create(name, type, category, power));
}
