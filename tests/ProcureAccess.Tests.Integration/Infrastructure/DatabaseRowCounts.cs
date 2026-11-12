// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Npgsql;

namespace ProcureAccess.Tests.Integration.Infrastructure;

/// <summary>
/// The "persists nothing" helper of TESTING §9 step 2 (EXP-05): the exact row count of every table
/// of the database, found through <c>information_schema</c>, so a table added later is covered
/// without a change here. Compare a snapshot before and after the call under test.
/// </summary>
internal static class DatabaseRowCounts
{
    /// <summary>
    /// Counts the rows of every base table outside the system schemas and
    /// <paramref name="excludedSchemas"/>, keyed <c>schema.table</c>.
    /// </summary>
    public static async Task<IReadOnlyDictionary<string, long>> TakeAsync(
        string connectionString,
        IReadOnlyCollection<string> excludedSchemas,
        CancellationToken cancellationToken)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        var tables = new List<(string Schema, string Table)>();
        await using (var list = new NpgsqlCommand(
            """
            SELECT table_schema, table_name FROM information_schema.tables
            WHERE table_type = 'BASE TABLE'
              AND table_schema NOT IN ('pg_catalog', 'information_schema')
              AND table_schema <> ALL(@excluded)
            ORDER BY table_schema, table_name
            """,
            connection))
        {
            list.Parameters.AddWithValue("excluded", excludedSchemas.ToArray());
            await using var reader = await list.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                tables.Add((reader.GetString(0), reader.GetString(1)));
            }
        }

        var counts = new Dictionary<string, long>(StringComparer.Ordinal);
        foreach (var (schema, table) in tables)
        {
            // Names come from information_schema, not from input; quoted as identifiers.
            await using var count = new NpgsqlCommand(
                $"SELECT count(*) FROM \"{schema.Replace("\"", "\"\"", StringComparison.Ordinal)}\".\"{table.Replace("\"", "\"\"", StringComparison.Ordinal)}\"",
                connection);
            counts[$"{schema}.{table}"] = (long)(await count.ExecuteScalarAsync(cancellationToken))!;
        }

        return counts;
    }
}
