// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

[assembly: ApiController]

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
                .AddAutoMapper(cfg => { },
                    typeof(MappingProfile).Assembly)
                .AddLocalization(options => options.ResourcesPath = "Resources");

// The application reads time only through TimeProvider (CLAUDE.md §7); tests replace it with FakeTimeProvider.
builder.Services.AddSingleton(TimeProvider.System);

// Configure logging: console + PostgreSQL sink in the logging schema, with retention (ARCHITECTURE §9.5)
builder.ConfigureSerilog();
builder.Services.RegisterLoggingInterfaces()
                .AddLogRetention();

// Usage events in the analytics schema, date only (ARCHITECTURE §5.7); nothing writes them yet (S1-23)
builder.Services.AddUsageEvents();

// /health/live and /health/ready (ARCHITECTURE §9.5)
builder.Services.AddProcureAccessHealthChecks();

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
    // not match the model.
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        app.Logger.LogInformation("Applying pending migrations...");
        db.Database.Migrate(); // creates DB if missing
        app.Logger.LogInformation("Migrations applied successfully!");
    }

    // Sample data only in Development, whatever the configuration says (SEC-06)
    await app.RebuildDatabaseIfAllowedAsync();

    // Endpoints are mapped before the Swagger UI is configured: the UI lists the API versions
    // through app.DescribeApiVersions(), which finds controllers anyway but minimal APIs only once
    // they are mapped. SwaggerUiTests fails when a versioned minimal API is mapped too late. The
    // position of Map* calls does not change the middleware order: WebApplication runs routing
    // first and the endpoints last.
    app.MapControllers();
    app.MapGroup("/api")
       .MapIdentityUserEndpoints();

    app.MapProcureAccessHealthChecks();

    app.ConfigureSwaggerExplorer();

    //app.UseHttpsRedirection();
    app.UseRequestLocalization();

    app.ConfigureCORS()
       .AddIdentityAuthMiddlewares();
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, Program.StartupFailedMessage);
    try
    {
        // Bounded: when the database itself is unreachable, the sink's last write attempt must
        // not hold up the exit. The entry is on the console in any case.
        await app.DisposeAsync().AsTask().WaitAsync(Program.StartupFailureFlushTimeout);
    }
    catch (Exception)
    {
        // A timeout or an error while disposing must not hide the startup error.
    }
    throw;
}

app.Run();

public partial class Program
{
    /// <summary>Message template of the log entry written when the startup fails.</summary>
    public const string StartupFailedMessage = "The API failed to start and stops.";

    internal static readonly TimeSpan StartupFailureFlushTimeout = TimeSpan.FromSeconds(5);
}
