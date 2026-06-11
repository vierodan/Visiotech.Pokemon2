using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Battles.Commands.ExecuteBattlePhase;

public sealed record ExecuteBattlePhaseCommand(
    Guid BattleId,
    Guid AttackerMyPokemonId,
    Guid MoveId)
    : ICommand<BattlePhaseExecutionResponse>;
