using Microsoft.EntityFrameworkCore;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Infrastructure.Persistence;

public sealed class PokemonMoveRepository(PokemonDbContext dbContext)
    : IPokemonMoveReadRepository, IPokemonMoveWriteRepository
{
    public async Task AddAsync(PokemonMove pokemonMove, CancellationToken cancellationToken) =>
        await dbContext.PokemonMoves.AddAsync(pokemonMove, cancellationToken);

    public void Remove(PokemonMove pokemonMove) => dbContext.PokemonMoves.Remove(pokemonMove);

    public Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken) =>
        dbContext.PokemonMoves.AnyAsync(
            pokemonMove => pokemonMove.NormalizedName == normalizedName,
            cancellationToken);

    public Task<bool> ExistsByNormalizedNameAsync(string normalizedName, Guid excludedId, CancellationToken cancellationToken) =>
        dbContext.PokemonMoves.AnyAsync(
            pokemonMove => pokemonMove.NormalizedName == normalizedName && pokemonMove.Id != excludedId,
            cancellationToken);

    public async Task<PokemonMove?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.PokemonMoves
            .SingleOrDefaultAsync(pokemonMove => pokemonMove.Id == id, cancellationToken);

    public async Task<PokemonMove?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.PokemonMoves
            .AsNoTracking()
            .SingleOrDefaultAsync(pokemonMove => pokemonMove.Id == id, cancellationToken);

    public async Task<PokemonMoveCatalogPage> SearchAsync(
        PokemonMoveCatalogFilter filter,
        CancellationToken cancellationToken)
    {
        var query = dbContext.PokemonMoves.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.NormalizedName))
        {
            var namePattern = $"%{filter.NormalizedName}%";
            query = query.Where(pokemonMove =>
                EF.Functions.Like(pokemonMove.NormalizedName, namePattern));
        }

        if (filter.Type is not null)
        {
            var type = filter.Type.Value;
            query = query.Where(pokemonMove => pokemonMove.Type == type);
        }

        if (filter.Category is not null)
        {
            var category = filter.Category.Value;
            query = query.Where(pokemonMove => pokemonMove.Category == category);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(pokemonMove => pokemonMove.Name.Value)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToArrayAsync(cancellationToken);

        return new PokemonMoveCatalogPage(items, totalCount);
    }
}
