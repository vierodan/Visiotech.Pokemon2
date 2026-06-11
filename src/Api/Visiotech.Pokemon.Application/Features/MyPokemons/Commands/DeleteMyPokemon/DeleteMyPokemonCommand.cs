using Visiotech.Pokemon.Application.Abstractions.Messaging;

namespace Visiotech.Pokemon.Application.Features.MyPokemons.Commands.DeleteMyPokemon;

public sealed record DeleteMyPokemonCommand(Guid Id) : ICommand<Guid>;
