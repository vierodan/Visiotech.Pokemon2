using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Visiotech.Pokemon.Api.Contracts;
using Visiotech.Pokemon.Application.Abstractions.Messaging;
using Visiotech.Pokemon.Application.Features.System.Queries.GetSystemInfo;
using Visiotech.Pokemon.Contracts;

namespace Visiotech.Pokemon.Api;

public static class SystemEndpoints
{
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/v1/system", GetSystemInfoAsync)
            .WithName("GetSystemInfo")
            .WithSummary("Returns technical metadata about the API host.")
            .Produces<SystemInfoContract>(StatusCodes.Status200OK);

        return endpoints;
    }

    private static async Task<Ok<SystemInfoContract>> GetSystemInfoAsync(
        IQueryHandler<GetSystemInfoQuery, SystemInfoResponse> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new GetSystemInfoQuery(), cancellationToken);
        return TypedResults.Ok(response.ToContract());
    }
}
