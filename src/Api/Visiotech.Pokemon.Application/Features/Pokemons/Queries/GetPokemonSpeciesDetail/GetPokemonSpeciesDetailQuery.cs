using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonSpeciesDetail;

public sealed record GetPokemonSpeciesDetailQuery(Guid Id) : IQuery<PokemonSpeciesResponse>;
