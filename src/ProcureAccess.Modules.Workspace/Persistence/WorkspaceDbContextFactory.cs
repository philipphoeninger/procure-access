// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;

namespace ProcureAccess.Modules.Workspace.Persistence;

/// <summary>
/// Used by the dotnet-ef tools at design time (<c>--context WorkspaceDbContext</c>, CLAUDE.md §5).
/// Reads the connection string from the environment variable ConnectionStrings__ProcureAccess.
/// Without it a connection string without password is used: commands that never connect
/// (<c>migrations add</c>, <c>migrations has-pending-model-changes</c>) then need no secret.
/// </summary>
internal sealed class WorkspaceDbContextFactory : IDesignTimeDbContextFactory<WorkspaceDbContext>
{
    private const string ConnectionStringVariable = "ConnectionStrings__ProcureAccess";
    private const string DesignTimeConnectionString = "Host=localhost;Port=5432;Database=ProcureAccessDb;Username=postgres";

    public WorkspaceDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.Error.WriteLine(
                $"{ConnectionStringVariable} is not set: using a connection string without password. " +
                "Set it for commands that connect to the database.");
            connectionString = DesignTimeConnectionString;
        }

        var options = new DbContextOptionsBuilder<WorkspaceDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        // No request, no organisation: the tools never read scoped rows.
        return new WorkspaceDbContext(options, FixedOrganizationContext.None);
    }
}
