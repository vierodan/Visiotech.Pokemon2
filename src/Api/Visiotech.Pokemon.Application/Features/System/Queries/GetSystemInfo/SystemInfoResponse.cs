namespace Visiotech.Pokemon.Application.Features.System.Queries.GetSystemInfo;

public sealed record SystemInfoResponse(
    string Service,
    string Environment,
    string Version,
    DateTimeOffset GeneratedAtUtc);

