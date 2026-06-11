using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.MyPokemons.Commands.UpdateMyPokemon;

public sealed record UpdateMyPokemonCommand(
    Guid Id,
    int Level,
    int CurrentHealthPoints,
    int TotalHealthPoints,
    IReadOnlyCollection<Guid>? EquippedMoveIds) : ICommand<MyPokemonResponse>;
