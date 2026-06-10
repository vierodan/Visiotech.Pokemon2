using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.MyPokemons.Queries.GetMyPokemonDetail;

public sealed record GetMyPokemonDetailQuery(Guid Id) : IQuery<MyPokemonResponse>;
