// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Modules.Catalog.Releases;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Catalog;

/// <summary>
/// Release 2026.1, part 1 (S3-27, SPEC R-ADM-01): the folder <c>catalog/releases/2026.1/</c> holds
/// the BIT inklusiv texts of Kriterienkatalog Teil A/B without priorities and plain-language titles
/// (O-1, K-1). <c>catalog import</c> accepts it whole (CAT-23), and <c>catalog activate</c> refuses
/// it until the priorities are in the folder (O-S3-5 Q2 (iii)).
/// </summary>
/// <remarks>The csproj copies <c>catalog/releases/**</c> into the test output.</remarks>
public sealed class Release2026_1Tests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Version = "2026.1";

    // Teil A: 122 criteria (126 headings without the four parent steps), Teil B: 61, plus one
    // procedure criterion per seeded test type.
    private const int RequirementCriteria = 183;
    private const int ProcedureCriteria = 5;
    private const int Criteria = RequirementCriteria + ProcedureCriteria;

    private static readonly string ReleasesPath = Path.Combine(AppContext.BaseDirectory, "CatalogReleases");

    public async ValueTask InitializeAsync() => await Api.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact(DisplayName = "INV:CAT-23 " + nameof(Import_Release2026_1_StoresEveryCriterionPublishedAndInactive))]
    [Trait("Invariant", "CAT-23")]
    public async Task Import_Release2026_1_StoresEveryCriterionPublishedAndInactive()
    {
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await ImportAsync();

        result.Errors.Select(e => e.ToString()).ShouldBeEmpty();
        result.Summary.ShouldBe(new CatalogImportSummary(Version, Criteria, Criteria, Criteria, 0, null));
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var release = await db.Releases
            .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.Criterion)
            .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.Priorities)
            .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.ProcedureTexts)
            .Include(r => r.Exclusions)
            .Include(r => r.Procedures)
            .AsSplitQuery()
            .SingleAsync(cancellationToken);
        var criteria = release.Criteria.Select(c => c.Revision).ToList();

        release.IsActive.ShouldBeFalse();
        criteria.Count(r => r.Criterion.Kind == CriterionKind.Requirement).ShouldBe(RequirementCriteria);
        criteria.Where(r => r.Criterion.Kind == CriterionKind.TestProcedure)
            .Select(r => r.Criterion.Key)
            .ShouldBe(["PROC-softwareTest", "PROC-appTest", "PROC-pdfTest", "PROC-bikWeb", "PROC-bikApp"], ignoreOrder: true);
        criteria.ShouldAllBe(r => r.Priorities.Count == 0 && r.PlainLanguageDe == null && r.RationaleDe == null);
        // Teil A: 118 criteria with Software- and App-Test texts, 4 App-Test only; Teil B: 61 PDF texts.
        criteria.Sum(r => r.ProcedureTexts.Count).ShouldBe((118 * 2) + 4 + 61);
        release.Exclusions.Count.ShouldBe(7);
        release.Procedures.Count.ShouldBe(6);
    }

    [Fact(DisplayName = "INV:CAT-23 " + nameof(Activate_Release2026_1WithoutPriorities_IsRefusedForEveryCriterion))]
    [Trait("Invariant", "CAT-23")]
    public async Task Activate_Release2026_1WithoutPriorities_IsRefusedForEveryCriterion()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        (await ImportAsync()).Errors.ShouldBeEmpty();
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();

        var result = await new CatalogReleaseActivator(db).ActivateAsync(Version, cancellationToken);

        result.IsIncomplete.ShouldBeTrue();
        result.Errors.Count.ShouldBe(Criteria);
        result.Errors.ShouldAllBe(e => e.EndsWith("has no catalog-default priority (a rule without app type and use case).", StringComparison.Ordinal));
        (await db.Releases.AsNoTracking().CountAsync(r => r.IsActive, cancellationToken)).ShouldBe(0);
    }

    private async Task<CatalogImportResult> ImportAsync()
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        return await new CatalogReleaseImporter(db, Clock).ImportAsync(ReleasesPath, Version, TestContext.Current.CancellationToken);
    }
}
