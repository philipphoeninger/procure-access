// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NpgsqlTypes;
using ProcureAccess.Modules.Platform.Persistence.Configurations;
using Serilog;
using Serilog.Core.Enrichers;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using Serilog.Sinks.PostgreSQL.ColumnWriters;

namespace ProcureAccess.Modules.Platform.Logging;

/// <summary>Serilog set-up and log retention (ARCHITECTURE §9.5).</summary>
internal static class LoggingRegistration
{
    internal static readonly string OutputTemplate =
@"[{Timestamp:yy-MM-dd HH:mm:ss} {Level:u3}] {ApplicationName}:{SourceContext}
Message: {Message:lj}
in method {MemberName} at {FilePath}:{LineNumber}
{Exception}
";

    /// <summary>
    /// Columns of <c>logging."SeriLogs"</c> (<see cref="SeriLogEntryConfiguration"/>) and what the
    /// sink writes into each. The migrations create the table; the sink never does.
    /// </summary>
    private static Dictionary<string, ColumnWriterBase> PostgreSqlColumns() => new()
    {
        [nameof(SeriLogEntry.TimeStamp)] = new TimestampColumnWriter(NpgsqlDbType.TimestampTz),
        [nameof(SeriLogEntry.Level)] = new LevelColumnWriter(true, NpgsqlDbType.Varchar),
        [nameof(SeriLogEntry.Message)] = new RenderedMessageColumnWriter(NpgsqlDbType.Text),
        [nameof(SeriLogEntry.MessageTemplate)] = new MessageTemplateColumnWriter(NpgsqlDbType.Text),
        [nameof(SeriLogEntry.Exception)] = new ExceptionColumnWriter(NpgsqlDbType.Text),
        [nameof(SeriLogEntry.Properties)] = new PropertiesColumnWriter(NpgsqlDbType.Jsonb),
        [nameof(SeriLogEntry.SourceContext)] = PropertyColumn("SourceContext"),
        [nameof(SeriLogEntry.ApplicationName)] = PropertyColumn("ApplicationName"),
        [nameof(SeriLogEntry.MachineName)] = PropertyColumn("MachineName"),
    };

    private static SinglePropertyColumnWriter PropertyColumn(string propertyName) =>
        new(propertyName, PropertyWriteMethod.Raw, NpgsqlDbType.Text);

    /// <summary>
    /// Replaces the default logging providers with Serilog: console and PostgreSQL sink
    /// (ARCHITECTURE §9.5). The PostgreSQL sink writes in batches on a background thread; if the
    /// database is unreachable it keeps at most <c>QueueLimit</c> events and drops newer ones
    /// instead of failing requests.
    /// </summary>
    public static IServiceCollection AddPlatformLogging(this IServiceCollection services, IConfiguration config)
    {
        var settings = config
            .GetSection(AppLoggingSettings.SectionName)
            .Get<AppLoggingSettings>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{AppLoggingSettings.SectionName}' is missing.");

        // An unknown level fails startup instead of silently writing Debug events into the database.
        if (!Enum.TryParse<LogEventLevel>(
                settings.General.RestrictedToMinimumLevel,
                true,
                out var logLevel)
            || !Enum.IsDefined(logLevel))
        {
            throw new InvalidOperationException(
                $"{AppLoggingSettings.SectionName}:General:RestrictedToMinimumLevel '{settings.General.RestrictedToMinimumLevel}' is not a Serilog level.");
        }

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Is(logLevel)

            .Enrich.FromLogContext()

            // "AppName", not "ApplicationName": that key is the host's own application name
            // (keys ignore case), which WebApplicationFactory sets in tests but production does not.
            .Enrich.With(new PropertyEnricher(
                "ApplicationName",
                config.GetValue<string>("AppName") ?? "Unknown"))

            .Enrich.WithMachineName()

            .WriteTo.Console(
                restrictedToMinimumLevel: logLevel,
                outputTemplate: OutputTemplate);

        // Without a connection string (e.g. design-time tools) only the console sink is used.
        var connectionString = config.GetConnectionString(settings.PostgreSQL.ConnectionStringName);
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            if (settings.PostgreSQL.QueueLimit <= 0)
            {
                throw new InvalidOperationException(
                    $"{AppLoggingSettings.SectionName}:PostgreSQL:QueueLimit must be greater than 0.");
            }

            loggerConfiguration.WriteTo.PostgreSQL(
                connectionString,
                SeriLogEntryConfiguration.Table,
                PostgreSqlColumns(),
                restrictedToMinimumLevel: logLevel,
                queueLimit: settings.PostgreSQL.QueueLimit,
                schemaName: SeriLogEntryConfiguration.Schema,
                needAutoCreateTable: false,
                needAutoCreateSchema: false);
        }

        var logger = loggerConfiguration.CreateLogger();

        // dispose: true flushes the batched PostgreSQL sink when the host stops.
        services.AddLogging(logging => logging
            .ClearProviders()
            .AddSerilog(logger, dispose: true));

        return services;
    }

    /// <summary>
    /// Log retention (ARCHITECTURE §9.5, OPS-02): <see cref="LogRetentionJob"/> and the hosted
    /// service that runs it every <see cref="AppLoggingSettings.RetentionSettings.RunInterval"/>.
    /// </summary>
    public static IServiceCollection AddLogRetention(this IServiceCollection services)
    {
        services.AddOptions<AppLoggingSettings.RetentionSettings>()
            .BindConfiguration(AppLoggingSettings.RetentionSettings.SectionName)
            .Validate(s => s.RetentionDays > 0, "AppLoggingSettings:Retention:RetentionDays must be greater than 0.")
            .Validate(s => s.RunInterval > TimeSpan.Zero, "AppLoggingSettings:Retention:RunInterval must be greater than 0.")
            .ValidateOnStart();

        services.AddScoped<LogRetentionJob>();
        services.AddHostedService<LogRetentionService>();

        return services;
    }
}
