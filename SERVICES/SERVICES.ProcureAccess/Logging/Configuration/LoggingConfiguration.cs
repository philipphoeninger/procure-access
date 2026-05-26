namespace SERVICES.ProcureAccess.Logging.Configuration;

public static class LoggingConfiguration
{
    internal static readonly string OutputTemplate =
@"[{Timestamp:yy-MM-dd HH:mm:ss} {Level:u3}] {ApplicationName}:{SourceContext}
Message: {Message:lj}
in method {MemberName} at {FilePath}:{LineNumber}
{Exception}
";

    public static void ConfigureSerilog(this WebApplicationBuilder builder)
    {
        builder.Logging.ClearProviders();

        var config = builder.Configuration;

        var settings = config
            .GetSection(nameof(AppLoggingSettings))
            .Get<AppLoggingSettings>();

        var restrictedToMinimumLevel =
            settings.General.RestrictedToMinimumLevel;

        if (!Enum.TryParse<LogEventLevel>(
                restrictedToMinimumLevel,
                true,
                out var logLevel))
        {
            logLevel = LogEventLevel.Debug;
        }

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)

            .Enrich.FromLogContext()

            .Enrich.With(new PropertyEnricher(
                "ApplicationName",
                config.GetValue<string>("ApplicationName") ?? "Unknown"))

            .Enrich.WithMachineName()

            .WriteTo.Console(
                restrictedToMinimumLevel: logLevel,
                outputTemplate: OutputTemplate)

            .WriteTo.File(
                path: builder.Environment.IsDevelopment()
                    ? settings.File.FileName
                    : settings.File.FullLogPathAndFileName,

                rollingInterval: RollingInterval.Day,

                restrictedToMinimumLevel: logLevel,

                outputTemplate: OutputTemplate);

        builder.Logging.AddSerilog(
            loggerConfiguration.CreateLogger(),
            dispose: false);
    }

    public static IServiceCollection RegisterLoggingInterfaces(
        this IServiceCollection services)
    {
        services.AddScoped(typeof(IAppLogging<>), typeof(AppLogging<>));

        return services;
    }
}
