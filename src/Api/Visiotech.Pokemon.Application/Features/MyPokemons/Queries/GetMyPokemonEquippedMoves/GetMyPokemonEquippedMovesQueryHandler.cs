using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Moves.Queries;

namespace Visiotech.Pokemon.Application.Features.MyPokemons.Queries.GetMyPokemonEquippedMoves;

public sealed class GetMyPokemonEquippedMovesQueryHandler(
    IMyPokemonReadRepository repository,
    IPokemonMoveReadRepository moveRepository)
    : IQueryHandler<GetMyPokemonEquippedMovesQuery, MyPokemonEquippedMovesResponse>
{
    public async Task<MyPokemonEquippedMovesResponse> Handle(
        GetMyPokemonEquippedMovesQuery query,
        CancellationToken cancellationToken)
    {
        var myPokemon = await repository.GetByIdAsync(query.Id, cancellationToken);
        if (myPokemon is null)
        {
            throw new ApplicationNotFoundException(
                $"My pokemon '{query.Id}' was not found.",
                "id");
        }

        var equippedMoveIds = myPokemon.EquippedMoveIds;
        if (equippedMoveIds.Count == 0)
        {
            return new MyPokemonEquippedMovesResponse(myPokemon.Id, []);
        }

        var equippedMoves = await moveRepository.GetByIdsAsync(equippedMoveIds, cancellationToken);
        var movesById = equippedMoves.ToDictionary(move => move.Id);
        var orderedMoves = equippedMoveIds
            .Where(movesById.ContainsKey)
            .Select(moveId => movesById[moveId])
            .Select(PokemonMoveMapping.ToResponse)
            .ToArray();

        return new MyPokemonEquippedMovesResponse(myPokemon.Id, orderedMoves);
    }
}
