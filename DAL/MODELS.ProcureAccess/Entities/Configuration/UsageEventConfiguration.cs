// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace MODELS.ProcureAccess.Entities.Configuration;

/// <summary>
/// Table of the usage events in the <c>analytics</c> schema (ARCHITECTURE §5.7). Date only and no
/// personal reference (PRV-05).
/// </summary>
public class UsageEventConfiguration : IEntityTypeConfiguration<UsageEvent>
{
    public const string Schema = "analytics";
    public const string Table = "UsageEvents";

    /// <summary>Longest dimension value of SPEC §9.1 is <c>anonymous</c>; 32 leaves room.</summary>
    public const int DimensionMaxLength = 32;

    public void Configure(EntityTypeBuilder<UsageEvent> builder)
    {
        builder.ToTable(Table, Schema);
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseIdentityByDefaultColumn();

        builder.Property(x => x.EventType).HasConversion<string>().HasMaxLength(64);
        builder.Property(x => x.OccurredOn).HasColumnType("date");
        builder.Property(x => x.Dimension1).HasMaxLength(DimensionMaxLength);
        builder.Property(x => x.Dimension2).HasMaxLength(DimensionMaxLength);

        // The indicators count events per type and month (SPEC §9.2).
        builder.HasIndex(x => new { x.EventType, x.OccurredOn });
    }
}
