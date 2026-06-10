using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Moves.Queries.GetPokemonMoveDetail;

public sealed class GetPokemonMoveDetailQueryHandler(IPokemonMoveReadRepository repository)
    : IQueryHandler<GetPokemonMoveDetailQuery, PokemonMoveResponse>
{
    public async Task<PokemonMoveResponse> Handle(
        GetPokemonMoveDetailQuery query,
        CancellationToken cancellationToken)
    {
        var pokemonMove = await repository.GetByIdAsync(query.Id, cancellationToken);
        if (pokemonMove is null)
        {
            throw new ApplicationNotFoundException(
                $"Pokemon move '{query.Id}' was not found.",
                "id");
        }

        return PokemonMoveMapping.ToResponse(pokemonMove);
    }
}
