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

    public static PokemonSpeciesCatalogContract ToContract(this PokemonSpeciesCatalogResponse response) =>
        new(
            response.Items.Select(item => item.ToContract()).ToArray(),
            response.Page,
            response.PageSize,
            response.TotalCount,
            response.TotalPages);
}
