using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Moves.Queries;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonSpeciesLearnableMoves;

public sealed class GetPokemonSpeciesLearnableMovesQueryHandler(
    IPokemonSpeciesReadRepository speciesRepository,
    IPokemonMoveReadRepository moveRepository)
    : IQueryHandler<GetPokemonSpeciesLearnableMovesQuery, PokemonLearnableMovesResponse>
{
    public async Task<PokemonLearnableMovesResponse> Handle(
        GetPokemonSpeciesLearnableMovesQuery query,
        CancellationToken cancellationToken)
    {
        var pokemonSpecies = await speciesRepository.GetByIdWithLearnableMovesAsync(query.Id, cancellationToken);
        if (pokemonSpecies is null)
        {
            throw new ApplicationNotFoundException(
                $"Pokemon species '{query.Id}' was not found.",
                "id");
        }

        var learnableMoveIds = pokemonSpecies.LearnableMoveIds;
        if (learnableMoveIds.Count == 0)
        {
            return new PokemonLearnableMovesResponse(
                pokemonSpecies.Id,
                pokemonSpecies.Name.Value,
                []);
        }

        var learnableMoves = await moveRepository.GetByIdsAsync(learnableMoveIds, cancellationToken);

        var movesById = learnableMoves.ToDictionary(move => move.Id);
        var orderedMoves = learnableMoveIds
            .Where(movesById.ContainsKey)
            .Select(moveId => movesById[moveId])
            .OrderBy(move => move.Name.Value, StringComparer.Ordinal)
            .Select(PokemonMoveMapping.ToResponse)
            .ToArray();

        return new PokemonLearnableMovesResponse(
            pokemonSpecies.Id,
            pokemonSpecies.Name.Value,
            orderedMoves);
    }
}
