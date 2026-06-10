using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Infrastructure.Persistence;

public sealed class MyPokemonRepository(PokemonDbContext dbContext) : IMyPokemonWriteRepository
{
    public async Task AddAsync(MyPokemon myPokemon, CancellationToken cancellationToken) =>
        await dbContext.MyPokemons.AddAsync(myPokemon, cancellationToken);
}
