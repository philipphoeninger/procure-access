// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence.Configurations;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;

namespace ProcureAccess.Modules.IdentityAccess.Persistence;

/// <summary>
/// The IdentityAccess module's own database context (ARCHITECTURE ADR-014) in the <c>identity</c>
/// schema. It holds the Data Protection key ring (§9.1, C60; S3-24) and, since S4-25, the contributor
/// profiles and their user links (§5.2), since S5-12a the hashed account tokens (§9.1) and since
/// S5-15 the organisations and their memberships (§5.2, §5.4), since S5-17a the invitations, and
/// since S6-10 the users and roles: the seven ASP.NET Core Identity tables, taken over from the
/// legacy context with their rows (ADR-014 rule 4; <see cref="IdentityTablesConfiguration"/>). No
/// other module and no legacy code uses it.
/// </summary>
/// <remarks>
/// Its history table is <c>identity."__EFMigrationsHistory"</c>. The organisation-scoped tables
/// carry the global query filter of ADR-007, bound to <paramref name="organizations"/> (TEN-01):
/// the context sees only the organisations of the request. A context built outside a request
/// (design-time factory, fixtures) gets <see cref="FixedOrganizationContext.None"/>.
/// </remarks>
internal sealed class IdentityAccessDbContext(
    DbContextOptions<IdentityAccessDbContext> options, IOrganizationContext organizations)
    : IdentityDbContext<User>(options), IDataProtectionKeyContext
{
    // A field, not the primary-constructor parameter: EF turns a member chain rooted at the context
    // instance into a query parameter, so the filter is evaluated per query, never cached in the model.
    private readonly IOrganizationContext _organizations = organizations;

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

    /// <summary>The tenants (§5.2); filtered to the organisations of the request (TEN-01).</summary>
    public DbSet<Organization> Organizations => Set<Organization>();

    /// <summary>
    /// The memberships (§5.2, §9.1); filtered like the organisations. The per-request loader of the
    /// current user's own rows is the one allow-listed unfiltered read (TEN-02).
    /// </summary>
    public DbSet<OrganizationMembership> OrganizationMemberships => Set<OrganizationMembership>();

    /// <summary>
    /// The invitations (§5.2, S5-17a); filtered like the memberships. The read by the link's token and
    /// the clean-up of expired rows are the allow-listed unfiltered reads (TEN-02).
    /// </summary>
    public DbSet<OrganizationInvitation> OrganizationInvitations => Set<OrganizationInvitation>();

    /// <summary>The settings of each user who saved any (S5-25a); not organisation-scoped.</summary>
    public DbSet<UserPreferences> UserPreferences => Set<UserPreferences>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // As in the other module contexts: set here, so every instance (application, design-time
        // tools, tests) uses the module's history table.
        optionsBuilder.UseNpgsql(npgsql => npgsql.MigrationsHistoryTable(HistoryTable, Schema));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Identity's own mapping first (keys, indexes, the 256-character names and addresses).
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);
        IdentityTablesConfiguration.Apply(modelBuilder);
        modelBuilder.HasCollation("public", CaseInsensitiveCollation, locale: "und-u-ks-level2", provider: "icu", deterministic: false);
        modelBuilder.ApplyConfiguration(new DataProtectionKeyConfiguration());
        modelBuilder.ApplyConfiguration(new ContributorProfileConfiguration());
        modelBuilder.ApplyConfiguration(new UserContributorLinkConfiguration());
        modelBuilder.ApplyConfiguration(new AccountTokenConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationMembershipConfiguration());
        modelBuilder.ApplyConfiguration(new OrganizationInvitationConfiguration());
        modelBuilder.ApplyConfiguration(new UserPreferencesConfiguration());

        // The organisation filter (ADR-007, TEN-01): a row of an organisation the request's user is
        // no member of does not exist for this context. The organisation row is scoped by its own id.
        modelBuilder.Entity<Organization>()
                    .HasQueryFilter(o => _organizations.OrganizationIds.Contains(o.Id));
        modelBuilder.Entity<OrganizationMembership>()
                    .HasQueryFilter(m => _organizations.OrganizationIds.Contains(m.OrganizationId));
        modelBuilder.Entity<OrganizationInvitation>()
                    .HasQueryFilter(i => _organizations.OrganizationIds.Contains(i.OrganizationId));
    }
}
