using Visiotech.Pokemon.Application.Abstractions.Messaging;

namespace Visiotech.Pokemon.Application.Features.System.Queries.GetSystemInfo;

public sealed record GetSystemInfoQuery() : IQuery<SystemInfoResponse>;

