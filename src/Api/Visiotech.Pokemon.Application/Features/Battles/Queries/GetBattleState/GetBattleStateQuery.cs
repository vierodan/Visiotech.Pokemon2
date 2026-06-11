using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Battles.Queries.GetBattleState;

public sealed record GetBattleStateQuery(Guid Id) : IQuery<BattleResponse>;
