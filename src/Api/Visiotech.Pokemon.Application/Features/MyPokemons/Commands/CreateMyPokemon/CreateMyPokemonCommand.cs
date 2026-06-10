using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.MyPokemons.Commands.CreateMyPokemon;

public sealed record CreateMyPokemonCommand(
    Guid PokemonSpeciesId,
    int Level,
    int CurrentHealthPoints,
    int TotalHealthPoints,
    IReadOnlyCollection<Guid>? EquippedMoveIds) : ICommand<MyPokemonResponse>;
