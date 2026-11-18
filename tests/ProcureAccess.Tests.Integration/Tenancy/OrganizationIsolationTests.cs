// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Reflection;
using DAL.ProcureAccess.EFStructures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Modules.Configurator.Domain;
using ProcureAccess.Modules.Configurator.Persistence;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.IdentityAccess.Tenancy;
using ProcureAccess.Modules.Platform.Contracts.Sharing;
using ProcureAccess.Modules.Platform.Persistence;
using ProcureAccess.Modules.Platform.Sharing;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Tenancy;

/// <summary>
/// TEN-01 (ARCHITECTURE §5.4, ADR-007; S5-15): for every entity that carries <c>OrganizationId</c>
/// — found by reflection over the models of every context, so a new entity is covered as soon as
/// it exists — and for <see cref="Organization"/> itself, a row of organisation A is invisible to a
/// member of B and to a request without organisation. The behavioural part stores one sample row
/// per entity type through <see cref="Samples"/>; a new scoped entity fails until a sample is added.
/// </summary>
public sealed class OrganizationIsolationTests(ApiFixture api) : IntegrationTest(api)
{
    /// <summary>The contexts whose models are scanned: every module context and the legacy one.</summary>
    private static readonly Type[] ContextTypes =
    [
        typeof(ApplicationDBContext),
        typeof(PlatformDbContext),
        typeof(CatalogDbContext),
        typeof(IdentityAccessDbContext),
        typeof(ConfiguratorDbContext),
    ];

    /// <summary>
    /// One row of organisation <c>a</c> per scoped entity type. <see cref="Organization"/> maps to
    /// <c>null</c>: the organisations A and B are stored first and are their own samples.
    /// </summary>
    private static readonly IReadOnlyDictionary<Type, Func<Organization, DateTimeOffset, object?>> Samples =
        new Dictionary<Type, Func<Organization, DateTimeOffset, object?>>
        {
            [typeof(Organization)] = (_, _) => null,
            [typeof(OrganizationMembership)] = (a, now) =>
                OrganizationMembership.Create(a.Id, "sample-user", OrganizationRole.Reader, now, now),
            [typeof(OrganizationInvitation)] = (a, now) =>
                OrganizationInvitation.Create(a.Id, "sample@example.test", OrganizationRole.Reader, new byte[32], now, TimeSpan.FromDays(14)),
            [typeof(CriteriaConfiguration)] = (a, now) =>
                CriteriaConfiguration.Create(
                    a.Id, "Sample list", "sample-user", now, "T.1", "hr", ["web"], ["base"], [],
                    [new ConfigurationCriterionDraft(1, PriorityLevel.A)]),
            [typeof(ShareLink)] = (a, now) =>
                ShareLink.Create(a.Id, ShareableResourceType.Configuration, Guid.NewGuid(), new byte[32], now, now.AddDays(90)),
        };

    public static TheoryData<string> ScopedEntityNames => [.. ScopedEntityTypes().Select(t => t.FullName!)];

    [Fact(DisplayName = "INV:TEN-01 " + nameof(ScopedEntities_Discovery_FindsTheKnownOnes))]
    [Trait("Invariant", "TEN-01")]
    public void ScopedEntities_Discovery_FindsTheKnownOnes()
    {
        var types = ScopedEntityTypes();

        types.ShouldContain(typeof(Organization));
        types.ShouldContain(typeof(OrganizationMembership));
        types.ShouldContain(typeof(OrganizationInvitation));
        types.ShouldContain(typeof(CriteriaConfiguration));
        types.ShouldContain(typeof(ShareLink));
        types.ShouldBe(Samples.Keys, ignoreOrder: true,
            "Every entity with OrganizationId needs a sample row in Samples (TEN-01), and every sample a live entity.");
    }

    [Theory(DisplayName = "INV:TEN-01 " + nameof(ScopedEntity_Model_HasTheOrganizationFilter))]
    [Trait("Invariant", "TEN-01")]
    [MemberData(nameof(ScopedEntityNames))]
    public void ScopedEntity_Model_HasTheOrganizationFilter(string entityTypeName)
    {
        using var scope = Api.Factory.Services.CreateScope();
        var entityType = FindEntityType(scope.ServiceProvider, entityTypeName);

        entityType.GetDeclaredQueryFilters().ShouldNotBeEmpty($"{entityTypeName} carries OrganizationId but has no query filter (ADR-007).");
    }

