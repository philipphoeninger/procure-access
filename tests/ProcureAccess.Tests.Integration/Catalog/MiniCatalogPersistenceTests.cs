// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using Keys = ProcureAccess.Tests.Shared.Fixtures.MiniCatalog.Keys;

namespace ProcureAccess.Tests.Integration.Catalog;

/// <summary>
/// The MiniCatalog (S3-14) against the real <c>catalog</c> schema: built on the seeded filter values
/// and stored through the context that loaded them, both releases round-trip without inserting a
/// filter value again, and the shared revisions are stored once. The in-memory values that unit
/// tests use agree with the seeded rows.
/// </summary>
public sealed class MiniCatalogPersistenceTests(ApiFixture api) : IntegrationTest(api)
{
    [Fact]
    public async Task Store_BothReleasesOnSeededValues_SharesRevisionsAndAddsNoFilterValue()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var valuesBefore = await CountFilterValuesAsync(cancellationToken);
        await using (var scope = Api.Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var catalog = MiniCatalog.Build(await MiniCatalogValues.LoadAsync(db, cancellationToken));
            db.Releases.AddRange(catalog.Releases);
            await db.SaveChangesAsync(cancellationToken);
        }

        await using var readScope = Api.Factory.Services.CreateAsyncScope();
        var readDb = readScope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var releases = await readDb.Releases
            .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.Criterion)
            .Include(r => r.Exclusions)
            .Include(r => r.Procedures)
            .AsSplitQuery()
            .OrderBy(r => r.Version)
            .ToListAsync(cancellationToken);

        releases.Select(r => (r.Version, r.Criteria.Count, r.Exclusions.Count, r.Procedures.Count, r.IsActive))
            .ShouldBe([(MiniCatalog.T1Version, 12, 6, 4, false), (MiniCatalog.T2Version, 11, 6, 3, false)]);
        releases.Select(r => r.PublishedAt).ShouldBe([MiniCatalog.T1PublishedAt, MiniCatalog.T2PublishedAt]);

        // T.2 brings four new revisions (CAT-07); the other seven are the rows of T.1.
        (await readDb.CriterionRevisions.CountAsync(cancellationToken)).ShouldBe(16);
        (await readDb.Criteria.CountAsync(cancellationToken)).ShouldBe(15);
        var t1Ids = releases[0].Criteria.Select(c => c.CriterionRevisionId).ToHashSet();
        releases[1].Criteria.Count(c => t1Ids.Contains(c.CriterionRevisionId)).ShouldBe(7);
        var changed = releases.Select(r => MiniCatalog.RevisionOf(r, Keys.Changed)).ToList();
        changed[1].CriterionId.ShouldBe(changed[0].CriterionId);
        changed.Select(r => r.RevisionNumber).ShouldBe([1, 2]);

        (await readDb.FilterExclusions.CountAsync(cancellationToken)).ShouldBe(7);
        (await readDb.StandardReferences.CountAsync(cancellationToken)).ShouldBe(1);
        (await CountFilterValuesAsync(cancellationToken)).ShouldBe(valuesBefore);
    }

    [Fact]
    public async Task InMemoryValues_MatchTheSeededRows()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var seeded = await MiniCatalogValues.LoadAsync(db, cancellationToken);
        var inMemory = MiniCatalogValues.InMemory();

        // Product-part explanations differ on purpose: the seeded ones quote BIT inklusiv.
        Describe(inMemory).ShouldBe(Describe(seeded));
        seeded.All.ShouldAllBe(v => v.Id > 0);
    }

    private static List<string> Describe(MiniCatalogValues values) =>
    [
        .. values.All.Select(value => value switch
        {
            UseCaseTemplate u => $"{u.Layer} {u.Key} {u.NameDe} active={u.IsActive}",
            AppType a => $"{a.Layer} {a.Key} {a.NameDe} {a.Functionality} {a.SortOrder} {a.HintDe}",
            ProductPart p => $"{p.Layer} {p.Key} {p.NameDe} {p.SortOrder} always={p.IsAlwaysSelected}",
            TestType t => $"{t.Layer} {t.Key} {t.NameDe} {t.SortOrder} {t.Url}",
            _ => throw new InvalidOperationException($"Unknown filter value {value.GetType().Name}."),
        }),
    ];

    private async Task<int> CountFilterValuesAsync(CancellationToken cancellationToken)
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        return await db.UseCaseTemplates.CountAsync(cancellationToken)
            + await db.AppTypes.CountAsync(cancellationToken)
            + await db.ProductParts.CountAsync(cancellationToken)
            + await db.TestTypes.CountAsync(cancellationToken);
    }
}
