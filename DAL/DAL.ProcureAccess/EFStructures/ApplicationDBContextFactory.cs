namespace DAL.ProcureAccess.EFStructures;

public class ApplicationDBContextFactory : IDesignTimeDbContextFactory<ApplicationDBContext>
{
    public ApplicationDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDBContext>();
        var connectionString = @"Server=db;Database=ProcureAccessDb;User Id=sa;Password=YourStrongPassw0rd_h3r3;Encrypt=False;TrustServerCertificate=True;";

        // var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

        // var connectionString = isDocker
        //     ? "Server=db;Database=ProcureAccessDb;User Id=sa;Password=YourStrongPassw0rd_h3r3;Encrypt=False;TrustServerCertificate=True;"
        //     : "Server=localhost,14333;Database=ProcureAccessDb;User Id=sa;Password=YourStrongPassw0rd_h3r3;Encrypt=False;TrustServerCertificate=True;";

        
        // var connectionString =
        // Environment.GetEnvironmentVariable("EF_CONNECTION_STRING")
        // ?? "Server=localhost,14333;Database=ProcureAccessDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;";

        
        optionsBuilder.UseSqlServer(connectionString, options => options.EnableRetryOnFailure());
        Console.WriteLine(connectionString);
        return new ApplicationDBContext(optionsBuilder.Options);
    }
}