    [Theory(DisplayName = "INV:TEN-01 " + nameof(ScopedEntity_RowOfOrganizationA_InvisibleToBAndToNoOrganization))]
    [Trait("Invariant", "TEN-01")]
    [MemberData(nameof(ScopedEntityNames))]
    public async Task ScopedEntity_RowOfOrganizationA_InvisibleToBAndToNoOrganization(string entityTypeName)
    {
        await Api.ResetDatabaseAsync();
        var now = Clock.GetUtcNow();
        var a = Organization.Create("Organisation A", OrganizationType.PublicBody, "owner-a", now);
        var b = Organization.Create("Organisation B", OrganizationType.Company, "owner-b", now);
        // The users the rows name: organisations and memberships reference them (S6-10b).
        await StoreAsync(TestAccounts.WithId("owner-a"), TestAccounts.WithId("owner-b"), TestAccounts.WithId("sample-user"));
        await StoreAsync(a, b);
        var entityType = Type.GetType(entityTypeName) ?? ScopedEntityTypes().Single(t => t.FullName == entityTypeName);
        var sample = Samples[entityType](a, now);
        if (sample is not null)
        {
            await StoreAsync(sample);
        }

        // The organisation rows: A sees A, B sees B. Every other scoped type has one row, of A.
        var ownRows = entityType == typeof(Organization) ? 1 : 0;
        (await CountVisibleAsync(entityType, [a.Id])).ShouldBe(1, "a member of A sees the row of A");
        (await CountVisibleAsync(entityType, [b.Id])).ShouldBe(ownRows, "a member of B does not see the row of A");
        (await CountVisibleAsync(entityType, [])).ShouldBe(0, "a request without organisation sees nothing");
        (await CountVisibleAsync(entityType, [a.Id, b.Id])).ShouldBe(1 + ownRows, "a member of both sees both");
    }

    /// <summary>
    /// Counts the rows the request-scoped context shows after the module's organisation context was
    /// loaded with <paramref name="organizationIds"/> — the same wiring a request uses.
    /// </summary>
    private async Task<int> CountVisibleAsync(Type entityType, IEnumerable<Guid> organizationIds)
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<OrganizationContext>()
             .Load(organizationIds.Select(id => new KeyValuePair<Guid, OrganizationRole>(id, OrganizationRole.Reader)));
        var context = ContextTypes.Select(scope.ServiceProvider.GetRequiredService).Cast<DbContext>()
                                  .Single(c => c.Model.FindEntityType(entityType) is not null);
        var count = typeof(OrganizationIsolationTests)
            .GetMethod(nameof(CountAsync), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(entityType)
            .Invoke(null, [context])!;
        return await (Task<int>)count;
    }

    private static Task<int> CountAsync<TEntity>(DbContext context) where TEntity : class =>
        context.Set<TEntity>().AsNoTracking().CountAsync(TestContext.Current.CancellationToken);

    private static IEntityType FindEntityType(IServiceProvider services, string entityTypeName) =>
        ContextTypes.Select(services.GetRequiredService).Cast<DbContext>()
                    .Select(c => c.Model.GetEntityTypes().SingleOrDefault(e => e.ClrType.FullName == entityTypeName))
                    .Single(e => e is not null)!;

    /// <summary>
    /// Every entity of every context with a property <c>OrganizationId</c>, plus the organisation
    /// itself. Built from the models without a database, so the theory data exists before the fixture.
    /// </summary>
    private static IReadOnlyList<Type> ScopedEntityTypes()
    {
        const string connectionString = "Host=localhost;Database=model-only;Username=model-only";
        var contexts = new DbContext[]
        {
            new ApplicationDBContext(Options<ApplicationDBContext>(connectionString)),
            new PlatformDbContext(Options<PlatformDbContext>(connectionString), Modules.Platform.Contracts.Tenancy.FixedOrganizationContext.None),
            new CatalogDbContext(Options<CatalogDbContext>(connectionString), TimeProvider.System),
            new IdentityAccessDbContext(Options<IdentityAccessDbContext>(connectionString), Modules.Platform.Contracts.Tenancy.FixedOrganizationContext.None),
            new ConfiguratorDbContext(Options<ConfiguratorDbContext>(connectionString), Modules.Platform.Contracts.Tenancy.FixedOrganizationContext.None),
        };
        try
        {
            return contexts.SelectMany(c => c.Model.GetEntityTypes())
                           .Where(e => e.ClrType == typeof(Organization) || e.FindProperty("OrganizationId") is not null)
                           .Select(e => e.ClrType)
                           .Distinct()
                           .ToList();
        }
        finally
        {
            foreach (var context in contexts)
            {
                context.Dispose();
            }
        }
    }

    private static DbContextOptions<TContext> Options<TContext>(string connectionString) where TContext : DbContext =>
        new DbContextOptionsBuilder<TContext>().UseNpgsql(connectionString).Options;
}
