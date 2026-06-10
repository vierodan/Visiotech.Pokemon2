using Visiotech.Pokemon.Application.Abstractions.Messaging;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Commands.DeletePokemonSpecies;

public sealed record DeletePokemonSpeciesCommand(Guid Id) : ICommand<Guid>;
