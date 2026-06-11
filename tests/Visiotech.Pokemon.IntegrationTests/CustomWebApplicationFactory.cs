using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Visiotech.Pokemon.Infrastructure.Persistence;

namespace Visiotech.Pokemon.IntegrationTests;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName;
    private readonly string _databaseConnectionString;
    private readonly string _adminConnectionString;

    public CustomWebApplicationFactory()
    {
        var templateConnectionString = ResolveTemplateConnectionString();
        var templateBuilder = new NpgsqlConnectionStringBuilder(templateConnectionString);

        var baseDatabaseName = string.IsNullOrWhiteSpace(templateBuilder.Database)
            ? "Pokemon2"
            : templateBuilder.Database;

        _databaseName = $"{baseDatabaseName}_integration_tests";

        _databaseConnectionString = new NpgsqlConnectionStringBuilder(templateBuilder.ConnectionString)
        {
            Database = _databaseName
        }.ConnectionString;

        _adminConnectionString = new NpgsqlConnectionStringBuilder(templateBuilder.ConnectionString)
        {
            Database = "postgres"
        }.ConnectionString;
    }

    public async Task ResetDatabaseAsync()
    {
        await EnsureDatabaseExistsAsync();
        await ResetPokemonSchemaAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PokemonDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Pokemon2Db"] = _databaseConnectionString,
                ["Seed:ApplyMvpRoster"] = "false"
            });
        });
    }

    private async Task EnsureDatabaseExistsAsync()
    {
        await using var connection = new NpgsqlConnection(_adminConnectionString);
        await connection.OpenAsync();

        await using var existsCommand = connection.CreateCommand();
        existsCommand.CommandText = "SELECT 1 FROM pg_database WHERE datname = @databaseName LIMIT 1;";
        existsCommand.Parameters.AddWithValue("databaseName", _databaseName);

        var exists = await existsCommand.ExecuteScalarAsync() is not null;
        if (exists)
        {
            return;
        }

        await using var createCommand = connection.CreateCommand();
        createCommand.CommandText = $"CREATE DATABASE {QuoteIdentifier(_databaseName)};";
        await createCommand.ExecuteNonQueryAsync();
    }

    private async Task ResetPokemonSchemaAsync()
    {
        await using var connection = new NpgsqlConnection(_databaseConnectionString);
        await connection.OpenAsync();

        await using var resetCommand = connection.CreateCommand();
        resetCommand.CommandText = """
            DROP SCHEMA IF EXISTS pokemon2 CASCADE;
            DROP TABLE IF EXISTS public."__EFMigrationsHistory";
            CREATE SCHEMA pokemon2;
            """;

        await resetCommand.ExecuteNonQueryAsync();
    }

    internal static string ResolveTemplateConnectionString()
    {
        var explicitConnectionString = Environment.GetEnvironmentVariable("IntegrationTests__Pokemon2Db")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__Pokemon2Db");

        if (!string.IsNullOrWhiteSpace(explicitConnectionString))
        {
            return explicitConnectionString;
        }

        var host = GetEnvironmentVariableOrDotEnv("POSTGRES_HOST", "localhost");
        var port = GetEnvironmentVariableOrDotEnv("POSTGRES_PORT", "5432");
        var database = GetEnvironmentVariableOrDotEnv("POSTGRES_DATABASE", "Pokemon2");
        var username = GetEnvironmentVariableOrDotEnv("POSTGRES_USERNAME", "postgres");
        var password = GetEnvironmentVariableOrDotEnv("POSTGRES_PASSWORD", "password");
        var pooling = GetEnvironmentVariableOrDotEnv("POSTGRES_POOLING", "true");
        var minPoolSize = GetEnvironmentVariableOrDotEnv("POSTGRES_MIN_POOL_SIZE", "1");
        var maxPoolSize = GetEnvironmentVariableOrDotEnv("POSTGRES_MAX_POOL_SIZE", "20");

        return
            $"Host={host};Port={port};Database={database};Username={username};Password={password};Pooling={pooling};MinPoolSize={minPoolSize};MaxPoolSize={maxPoolSize}";
    }

    private static string GetEnvironmentVariableOrDotEnv(string key, string fallbackValue)
    {
        var environmentValue = Environment.GetEnvironmentVariable(key);
        if (!string.IsNullOrWhiteSpace(environmentValue))
        {
            return environmentValue;
        }

        var dotEnvPath = FindFileInAncestors(AppContext.BaseDirectory, ".env");
        if (dotEnvPath is null)
        {
            return fallbackValue;
        }

        foreach (var line in File.ReadLines(dotEnvPath))
        {
            var trimmedLine = line.Trim();
            if (string.IsNullOrWhiteSpace(trimmedLine) ||
                trimmedLine.StartsWith('#') ||
                !trimmedLine.Contains('=', StringComparison.Ordinal))
            {
                continue;
            }

            var separatorIndex = trimmedLine.IndexOf('=');
            var candidateKey = trimmedLine[..separatorIndex].Trim();
            if (!string.Equals(candidateKey, key, StringComparison.Ordinal))
            {
                continue;
            }

            var rawValue = trimmedLine[(separatorIndex + 1)..].Trim();
            return rawValue.Trim('"');
        }

        return fallbackValue;
    }

    private static string? FindFileInAncestors(string startingDirectory, string fileName)
    {
        var directory = new DirectoryInfo(startingDirectory);

        while (directory is not null)
        {
            var candidatePath = Path.Combine(directory.FullName, fileName);
            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static string QuoteIdentifier(string identifier) =>
        $"\"{identifier.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
