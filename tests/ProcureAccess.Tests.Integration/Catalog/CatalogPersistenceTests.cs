// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Catalog;

/// <summary>
/// The core catalog entities against the real <c>catalog</c> schema (S3-12): a published release
/// round-trips with its revisions and standard references, the database stores published releases
/// only, and what EF loads is sealed (CAT-05; owner, S3-12 decision 1). The import itself and
/// CAT-05's import part follow in S3-16.
/// </summary>
public sealed class CatalogPersistenceTests(ApiFixture api) : IntegrationTest(api)
{
    [Fact]
    public async Task Store_PublishedRelease_ReloadsWithRevisionsReferencesAndCreationTime()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var reference = StandardReference.Create(Standard.En301549, "11.2.1.1", "https://example.org/en301549#11.2.1.1");
        var requirement = CriterionRevision.Create(
            Criterion.Create("T-REQ-1", CriterionKind.Requirement, "Basisfunktionalität"),
            1,
            "Titel der Anforderung",
            plainLanguageDe: "Einfache Sprache",
            rationaleDe: "Begründung",
            sortOrder: 10);
        requirement.AddStandardReference(reference);

        // The procedure criterion names a seeded test type, so the release is stored through the
        // context that loaded it (StoreAsync would insert the test type a second time).
        await using (var scope = Api.Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var pdfTest = await db.TestTypes.SingleAsync(t => t.Key == "pdfTest", cancellationToken);
            var procedure = CriterionRevision.Create(
                Criterion.Create("PROC-pdfTest", CriterionKind.TestProcedure, testType: pdfTest), 1, "Geprüft nach T");
            var release = CatalogRelease.Create("T.1", "Testversion");
            release.Include(requirement);
            release.Include(procedure);
            release.Publish(Clock.GetUtcNow());
            db.Releases.Add(release);
            await db.SaveChangesAsync(cancellationToken);
        }

        await using var readScope = Api.Factory.Services.CreateAsyncScope();
        var readDb = readScope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var loaded = await readDb.Releases.AsNoTracking()
            .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.Criterion)
            .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.StandardReferences)
            .SingleAsync(r => r.Version == "T.1", cancellationToken);

        loaded.PublishedAt.ShouldBe(Clock.GetUtcNow());
        loaded.CreatedAt.ShouldBe(Clock.GetUtcNow());
        loaded.ReleaseNotes.ShouldBe("Testversion");
        loaded.IsActive.ShouldBeFalse();
        loaded.Criteria.Select(c => c.Revision.Criterion.Key).ShouldBe(["PROC-pdfTest", "T-REQ-1"], ignoreOrder: true);

        var loadedRequirement = loaded.Criteria.Single(c => c.Revision.Criterion.Key == "T-REQ-1").Revision;
        loadedRequirement.Criterion.Kind.ShouldBe(CriterionKind.Requirement);
        loadedRequirement.Criterion.Category.ShouldBe("Basisfunktionalität");
        loadedRequirement.RevisionNumber.ShouldBe(1);
        loadedRequirement.TitleDe.ShouldBe("Titel der Anforderung");
        loadedRequirement.PlainLanguageDe.ShouldBe("Einfache Sprache");
        loadedRequirement.RationaleDe.ShouldBe("Begründung");
        loadedRequirement.SortOrder.ShouldBe(10);
        var loadedReference = loadedRequirement.StandardReferences.ShouldHaveSingleItem();
        loadedReference.Standard.ShouldBe(Standard.En301549);
        loadedReference.Clause.ShouldBe("11.2.1.1");
        loadedReference.Url.ShouldBe("https://example.org/en301549#11.2.1.1");
        var loadedProcedure = loaded.Criteria.Single(c => c.Revision.Criterion.Key == "PROC-pdfTest").Revision.Criterion;
        loadedProcedure.Kind.ShouldBe(CriterionKind.TestProcedure);
        loadedProcedure.TestTypeId.ShouldNotBeNull();
        loadedRequirement.Criterion.TestTypeId.ShouldBeNull();
    }

    [Fact(DisplayName = "INV:CAT-05 " + nameof(Load_StoredRelease_IsSealedAndOnlyActivationIsSaved))]
    [Trait("Invariant", "CAT-05")]
    public async Task Load_StoredRelease_IsSealedAndOnlyActivationIsSaved()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var release = CatalogRelease.Create("T.2");
        release.Include(CriterionRevision.Create(Criterion.Create("T-REQ-2", CriterionKind.Requirement), 1, "Titel"));
        release.Publish(Clock.GetUtcNow());
        await StoreAsync(release);

        await using (var scope = Api.Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var loaded = await db.Releases
                .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.Criterion)
                .SingleAsync(r => r.Version == "T.2", cancellationToken);
            var revision = loaded.Criteria.ShouldHaveSingleItem().Revision;

            revision.IsSealed.ShouldBeTrue();
            Should.Throw<PublishedCatalogChangeException>(
                () => revision.AddStandardReference(StandardReference.Create(Standard.Wcag, "1.4.3")));
            Should.Throw<PublishedCatalogChangeException>(
                () => loaded.Include(CriterionRevision.Create(Criterion.Create("T-REQ-3", CriterionKind.Requirement), 1, "Neu")));

            loaded.Activate();
            await db.SaveChangesAsync(cancellationToken);
        }

        await using (var scope = Api.Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            (await db.Releases.AsNoTracking().SingleAsync(r => r.Version == "T.2", cancellationToken)).IsActive.ShouldBeTrue();
            (await db.StandardReferences.CountAsync(r => r.Clause == "1.4.3", cancellationToken)).ShouldBe(0);
            (await db.Criteria.CountAsync(c => c.Key == "T-REQ-3", cancellationToken)).ShouldBe(0);
        }
    }

    [Fact(DisplayName = "INV:CAT-05 " + nameof(Store_UnpublishedRelease_IsRejectedByTheDatabase))]
    [Trait("Invariant", "CAT-05")]
    public async Task Store_UnpublishedRelease_IsRejectedByTheDatabase()
    {
        // The database holds published releases only (owner, S3-12 decision 1), so a release EF
        // loads is always published.
        var draft = CatalogRelease.Create("T.draft");
        draft.Include(CriterionRevision.Create(Criterion.Create("T-REQ-4", CriterionKind.Requirement), 1, "Titel"));

        await Should.ThrowAsync<DbUpdateException>(() => StoreAsync(draft));
    }
}
