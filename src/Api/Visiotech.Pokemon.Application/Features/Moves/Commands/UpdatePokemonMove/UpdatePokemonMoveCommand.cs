using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Moves.Commands.UpdatePokemonMove;

public sealed record UpdatePokemonMoveCommand(
    Guid Id,
    string? Name,
    string? Type,
    string? Category,
    int Power) : ICommand<PokemonMoveResponse>;
