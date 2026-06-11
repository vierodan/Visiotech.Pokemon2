using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Application.Common.Exceptions;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Battles.Queries.GetBattleHistory;

public sealed class GetBattleHistoryQueryHandler(IBattleReadRepository repository)
    : IQueryHandler<GetBattleHistoryQuery, BattleHistoryResponse>
{
    public async Task<BattleHistoryResponse> Handle(
        GetBattleHistoryQuery query,
        CancellationToken cancellationToken)
    {
        var battle = await repository.GetByIdAsync(query.Id, cancellationToken);
        if (battle is null)
        {
            throw new ApplicationNotFoundException(
                $"Battle '{query.Id}' was not found.",
                "id");
        }

        return BattleMapping.ToHistoryResponse(battle);
    }
}
