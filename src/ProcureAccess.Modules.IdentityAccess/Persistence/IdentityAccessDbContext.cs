// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence.Configurations;

namespace ProcureAccess.Modules.IdentityAccess.Persistence;

/// <summary>
/// The IdentityAccess module's own database context (ARCHITECTURE ADR-014) in the <c>identity</c>
/// schema. It holds the Data Protection key ring (§9.1, C60; S3-24) and, since S4-25, the contributor
/// profiles and their user links (§5.2) and, since S5-12a, the hashed account tokens (§9.1); users and memberships follow with migration step 3. No other
/// module and no legacy code uses it.
/// </summary>
/// <remarks>Its history table is <c>identity."__EFMigrationsHistory"</c>.</remarks>
internal sealed class IdentityAccessDbContext(DbContextOptions<IdentityAccessDbContext> options)
    : DbContext(options), IDataProtectionKeyContext
{
    public const string Schema = "identity";
    public const string HistoryTable = "__EFMigrationsHistory";

    /// <summary>
    /// ICU collation that compares case-insensitively (strength level 2: „Anna“ = „anna“, but
    /// „Ä“ ≠ „A“). Non-deterministic, so PostgreSQL refuses <c>LIKE</c> on its columns.
    /// It lives in <c>public</c>, not in <c>identity</c>: a column names its collation without a schema
    /// (EF quotes a qualified name as one identifier), and PostgreSQL finds it only on the search path.
    /// </summary>
    public const string CaseInsensitiveCollation = "identity_case_insensitive";

    /// <summary>The Data Protection key ring, read and written only by ASP.NET Core Data Protection.</summary>
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    public DbSet<ContributorProfile> ContributorProfiles => Set<ContributorProfile>();

    /// <summary>Restricted (PRV-04): never mapped to a DTO, never returned with a profile.</summary>
    public DbSet<UserContributorLink> UserContributorLinks => Set<UserContributorLink>();

    /// <summary>The hashed single-use tokens of the account e-mails (S5-12a, C72).</summary>
    public DbSet<AccountToken> AccountTokens => Set<AccountToken>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // As in the other module contexts: set here, so every instance (application, design-time
        // tools, tests) uses the module's history table.
        optionsBuilder.UseNpgsql(npgsql => npgsql.MigrationsHistoryTable(HistoryTable, Schema));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.HasCollation("public", CaseInsensitiveCollation, locale: "und-u-ks-level2", provider: "icu", deterministic: false);
        modelBuilder.ApplyConfiguration(new DataProtectionKeyConfiguration());
        modelBuilder.ApplyConfiguration(new ContributorProfileConfiguration());
        modelBuilder.ApplyConfiguration(new UserContributorLinkConfiguration());
        modelBuilder.ApplyConfiguration(new AccountTokenConfiguration());
    }
}
