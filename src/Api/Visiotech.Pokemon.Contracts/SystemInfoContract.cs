namespace Visiotech.Pokemon.Contracts;

public sealed record SystemInfoContract(
    string Service,
    string Environment,
    string Version,
    DateTimeOffset GeneratedAtUtc);
