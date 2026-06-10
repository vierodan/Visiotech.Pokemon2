using Visiotech.Pokemon.Application.Features.System.Queries.GetSystemInfo;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Contracts;

namespace Visiotech.Pokemon.Api.Contracts;

public static class ApiMappings
{
    public static SystemInfoContract ToContract(this SystemInfoResponse response) =>
        new(response.Service, response.Environment, response.Version, response.GeneratedAtUtc);

    public static PokemonContract ToContract(this PokemonResponse response) =>
        new(
            response.Id,
            response.Name,
            response.Type,
            response.Level,
            response.Health,
            response.Attack,
            response.Defense,
            response.SpecialAttack,
            response.SpecialDefense,
            response.Speed,
            response.Moves
                .Select(move => new PokemonMoveContract(move.Name, move.Type, move.Power))
                .ToArray(),
            response.Abilities
                .Select(ability => new PokemonAbilityContract(ability.Name))
                .ToArray());
}
