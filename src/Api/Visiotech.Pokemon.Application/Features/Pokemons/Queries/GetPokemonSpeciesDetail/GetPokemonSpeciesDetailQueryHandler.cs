using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonSpeciesDetail;

public sealed class GetPokemonSpeciesDetailQueryHandler(IPokemonSpeciesReadRepository repository)
    : IQueryHandler<GetPokemonSpeciesDetailQuery, PokemonSpeciesResponse>
{
    public async Task<PokemonSpeciesResponse> Handle(
        GetPokemonSpeciesDetailQuery query,
        CancellationToken cancellationToken)
    {
        var pokemonSpecies = await repository.GetByIdAsync(query.Id, cancellationToken);
        if (pokemonSpecies is null)
        {
            throw new ApplicationNotFoundException(
                $"Pokemon species '{query.Id}' was not found.",
                "id");
        }

        return PokemonSpeciesMapping.ToResponse(pokemonSpecies);
    }
}
