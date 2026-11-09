// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Persistence.Configurations;

namespace ProcureAccess.Modules.IdentityAccess.Persistence;

/// <summary>
/// The IdentityAccess module's own database context (ARCHITECTURE ADR-014) in the <c>identity</c>
/// schema. So far it holds only the Data Protection key ring (§9.1, C60; S3-24); users, memberships
/// and contributor profiles follow with migration step 3. No other module and no legacy code uses it.
/// </summary>
/// <remarks>Its history table is <c>identity."__EFMigrationsHistory"</c>.</remarks>
internal sealed class IdentityAccessDbContext(DbContextOptions<IdentityAccessDbContext> options)
    : DbContext(options), IDataProtectionKeyContext
{
    public const string Schema = "identity";
    public const string HistoryTable = "__EFMigrationsHistory";

    /// <summary>The Data Protection key ring, read and written only by ASP.NET Core Data Protection.</summary>
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // As in the other module contexts: set here, so every instance (application, design-time
        // tools, tests) uses the module's history table.
        optionsBuilder.UseNpgsql(npgsql => npgsql.MigrationsHistoryTable(HistoryTable, Schema));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new DataProtectionKeyConfiguration());
    }
}
