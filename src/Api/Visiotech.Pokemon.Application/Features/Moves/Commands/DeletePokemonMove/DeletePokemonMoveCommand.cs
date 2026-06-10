using Visiotech.Pokemon.Application.Abstractions.Messaging;

namespace Visiotech.Pokemon.Application.Features.Moves.Commands.DeletePokemonMove;

public sealed record DeletePokemonMoveCommand(Guid Id) : ICommand<Guid>;
