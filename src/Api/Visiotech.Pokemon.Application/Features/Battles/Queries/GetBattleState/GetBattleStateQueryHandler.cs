using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Battles;

namespace Visiotech.Pokemon.Application.Features.Battles.Queries.GetBattleState;

public sealed class GetBattleStateQueryHandler(IBattleReadRepository repository)
    : IQueryHandler<GetBattleStateQuery, BattleResponse>
{
    public async Task<BattleResponse> Handle(
        GetBattleStateQuery query,
        CancellationToken cancellationToken)
    {
        var battle = await repository.GetByIdAsync(query.Id, cancellationToken);
        if (battle is null)
        {
            throw new ApplicationNotFoundException(
                $"Battle '{query.Id}' was not found.",
                "id");
        }

        return BattleMapping.ToResponse(battle);
    }
}
