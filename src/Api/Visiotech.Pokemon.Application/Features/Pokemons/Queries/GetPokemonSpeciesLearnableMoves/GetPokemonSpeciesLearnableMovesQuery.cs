using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonSpeciesLearnableMoves;

public sealed record GetPokemonSpeciesLearnableMovesQuery(Guid Id) : IQuery<PokemonLearnableMovesResponse>;
