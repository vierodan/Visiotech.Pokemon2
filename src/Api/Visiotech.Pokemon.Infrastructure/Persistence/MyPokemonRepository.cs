using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Models;
using Microsoft.EntityFrameworkCore;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Infrastructure.Persistence;

public sealed class MyPokemonRepository(PokemonDbContext dbContext) : IMyPokemonWriteRepository, IMyPokemonReadRepository
{
    public async Task AddAsync(MyPokemon myPokemon, CancellationToken cancellationToken) =>
        await dbContext.MyPokemons.AddAsync(myPokemon, cancellationToken);

    public void Remove(MyPokemon myPokemon) => dbContext.MyPokemons.Remove(myPokemon);

    public async Task<MyPokemon?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.MyPokemons
            .Include(myPokemon => myPokemon.EquippedMoves)
            .SingleOrDefaultAsync(myPokemon => myPokemon.Id == id, cancellationToken);

    public async Task<MyPokemon?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.MyPokemons
            .AsNoTracking()
            .Include(myPokemon => myPokemon.EquippedMoves)
            .SingleOrDefaultAsync(myPokemon => myPokemon.Id == id, cancellationToken);

    public async Task<MyPokemonCatalogPage> SearchAsync(MyPokemonCatalogFilter filter, CancellationToken cancellationToken)
    {
        var query = dbContext.MyPokemons
            .AsNoTracking()
            .Include(myPokemon => myPokemon.EquippedMoves);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(myPokemon => myPokemon.Id)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToArrayAsync(cancellationToken);

        return new MyPokemonCatalogPage(items, totalCount);
    }
}
