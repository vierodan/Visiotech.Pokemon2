using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Damage.Queries.CalculateMoveDamage;

public sealed record CalculateMoveDamageQuery(
    Guid AttackerMyPokemonId,
    Guid DefenderMyPokemonId,
    Guid MoveId)
    : IQuery<MoveDamageCalculationResponse>;
