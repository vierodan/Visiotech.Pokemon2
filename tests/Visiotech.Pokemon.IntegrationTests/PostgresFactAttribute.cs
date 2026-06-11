using Npgsql;

namespace Visiotech.Pokemon.IntegrationTests;

public sealed class PostgresFactAttribute : FactAttribute
{
    private static readonly Lazy<DatabaseAvailability> Availability = new(ProbeAvailability, LazyThreadSafetyMode.ExecutionAndPublication);

    public PostgresFactAttribute()
    {
        var availability = Availability.Value;
        if (!availability.IsAvailable)
        {
            Skip = availability.Reason;
        }
    }

    private static DatabaseAvailability ProbeAvailability()
    {
        var templateConnectionString = CustomWebApplicationFactory.ResolveTemplateConnectionString();
        var adminConnectionString = new NpgsqlConnectionStringBuilder(templateConnectionString)
        {
            Database = "postgres",
            Timeout = 3,
            CommandTimeout = 3,
            Pooling = false
        }.ConnectionString;

        try
        {
            using var connection = new NpgsqlConnection(adminConnectionString);
            connection.Open();
            return DatabaseAvailability.Available();
        }
        catch (Exception exception) when (IsDatabaseUnavailable(exception))
        {
            return DatabaseAvailability.Unavailable(
                $"Skipping integration tests because PostgreSQL is unavailable or inaccessible using '{BuildDataSourceDescription(adminConnectionString)}'.");
        }
    }

    private static bool IsDatabaseUnavailable(Exception exception) =>
        exception is TimeoutException ||
        exception is NpgsqlException ||
        exception is OperationCanceledException ||
        exception.InnerException is not null && IsDatabaseUnavailable(exception.InnerException);

    private static string BuildDataSourceDescription(string connectionString)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        return $"{builder.Host}:{builder.Port}/{builder.Database}";
    }

    private sealed record DatabaseAvailability(bool IsAvailable, string? Reason)
    {
        public static DatabaseAvailability Available() => new(true, null);

        public static DatabaseAvailability Unavailable(string reason) => new(false, reason);
    }
}
