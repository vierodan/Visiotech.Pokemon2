using Visiotech.Pokemon.Application.Common.Models;
using Visiotech.Pokemon.Application.Features.Moves.Commands.CreatePokemonMove;
using Visiotech.Pokemon.Application.Features.Moves.Queries.GetPokemonMoveDetail;
using Visiotech.Pokemon.Application.Features.Moves.Queries.GetPokemonMovesCatalog;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.CreatePokemonSpecies;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.DeletePokemonSpecies;
using Visiotech.Pokemon.Application.Features.Pokemons.Commands.UpdatePokemonSpecies;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonSpeciesDetail;
using Visiotech.Pokemon.Application.Features.Pokemons.Queries.GetPokemonsCatalog;
using Visiotech.Pokemon.Application.Features.System.Queries.GetSystemInfo;

namespace Visiotech.Pokemon.Application.Common.Observability;

internal static class RequestLogContextFactory
{
    public static object CreateRequestPayload<TRequest>(TRequest request) =>
        request switch
        {
            CreatePokemonMoveCommand command => new
            {
                command.Name,
                command.Type,
                command.Category,
                command.Power
            },
            GetPokemonMoveDetailQuery query => new
            {
                query.Id
            },
            GetPokemonMovesCatalogQuery query => new
            {
                query.Name,
                query.Type,
                query.Category,
                query.Page,
                query.PageSize
            },
            CreatePokemonSpeciesCommand command => new
            {
                command.Name,
                command.Types,
                BaseStats = new
                {
                    command.Health,
                    command.Attack,
                    command.Defense,
                    command.SpecialAttack,
                    command.SpecialDefense,
                    command.Speed
                }
            },
            UpdatePokemonSpeciesCommand command => new
            {
                command.Id,
                command.Name,
                command.Types,
                BaseStats = new
                {
                    command.Health,
                    command.Attack,
                    command.Defense,
                    command.SpecialAttack,
                    command.SpecialDefense,
                    command.Speed
                }
            },
            DeletePokemonSpeciesCommand command => new
            {
                command.Id
            },
            GetPokemonSpeciesDetailQuery query => new
            {
                query.Id
            },
            GetPokemonsCatalogQuery query => new
            {
                query.Name,
                query.Type,
                query.Page,
                query.PageSize
            },
            GetSystemInfoQuery => new
            {
                Request = nameof(GetSystemInfoQuery)
            },
            null => new
            {
                Request = "null"
            },
            _ => new
            {
                RequestType = typeof(TRequest).Name
            }
        };

    public static object CreateResponseSummary<TResponse>(TResponse response) =>
        response switch
        {
            PokemonMoveResponse move => new
            {
                move.Id,
                move.Name,
                move.Type,
                move.Category,
                move.Power
            },
            PokemonMoveCatalogResponse catalog => new
            {
                catalog.Page,
                catalog.PageSize,
                catalog.TotalCount,
                ItemCount = catalog.Items.Count
            },
            PokemonSpeciesResponse species => new
            {
                species.Id,
                species.Name,
                TypeCount = species.Types.Count
            },
            PokemonSpeciesCatalogResponse catalog => new
            {
                catalog.Page,
                catalog.PageSize,
                catalog.TotalCount,
                ItemCount = catalog.Items.Count
            },
            SystemInfoResponse systemInfo => new
            {
                systemInfo.Service,
                systemInfo.Environment,
                systemInfo.GeneratedAtUtc
            },
            Guid id => new
            {
                Id = id
            },
            null => new
            {
                Response = "null"
            },
            _ => new
            {
                ResponseType = typeof(TResponse).Name
            }
        };
}
