using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Commands.UpdatePokemonSpeciesLearnableMoves;

public sealed record UpdatePokemonSpeciesLearnableMovesCommand(
    Guid PokemonSpeciesId,
    IReadOnlyCollection<Guid>? AddMoveIds,
    IReadOnlyCollection<Guid>? RemoveMoveIds)
    : ICommand<PokemonLearnableMovesResponse>;
