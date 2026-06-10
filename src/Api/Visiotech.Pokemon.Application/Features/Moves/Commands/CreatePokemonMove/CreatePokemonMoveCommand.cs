using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Moves.Commands.CreatePokemonMove;

public sealed record CreatePokemonMoveCommand(
    string? Name,
    string? Type,
    string? Category,
    int Power) : ICommand<PokemonMoveResponse>;
