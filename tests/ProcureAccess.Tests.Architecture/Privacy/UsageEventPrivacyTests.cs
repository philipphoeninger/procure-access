// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using ProcureAccess.Modules.Platform.Analytics;
using ProcureAccess.Modules.Platform.Persistence;
using ProcureAccess.Modules.Platform.Persistence.Configurations;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Architecture.Privacy;

/// <summary>
/// PRV-05: a usage event has no user, organisation, IP or session field and stores a date, not a
/// timestamp (ARCHITECTURE §5.7, SPEC R-MET-01). The columns are an allow-list: every new field
/// fails here and needs a SPEC change and a privacy reason first.
/// </summary>
public class UsageEventPrivacyTests
{
    private static readonly string[] AllowedColumns =
        ["Id", "EventType", "OccurredOn", "Dimension1", "Dimension2"];

    /// <summary>Name fragments that point to a personal reference (checked on top of the allow-list).</summary>
    private static readonly string[] PersonalFragments =
        ["user", "org", "tenant", "session", "ip", "address", "mail", "name", "contributor", "account", "agent"];

    [Fact(DisplayName = "INV:PRV-05 " + nameof(UsageEventEntity_Properties_OnlyAllowedFields))]
    [Trait("Invariant", "PRV-05")]
    public void UsageEventEntity_Properties_OnlyAllowedFields()
    {
        var properties = typeof(UsageEvent).GetProperties().Select(property => property.Name);

        properties.ShouldBe(AllowedColumns, ignoreOrder: true);
    }

    [Fact(DisplayName = "INV:PRV-05 " + nameof(UsageEventTable_MappedColumns_OnlyAllowedColumnsWithoutPersonalReference))]
    [Trait("Invariant", "PRV-05")]
    public void UsageEventTable_MappedColumns_OnlyAllowedColumnsWithoutPersonalReference()
    {
        var entityType = UsageEventEntityType();
        var table = StoreObjectIdentifier.Table(UsageEventConfiguration.Table, UsageEventConfiguration.Schema);

        entityType.GetSchema().ShouldBe(UsageEventConfiguration.Schema);
        entityType.GetTableName().ShouldBe(UsageEventConfiguration.Table);

        // Includes shadow properties, which the entity class would not show.
        var columns = entityType.GetProperties().Select(property => property.GetColumnName(table)!).ToList();
        columns.ShouldBe(AllowedColumns, ignoreOrder: true);

        foreach (var column in columns.Where(column => column != "Id"))
        {
            foreach (var fragment in PersonalFragments)
            {
                column.Contains(fragment, StringComparison.OrdinalIgnoreCase)
                    .ShouldBeFalse($"Column '{column}' looks like a personal reference ('{fragment}').");
            }
        }
    }

    [Fact(DisplayName = "INV:PRV-05 " + nameof(UsageEventOccurredOn_TypeAndColumn_DateWithoutTimeOfDay))]
    [Trait("Invariant", "PRV-05")]
    public void UsageEventOccurredOn_TypeAndColumn_DateWithoutTimeOfDay()
    {
        typeof(UsageEvent).GetProperty(nameof(UsageEvent.OccurredOn))!.PropertyType.ShouldBe(typeof(DateOnly));

        var entityType = UsageEventEntityType();
        entityType.GetProperty(nameof(UsageEvent.OccurredOn)).GetColumnType().ShouldBe("date");

        var timeTypes = new[] { typeof(DateTime), typeof(DateTimeOffset), typeof(TimeOnly), typeof(TimeSpan) };
        entityType.GetProperties()
            .Where(property => timeTypes.Contains(Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType))
            .Select(property => property.Name)
            .ShouldBeEmpty("A usage event stores the day only.");
    }

    /// <summary>
    /// The design-time model of the real DbContext, the one the migrations are built from: the
    /// Platform module's own context since S2-07 (ADR-014). Building it needs no database connection.
    /// </summary>
    private static IEntityType UsageEventEntityType()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseNpgsql("Host=unused")
            .Options;
        using var context = new PlatformDbContext(options);

        var model = context.GetService<IDesignTimeModel>().Model;
        return model.FindEntityType(typeof(UsageEvent))
            ?? throw new InvalidOperationException("UsageEvent is not part of the model.");
    }
}
