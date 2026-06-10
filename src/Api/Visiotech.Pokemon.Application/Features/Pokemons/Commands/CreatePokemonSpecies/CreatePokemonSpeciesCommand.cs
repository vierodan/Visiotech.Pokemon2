using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Common.Models;

namespace Visiotech.Pokemon.Application.Features.Pokemons.Commands.CreatePokemonSpecies;

public sealed record CreatePokemonSpeciesCommand(
    string? Name,
    IReadOnlyCollection<string>? Types,
    int Health,
    int Attack,
    int Defense,
    int SpecialAttack,
    int SpecialDefense,
    int Speed) : ICommand<PokemonSpeciesResponse>;
