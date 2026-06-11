using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Services;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Damage;

namespace Visiotech.Pokemon.Application.Features.Damage.Queries.CalculateMoveDamage;

public sealed class CalculateMoveDamageQueryHandler(IMoveDamageCalculationService moveDamageCalculationService)
    : IQueryHandler<CalculateMoveDamageQuery, MoveDamageCalculationResponse>
{
    public async Task<MoveDamageCalculationResponse> Handle(
        CalculateMoveDamageQuery query,
        CancellationToken cancellationToken) =>
        await moveDamageCalculationService.CalculateAsync(
            new MoveDamageCalculationRequest(
                query.AttackerMyPokemonId,
                query.DefenderMyPokemonId,
                query.MoveId),
            cancellationToken);
}
