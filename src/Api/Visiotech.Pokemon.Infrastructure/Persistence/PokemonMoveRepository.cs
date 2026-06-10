using Microsoft.EntityFrameworkCore;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Infrastructure.Persistence;

public sealed class PokemonMoveRepository(PokemonDbContext dbContext) : IPokemonMoveWriteRepository
{
    public async Task AddAsync(PokemonMove pokemonMove, CancellationToken cancellationToken) =>
        await dbContext.PokemonMoves.AddAsync(pokemonMove, cancellationToken);

    public Task<bool> ExistsByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken) =>
        dbContext.PokemonMoves.AnyAsync(
            pokemonMove => pokemonMove.NormalizedName == normalizedName,
            cancellationToken);
}
