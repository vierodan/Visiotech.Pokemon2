using System.Reflection;
using Visiotech.Pokemon.Application.Abstractions.Clock;
using Visiotech.Pokemon.Application.Abstractions.Messaging;

namespace Visiotech.Pokemon.Application.Features.System.Queries.GetSystemInfo;

public sealed class GetSystemInfoQueryHandler(IClock clock)
    : IQueryHandler<GetSystemInfoQuery, SystemInfoResponse>
{
    public Task<SystemInfoResponse> Handle(GetSystemInfoQuery query, CancellationToken cancellationToken)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        return Task.FromResult(new SystemInfoResponse(
            "Visiotech.Pokemon.Api",
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
            version,
            clock.UtcNow));
    }
}

