using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.MyPokemons.Queries.GetMyPokemonEquippedMoves;

public sealed record GetMyPokemonEquippedMovesQuery(Guid Id) : IQuery<MyPokemonEquippedMovesResponse>;
