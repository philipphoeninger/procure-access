// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProcureAccess.Modules.IdentityAccess.Persistence;

/// <summary>
/// Used by the dotnet-ef tools at design time (<c>--context IdentityAccessDbContext</c>, CLAUDE.md §5).
/// Reads the connection string from the environment variable ConnectionStrings__ProcureAccess.
/// Without it a connection string without password is used: commands that never connect
/// (<c>migrations add</c>, <c>migrations has-pending-model-changes</c>) then need no secret.
/// </summary>
internal sealed class IdentityAccessDbContextFactory : IDesignTimeDbContextFactory<IdentityAccessDbContext>
{
    private const string ConnectionStringVariable = "ConnectionStrings__ProcureAccess";
    private const string DesignTimeConnectionString = "Host=localhost;Port=5432;Database=ProcureAccessDb;Username=postgres";

    public IdentityAccessDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.Error.WriteLine(
                $"{ConnectionStringVariable} is not set: using a connection string without password. " +
                "Set it for commands that connect to the database.");
            connectionString = DesignTimeConnectionString;
        }

        var options = new DbContextOptionsBuilder<IdentityAccessDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new IdentityAccessDbContext(options);
    }
}
