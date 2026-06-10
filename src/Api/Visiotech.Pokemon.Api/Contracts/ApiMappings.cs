using Visiotech.Pokemon.Application.Features.System.Queries.GetSystemInfo;
using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Contracts;

namespace Visiotech.Pokemon.Api.Contracts;

public static class ApiMappings
{
    public static SystemInfoContract ToContract(this SystemInfoResponse response) =>
        new(response.Service, response.Environment, response.Version, response.GeneratedAtUtc);

    public static PokemonSpeciesContract ToContract(this PokemonSpeciesResponse response) =>
        new(
            response.Id,
            response.Name,
            response.Types,
            new PokemonBaseStatsContract(
                response.BaseStats.Health,
                response.BaseStats.Attack,
                response.BaseStats.Defense,
                response.BaseStats.SpecialAttack,
                response.BaseStats.SpecialDefense,
                response.BaseStats.Speed));

    public static PokemonMoveContract ToContract(this PokemonMoveResponse response) =>
        new(
            response.Id,
            response.Name,
            response.Type,
            response.Category,
            response.Power);

    public static PokemonLearnableMovesContract ToContract(this PokemonLearnableMovesResponse response) =>
        new(
            response.PokemonSpeciesId,
            response.PokemonSpeciesName,
            response.Moves.Select(item => item.ToContract()).ToArray());

    public static PokemonMoveSharedSpeciesContract ToContract(this PokemonMoveSharedSpeciesResponse response) =>
        new(
            response.PokemonMoveId,
            response.PokemonMoveName,
            response.PokemonSpecies.Select(item => item.ToContract()).ToArray());

    public static PokemonMoveCatalogContract ToContract(this PokemonMoveCatalogResponse response) =>
        new(
            response.Items.Select(item => item.ToContract()).ToArray(),
            response.Page,
            response.PageSize,
            response.TotalCount,
            response.TotalPages);

    public static PokemonSpeciesCatalogContract ToContract(this PokemonSpeciesCatalogResponse response) =>
        new(
            response.Items.Select(item => item.ToContract()).ToArray(),
            response.Page,
            response.PageSize,
            response.TotalCount,
            response.TotalPages);
}
