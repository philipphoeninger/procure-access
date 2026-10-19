namespace DAL.ProcureAccess.EFStructures;

/// <summary>
/// Used by the dotnet-ef tools at design time. Reads the connection string from the
/// environment variable ConnectionStrings__ProcureAccess (see .env.example and CLAUDE.md §5).
/// </summary>
public class ApplicationDBContextFactory : IDesignTimeDbContextFactory<ApplicationDBContext>
{
    private const string ConnectionStringVariable = "ConnectionStrings__ProcureAccess";

    public ApplicationDBContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Set the environment variable {ConnectionStringVariable} before running dotnet ef.");
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
