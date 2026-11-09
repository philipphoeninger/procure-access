// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

[assembly: ApiController]

// `catalog import <version>` and `catalog activate <version>` run the catalog command and exit instead of starting the API (S3-16, S3-17).
if (CatalogModule.IsCommand(args))
{
    return await CatalogCommandHost.RunAsync(args);
}

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
                .AddJsonOptions(x =>
                {
                    x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    x.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });
builder.Services.AddSwaggerExplorer(builder.Configuration)
                .AddProcureAccessApiVersionConfiguration(new ApiVersion(1, 0))
                .AddPostgreSQLConnection(builder.Configuration)
                .AddAppConfig(builder.Configuration)
                .AddEmailConfig(builder.Configuration)
                .AddFrontendCors(builder.Configuration)
                .AddIdentityHandlersAndStores()
                .ConfigureIdentityOptions()
                .AddHttpContextAccessor()
                .AddIdentityAuth(builder.Configuration)
                .AddRepositories()
                .AddDataServices()
                .AddLocalization(options => options.ResourcesPath = "Resources");

// Module projects (ARCHITECTURE §4.3). Platform holds time, logging with retention, usage events,
// the health checks (S2-07) and Problem Details (S2-10); IdentityAccess the Data Protection key
// store (S3-24); Catalog its database context (S3-12). The others register nothing yet: the legacy
// code in API, DAL, MODELS and SERVICES moves into them with the migration step that replaces it
// (S2-06, owner decision O-S2-1).
builder.Services.AddPlatformModule(builder.Configuration)
                .AddIdentityAccessModule(builder.Configuration)
                .AddCatalogModule(builder.Configuration)
                .AddConfiguratorModule()
                .AddExportModule()
                .AddDirectoryModule()
                .AddWorkspaceModule()
                .AddModerationModule();

// Legacy logging wrapper of the legacy controllers; Serilog itself is set up by the Platform module
builder.Services.RegisterLoggingInterfaces();

// Configure localization
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { "en", "de" };

    options.SetDefaultCulture("de")
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
});

Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");

var app = builder.Build();

// Every error between Build() and Run() stops the API. It is logged, and disposing the app flushes
// the batched PostgreSQL log sink before the process ends (app.Run() disposes the app itself).
try
{
    // Automatically apply migrations: the API must not serve requests against a schema that does
    // not match the model. The legacy context first (it creates the database if missing), then the
    // module contexts in registration order (ADR-014).
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        app.Logger.LogInformation("Applying pending migrations...");
        db.Database.Migrate(); // creates DB if missing
        foreach (var migrator in scope.ServiceProvider.GetServices<IModuleDatabaseMigrator>())
        {
            app.Logger.LogInformation("Applying pending migrations of module {Module}...", migrator.ModuleName);
            await migrator.MigrateAsync(CancellationToken.None);
        }
        app.Logger.LogInformation("Migrations applied successfully!");
    }

    // Sample data only in Development, whatever the configuration says (SEC-06)
    await app.RebuildDatabaseIfAllowedAsync();

    // First in the pipeline: errors of every later middleware become Problem Details (S2-10)
    app.UsePlatformMiddleware();

    // Endpoints. WebApplication runs routing first and the endpoints last, so mapping them here
    // does not change the middleware order; it must come before the Swagger set-up, which lists the
    // API versions of the endpoints mapped so far (#176).
    app.MapControllers();
    app.MapGroup("/api")
       .MapIdentityUserEndpoints();
    app.MapPlatformEndpoints();

    // Module endpoints under /api/v1 (ARCHITECTURE §7.1; owner, S3-19: only the new endpoints are
    // versioned, the legacy controllers stay at /api/<controller>, #153).
    app.NewVersionedApi()
       .MapGroup("/api/v{version:apiVersion}")
       .HasApiVersion(1, 0)
       .MapCatalogEndpoints();

    app.ConfigureSwaggerExplorer();

    //app.UseHttpsRedirection();
    app.UseRequestLocalization();

    app.ConfigureCORS()
       .AddIdentityAuthMiddlewares();
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, Program.StartupFailedMessage);
    await app.DisposeAsync();
    throw;
}

app.Run();
return 0;

public partial class Program
{
    /// <summary>Message template of the log entry written when the startup fails.</summary>
    public const string StartupFailedMessage = "The API failed to start and stops.";
}
