using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Infrastructure.Persistence;

public sealed class PokemonMoveDeletionDependencyChecker(PokemonDbContext dbContext)
    : IPokemonMoveDeletionDependencyChecker
{
    public async Task<IReadOnlyCollection<string>> GetBlockingReasonsAsync(
        Guid pokemonMoveId,
        CancellationToken cancellationToken)
    {
        var pokemonMoveEntityType = dbContext.Model.FindEntityType(typeof(PokemonMove))
            ?? throw new InvalidOperationException("Pokemon move entity type is not mapped.");

        var foreignKeys = dbContext.Model
            .GetEntityTypes()
            .SelectMany(static entityType => entityType.GetForeignKeys())
            .Where(foreignKey => foreignKey.PrincipalEntityType == pokemonMoveEntityType && !foreignKey.IsOwnership)
            .ToArray();

        if (foreignKeys.Length == 0)
        {
            return [];
        }

        var connection = dbContext.Database.GetDbConnection();
        var shouldCloseConnection = connection.State != ConnectionState.Open;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            var blockingReasons = new List<string>();

            foreach (var foreignKey in foreignKeys)
            {
                if (!TryGetStoreMapping(foreignKey, out var tableName, out var schema, out var columnName))
                {
                    blockingReasons.Add(
                        $"Pokemon move cannot be deleted because relation '{foreignKey.DeclaringEntityType.DisplayName()}' cannot be validated automatically.");
                    continue;
                }

                if (await HasReferenceAsync(connection, tableName, schema, columnName, pokemonMoveId, cancellationToken))
                {
                    blockingReasons.Add(
                        $"Pokemon move cannot be deleted because it is referenced by '{foreignKey.DeclaringEntityType.DisplayName()}'.");
                }
            }

            return blockingReasons;
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }

    private bool TryGetStoreMapping(
        IForeignKey foreignKey,
        out string tableName,
        out string? schema,
        out string columnName)
    {
        tableName = string.Empty;
        schema = null;
        columnName = string.Empty;

        if (foreignKey.Properties.Count != 1 || foreignKey.Properties[0].ClrType != typeof(Guid))
        {
            return false;
        }

        tableName = foreignKey.DeclaringEntityType.GetTableName() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(tableName))
        {
            return false;
        }

        var storeSchema = foreignKey.DeclaringEntityType.GetSchema();
        schema = storeSchema;

        var storeObjectIdentifier = StoreObjectIdentifier.Table(tableName, storeSchema);
        columnName = foreignKey.Properties[0].GetColumnName(storeObjectIdentifier) ?? string.Empty;

        return !string.IsNullOrWhiteSpace(columnName);
    }

    private async Task<bool> HasReferenceAsync(
        System.Data.Common.DbConnection connection,
        string tableName,
        string? schema,
        string columnName,
        Guid pokemonMoveId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            $"SELECT 1 FROM {QualifyTableName(tableName, schema)} WHERE {QuoteIdentifier(columnName)} = @pokemonMoveId LIMIT 1";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@pokemonMoveId";
        parameter.Value = pokemonMoveId;
        command.Parameters.Add(parameter);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && result is not DBNull;
    }

    private static string QualifyTableName(string tableName, string? schema) =>
        string.IsNullOrWhiteSpace(schema)
            ? QuoteIdentifier(tableName)
            : $"{QuoteIdentifier(schema)}.{QuoteIdentifier(tableName)}";

    private static string QuoteIdentifier(string identifier) =>
        $"\"{identifier.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
