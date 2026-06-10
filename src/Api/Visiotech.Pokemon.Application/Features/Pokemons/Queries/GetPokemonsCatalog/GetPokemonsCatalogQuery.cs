using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonsCatalog;

public sealed record GetPokemonsCatalogQuery() : IQuery<IReadOnlyCollection<PokemonSpeciesResponse>>;
