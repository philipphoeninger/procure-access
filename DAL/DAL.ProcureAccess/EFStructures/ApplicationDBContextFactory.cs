// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace DAL.ProcureAccess.EFStructures;

/// <summary>
/// Used by the dotnet-ef tools at design time. Reads the connection string from the
/// environment variable ConnectionStrings__ProcureAccess (see .env.example and CLAUDE.md §5).
/// Without it a connection string without password is used: commands that never connect
/// (<c>migrations add</c>, <c>migrations has-pending-model-changes</c>) then need no secret,
/// and commands that connect (<c>database update</c>) fail with an authentication error.
/// </summary>
public class ApplicationDBContextFactory : IDesignTimeDbContextFactory<ApplicationDBContext>
{
    private const string ConnectionStringVariable = "ConnectionStrings__ProcureAccess";
    private const string DesignTimeConnectionString = "Host=localhost;Port=5432;Database=ProcureAccessDb;Username=postgres";

    public ApplicationDBContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Console.Error.WriteLine(
                $"{ConnectionStringVariable} is not set: using a connection string without password. " +
                "Set it for commands that connect to the database.");
            connectionString = DesignTimeConnectionString;
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDBContext>();
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.EnableRetryOnFailure();
            options.CommandTimeout(60);
        });
        return new ApplicationDBContext(optionsBuilder.Options);
    }
}
