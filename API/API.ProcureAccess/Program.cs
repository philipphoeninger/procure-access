[assembly: ApiController]

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers()
                .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve);
builder.Services.AddSwaggerExplorer(builder.Configuration)
                .AddProcureAccessApiVersionConfiguration(new ApiVersion(1, 0))
                .AddSqlServerConnection(builder.Configuration)
                .AddAppConfig(builder.Configuration)
                .AddCors()
                .AddIdentityHandlersAndStores()
                .ConfigureIdentityOptions()
                .AddHttpContextAccessor()
                .AddIdentityAuth(builder.Configuration)
                .AddRepositories();


// Configure logging
builder.ConfigureSerilog();
builder.Services.RegisterLoggingInterfaces();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Initialize the database
    if (app.Configuration.GetValue<bool>("RebuildDatabase"))
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        //SampleDataInitializer.InitializeData(dbContext, userManager);
        await SampleDataInitializer.ClearAndReseedDatabase(dbContext, userManager);
    }
    app.ConfigureSwaggerExplorer();
}

app.UseHttpsRedirection();

app.ConfigureCORS(builder.Configuration)
   .AddIdentityAuthMiddlewares();

app.MapControllers();
app.MapGroup("/api")
   .MapIdentityApi<User>();
app.MapGroup("/api")
   .MapIdentityUserEndpoints();


// add anonymously available health check endpoint
if (app.Environment.IsDevelopment())
{
    app.MapGet("/health", () => Results.Ok(new { status = "Healthy", time = DateTime.UtcNow }))
       .AllowAnonymous();
    // .WithName("GetHealth")
    // .WithOpenApi();
}

app.Run();
