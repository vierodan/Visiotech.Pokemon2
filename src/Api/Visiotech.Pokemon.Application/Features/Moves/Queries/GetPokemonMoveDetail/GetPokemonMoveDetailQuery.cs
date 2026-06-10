using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Moves.Queries.GetPokemonMoveDetail;

public sealed record GetPokemonMoveDetailQuery(Guid Id) : IQuery<PokemonMoveResponse>;
