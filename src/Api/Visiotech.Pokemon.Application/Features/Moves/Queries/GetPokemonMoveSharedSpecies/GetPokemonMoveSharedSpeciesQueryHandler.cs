using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries;

namespace Visiotech.Pokemon.Application.Features.Moves.Queries.GetPokemonMoveSharedSpecies;

public sealed class GetPokemonMoveSharedSpeciesQueryHandler(
    IPokemonMoveReadRepository moveRepository,
    IPokemonSpeciesReadRepository speciesRepository)
    : IQueryHandler<GetPokemonMoveSharedSpeciesQuery, PokemonMoveSharedSpeciesResponse>
{
    public async Task<PokemonMoveSharedSpeciesResponse> Handle(
        GetPokemonMoveSharedSpeciesQuery query,
        CancellationToken cancellationToken)
    {
        var pokemonMove = await moveRepository.GetByIdAsync(query.Id, cancellationToken);
        if (pokemonMove is null)
        {
            throw new ApplicationNotFoundException(
                $"Pokemon move '{query.Id}' was not found.",
                "id");
        }

        var pokemonSpecies = await speciesRepository.GetByLearnableMoveIdAsync(query.Id, cancellationToken);

        return new PokemonMoveSharedSpeciesResponse(
            pokemonMove.Id,
            pokemonMove.Name.Value,
            pokemonSpecies.Select(PokemonSpeciesMapping.ToResponse).ToArray());
    }
}
