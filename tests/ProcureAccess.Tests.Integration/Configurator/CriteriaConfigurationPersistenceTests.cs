// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Contracts.Configurations;
using ProcureAccess.Modules.Configurator.Domain;
using ProcureAccess.Modules.Configurator.Persistence;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Tenancy;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Configurator;

/// <summary>
/// The saved criteria lists in the database (S5-19, ARCHITECTURE §5.3): CAT-08 — the recommended
/// level survives an override, also across a save and a reload —, the rows stored and deleted with
/// their list, and <see cref="IConfigurationReader"/> behind the organisation filter (TEN-01).
/// </summary>
public sealed class CriteriaConfigurationPersistenceTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private Tenants? _tenants;

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        _tenants = Tenants.Build(Clock.GetUtcNow());
        await StoreAsync(_tenants.Entities);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact(DisplayName = "INV:CAT-08 " + nameof(Decide_OverrideOnAStoredList_KeepsTheResolvedLevelAfterSaveAndReload))]
    [Trait("Invariant", "CAT-08")]
    public async Task Decide_OverrideOnAStoredList_KeepsTheResolvedLevelAfterSaveAndReload()
    {
        var createdAt = Clock.GetUtcNow();
        var list = List(new(11, PriorityLevel.A), new(12, PriorityLevel.B), new(13, PriorityLevel.Info, OverrideLevel: PriorityLevel.A));
        await StoreAsync(list);
        var later = createdAt.AddMinutes(5);

        await using (var scope = ScopeOf(Fixture.A.Id))
        {
            var db = scope.ServiceProvider.GetRequiredService<ConfiguratorDbContext>();
            var stored = await db.CriteriaConfigurations.SingleAsync(c => c.Id == list.Id, Cancellation);
            stored.Decide(11, isIncluded: true, PriorityLevel.B, overrideWeight: 6, note: "Für uns ein Soll", later);
            stored.Decide(12, isIncluded: false, PriorityLevel.Info, overrideWeight: null, note: null, later);
            stored.Decide(13, isIncluded: true, overrideLevel: null, overrideWeight: null, note: null, later);
            await db.SaveChangesAsync(Cancellation);
        }

        await using var read = ScopeOf(Fixture.A.Id);
        var reloaded = await read.ServiceProvider.GetRequiredService<ConfiguratorDbContext>()
                                 .CriteriaConfigurations.AsNoTracking().SingleAsync(c => c.Id == list.Id, Cancellation);
        reloaded.Criteria.OrderBy(c => c.CriterionRevisionId)
                .Select(c => (c.CriterionRevisionId, c.ResolvedLevel, c.OverrideLevel, c.OverrideWeight, c.IsIncluded, c.NoteDe))
                .ShouldBe(
                [
                    (11, PriorityLevel.A, PriorityLevel.B, 6, true, "Für uns ein Soll"),
                    (12, PriorityLevel.B, PriorityLevel.Info, null, false, null),
                    (13, PriorityLevel.Info, null, null, true, null),
                ]);
        reloaded.CreatedAt.ShouldBe(createdAt);
        reloaded.ModifiedAt.ShouldBe(later);
    }

    [Fact]
    public async Task Reader_ListOfOrganizationA_IsReadByItsMembersOnly()
    {
        var list = List(new(11, PriorityLevel.A, OverrideLevel: PriorityLevel.B, OverrideWeight: 3, Note: "Notiz"), new(12, PriorityLevel.B, IsIncluded: false));
        await StoreAsync(list);

        var forMember = await FindAsync(list.Id, Fixture.A.Id);
        forMember.ShouldNotBeNull().ShouldSatisfyAllConditions(
            l => l.Id.ShouldBe(list.Id),
            l => l.Name.ShouldBe("Vergabe 2027"),
            l => l.ReleaseVersion.ShouldBe("T.1"),
            l => l.UseCaseKey.ShouldBe("hr"),
            l => l.AppTypeKeys.ShouldBe(["mobile", "web"], ignoreOrder: true),
            l => l.ProductPartKeys.ShouldBe(["base"]),
            l => l.TestTypeKeys.ShouldBe(["softwareTest"]),
            l => l.Criteria.OrderBy(c => c.CriterionRevisionId).ShouldBe(
            [
                new SavedCriterion(11, true, PriorityLevel.A, PriorityLevel.B, 3, "Notiz"),
                new SavedCriterion(12, false, PriorityLevel.B, null, null, null),
            ]));

        (await FindAsync(list.Id, Fixture.B.Id)).ShouldBeNull("a member of B does not read a list of A");
        (await FindAsync(list.Id)).ShouldBeNull("a request without organisation reads no list");
        (await FindAsync(Guid.NewGuid(), Fixture.A.Id)).ShouldBeNull();
    }

    [Fact]
    public async Task Delete_List_RemovesItsRows()
    {
        var list = List(new(11, PriorityLevel.A), new(12, PriorityLevel.B));
        await StoreAsync(list);
        (await RowCountAsync()).ShouldBe(1 + 2 + 1 + 1 + 2);

        await using (var scope = ScopeOf(Fixture.A.Id))
        {
            var db = scope.ServiceProvider.GetRequiredService<ConfiguratorDbContext>();
            db.Remove(await db.CriteriaConfigurations.SingleAsync(c => c.Id == list.Id, Cancellation));
            await db.SaveChangesAsync(Cancellation);
        }

        (await RowCountAsync()).ShouldBe(0);
    }

    private CriteriaConfiguration List(params ConfigurationCriterionDraft[] criteria) =>
        CriteriaConfiguration.Create(
            Fixture.A.Id, "Vergabe 2027", TestUsers.EditorA.Id, Clock.GetUtcNow(), "T.1", "hr", ["web", "mobile"], ["base"], ["softwareTest"], criteria);

    private async Task<SavedConfiguration?> FindAsync(Guid id, params Guid[] organizationIds)
    {
        await using var scope = ScopeOf(organizationIds);
        return await scope.ServiceProvider.GetRequiredService<IConfigurationReader>().FindAsync(id, Cancellation);
    }

    /// <summary>A scope wired like a request of a member of <paramref name="organizationIds"/>.</summary>
    private AsyncServiceScope ScopeOf(params Guid[] organizationIds)
    {
        var scope = Api.Factory.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<OrganizationContext>()
             .Load(organizationIds.Select(id => new KeyValuePair<Guid, OrganizationRole>(id, OrganizationRole.Editor)));
        return scope;
    }

    /// <summary>The rows of all five tables of the module.</summary>
    private async Task<long> RowCountAsync()
    {
        var counts = await DatabaseRowCounts.TakeAsync(Api.ConnectionString, [], Cancellation);
        return counts.Where(c => c.Key.StartsWith(ConfiguratorDbContext.Schema + ".", StringComparison.Ordinal)
                                 && !c.Key.EndsWith(ConfiguratorDbContext.HistoryTable, StringComparison.Ordinal))
                     .Sum(c => c.Value);
    }
}
