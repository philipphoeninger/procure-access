// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Configuration;

/// <summary>
/// Log table of the Serilog PostgreSQL sink (ARCHITECTURE §9.5). The migrations own the table;
/// the sink is configured with the names below and never creates tables itself.
/// </summary>
public class SeriLogEntryConfiguration : IEntityTypeConfiguration<SeriLogEntry>
{
    public const string Schema = "logging";
    public const string Table = "SeriLogs";

    public void Configure(EntityTypeBuilder<SeriLogEntry> builder)
    {
        builder.ToTable(Table, Schema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();

        builder.Property(x => x.TimeStamp).HasColumnType("timestamp with time zone");

        builder.Property(x => x.Level).HasMaxLength(32);

        // Log texts have no natural length limit: text instead of the context-wide default of 200.
        builder.Property(x => x.Message).HasColumnType("text");
        builder.Property(x => x.MessageTemplate).HasColumnType("text");
        builder.Property(x => x.Exception).HasColumnType("text");
        builder.Property(x => x.Properties).HasColumnType("jsonb");
        builder.Property(x => x.SourceContext).HasColumnType("text");
        builder.Property(x => x.ApplicationName).HasColumnType("text");
        builder.Property(x => x.MachineName).HasColumnType("text");

        // The retention job deletes by time stamp.
        builder.HasIndex(x => x.TimeStamp);
    }
}
