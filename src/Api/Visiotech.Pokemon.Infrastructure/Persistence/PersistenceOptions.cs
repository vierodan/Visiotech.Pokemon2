namespace Visiotech.Pokemon.Infrastructure.Persistence;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public string? Provider { get; set; }

    public string? InMemoryDatabaseName { get; set; }
}
