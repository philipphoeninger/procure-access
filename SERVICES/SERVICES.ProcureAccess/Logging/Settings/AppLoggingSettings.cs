// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace SERVICES.ProcureAccess.Logging.Settings;

/// <summary>
/// Configuration section <c>AppLoggingSettings</c> (appsettings.json, overridable per environment
/// and through environment variables such as <c>AppLoggingSettings__Retention__RetentionDays</c>).
/// </summary>
public class AppLoggingSettings
{
    public const string SectionName = nameof(AppLoggingSettings);

    public GeneralSettings General { get; set; } = new();
    public PostgreSQLSettings PostgreSQL { get; set; } = new();
    public RetentionSettings Retention { get; set; } = new();

    public class GeneralSettings
    {
        /// <summary>Minimum level for all sinks (Serilog level name, e.g. <c>Warning</c>).</summary>
        public string RestrictedToMinimumLevel { get; set; } = string.Empty;
    }

    public class PostgreSQLSettings
    {
        /// <summary>Name of the connection string the sink writes with (same database as the app).</summary>
        public string ConnectionStringName { get; set; } = string.Empty;

        /// <summary>
        /// Most events kept in memory while the database cannot be written to; newer events are
        /// dropped beyond it. Without a limit the queue would grow for as long as the database is down.
        /// </summary>
        public int QueueLimit { get; set; }
    }

    /// <summary>Log retention in the <c>logging</c> schema (ARCHITECTURE §9.5, OPS-02).</summary>
    public class RetentionSettings
    {
        public const string SectionName = $"{AppLoggingSettings.SectionName}:Retention";

        /// <summary>Rows older than this many days are deleted.</summary>
        public int RetentionDays { get; set; }

        /// <summary>Time between two runs of the retention job.</summary>
        public TimeSpan RunInterval { get; set; }
    }
}
