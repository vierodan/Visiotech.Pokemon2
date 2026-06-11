using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Visiotech.Pokemon.Application.Abstractions.Persistence;
using Visiotech.Pokemon.Domain.Pokemons;

namespace Visiotech.Pokemon.Infrastructure.Persistence;

public sealed class MyPokemonDeletionDependencyChecker(PokemonDbContext dbContext)
    : IMyPokemonDeletionDependencyChecker
{
    public async Task<IReadOnlyCollection<string>> GetBlockingReasonsAsync(
        Guid myPokemonId,
        CancellationToken cancellationToken)
    {
        var myPokemonEntityType = dbContext.Model.FindEntityType(typeof(MyPokemon))
            ?? throw new InvalidOperationException("My pokemon entity type is not mapped.");

        var foreignKeys = dbContext.Model
            .GetEntityTypes()
            .SelectMany(static entityType => entityType.GetForeignKeys())
            .Where(foreignKey =>
                foreignKey.PrincipalEntityType == myPokemonEntityType &&
                !foreignKey.IsOwnership &&
                foreignKey.DeclaringEntityType.ClrType != typeof(MyPokemonMoveSlot))
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
            var activeBattleIds = await GetActiveBattleIdsAsync(connection, myPokemonId, cancellationToken);

            var blockingReasons = activeBattleIds
                .Select(static battleId => $"My pokemon cannot be deleted because it participates in active battle '{battleId}'.")
                .ToList();

            foreach (var foreignKey in foreignKeys)
            {
                if (!TryGetStoreMapping(foreignKey, out var tableName, out var schema, out var columnName))
                {
                    blockingReasons.Add(
                        $"My pokemon cannot be deleted because relation '{foreignKey.DeclaringEntityType.DisplayName()}' cannot be validated automatically.");
                    continue;
                }

                if (await HasReferenceAsync(connection, tableName, schema, columnName, myPokemonId, cancellationToken))
                {
                    blockingReasons.Add(
                        $"My pokemon cannot be deleted because it is referenced by '{foreignKey.DeclaringEntityType.DisplayName()}'.");
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
        Guid myPokemonId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            $"SELECT 1 FROM {QualifyTableName(tableName, schema)} WHERE {QuoteIdentifier(columnName)} = @myPokemonId LIMIT 1";

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@myPokemonId";
        parameter.Value = myPokemonId;
        command.Parameters.Add(parameter);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && result is not DBNull;
    }

    private static async Task<IReadOnlyCollection<Guid>> GetActiveBattleIdsAsync(
        System.Data.Common.DbConnection connection,
        Guid myPokemonId,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT DISTINCT bc.battle_id
            FROM "pokemon2"."battle_combatants" bc
            INNER JOIN "pokemon2"."battles" b ON b."Id" = bc.battle_id
            WHERE bc.my_pokemon_id = @myPokemonId
              AND b.status IN ('Created', 'InProgress')
            """;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "@myPokemonId";
        parameter.Value = myPokemonId;
        command.Parameters.Add(parameter);

        var battleIds = new List<Guid>();

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            battleIds.Add(reader.GetGuid(0));
        }

        return battleIds;
    }

    private static string QualifyTableName(string tableName, string? schema) =>
        string.IsNullOrWhiteSpace(schema)
            ? QuoteIdentifier(tableName)
            : $"{QuoteIdentifier(schema)}.{QuoteIdentifier(tableName)}";

    private static string QuoteIdentifier(string identifier) =>
        $"\"{identifier.Replace("\"", "\"\"", StringComparison.Ordinal)}\"";
}
