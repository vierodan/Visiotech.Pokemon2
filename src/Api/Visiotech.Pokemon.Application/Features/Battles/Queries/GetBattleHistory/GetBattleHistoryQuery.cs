using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Battles.Queries.GetBattleHistory;

public sealed record GetBattleHistoryQuery(Guid Id) : IQuery<BattleHistoryResponse>;
