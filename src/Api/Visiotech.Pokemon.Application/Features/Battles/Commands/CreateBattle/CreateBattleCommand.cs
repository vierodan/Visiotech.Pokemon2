using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Battles.Commands.CreateBattle;

public sealed record CreateBattleCommand(
    Guid FirstMyPokemonId,
    Guid SecondMyPokemonId)
    : ICommand<BattleResponse>;
