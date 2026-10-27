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

// Automatically apply migrations. A failure stops the startup: the API must not serve requests
// against a schema that does not match the model.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("Applying pending migrations...");
        db.Database.Migrate(); // creates DB if missing
        logger.LogInformation("Migrations applied successfully!");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Applying the database migrations failed; the API stops.");
        throw;
    }
}

// Sample data only in Development, whatever the configuration says (SEC-06)
await app.RebuildDatabaseIfAllowedAsync();

// Endpoints are mapped before the Swagger UI is configured, because the UI lists the API versions
// of the mapped endpoints (app.DescribeApiVersions). The position of Map* calls does not change the
// middleware order: WebApplication runs routing first and the endpoints last.
app.MapControllers();
app.MapGroup("/api")
   .MapIdentityUserEndpoints();

app.MapProcureAccessHealthChecks();

app.ConfigureSwaggerExplorer();

//app.UseHttpsRedirection();
app.UseRequestLocalization();

app.ConfigureCORS()
   .AddIdentityAuthMiddlewares();

app.Run();
