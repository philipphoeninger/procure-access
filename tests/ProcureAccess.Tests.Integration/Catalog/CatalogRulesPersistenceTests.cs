// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Catalog.Domain;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Modules.Catalog.Persistence.Configurations;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Catalog;

/// <summary>
/// The filter rules of S3-13 against the real <c>catalog</c> schema: a published release round-trips
/// with its exclusions, procedure mapping, applicability, priority rules and procedure texts, all on
/// the seeded filter values; the CHECK constraints refuse what the domain refuses (CAT-13, the test
/// type of a criterion, the allowed ratings); and a loaded revision keeps its rows (CAT-05).
/// </summary>
/// <remarks>
/// Releases are stored through the context that loaded the seeded values, so EF does not try to
/// insert them again. The constraint tests write SQL on purpose: the domain would refuse the rows
/// before the database sees them.
/// </remarks>
public sealed class CatalogRulesPersistenceTests(ApiFixture api) : IntegrationTest(api)
{
    [Fact]
    public async Task Store_PublishedReleaseWithRules_ReloadsEveryRow()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using (var scope = Api.Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var hr = await db.UseCaseTemplates.SingleAsync(u => u.Key == "hr", cancellationToken);
            var web = await db.AppTypes.SingleAsync(a => a.Key == "web", cancellationToken);
            var document = await db.AppTypes.SingleAsync(a => a.Key == "document", cancellationToken);
            var video = await db.ProductParts.SingleAsync(p => p.Key == "video", cancellationToken);
            var softwareTest = await db.TestTypes.SingleAsync(t => t.Key == "softwareTest", cancellationToken);
            var appTest = await db.TestTypes.SingleAsync(t => t.Key == "appTest", cancellationToken);

            var revision = CriterionRevision.Create(Criterion.Create("T-REQ-R1", CriterionKind.Requirement), 1, "Titel");
            revision.AddApplicability(web);
            revision.AddApplicability(web, video);
            revision.AddPriority(null, null, PriorityLevel.B, "Standard");
            revision.AddPriority(web, hr, PriorityLevel.A, "Für HR im Web", weight: 2);
            revision.AddProcedureText(
                softwareTest,
                new ProcedureTextContent(
                    "11.2.1.1",
                    "Originaltitel",
                    Rating.Fulfilled | Rating.SlightRestriction | Rating.NotApplicable,
                    "https://softwaretest.bit-inklusiv.de/#11.2.1.1",
                    ApplicabilityDe: "Anwendbarkeit",
                    NegativeExamplesDe: "Negativ"));
            var release = CatalogRelease.Create("T.R1");
            release.Include(revision);
            release.AddExclusion(FilterExclusion.Create(hr, document, "Passt nicht zum Verwendungszweck"));
            release.AddExclusion(FilterExclusion.Create(web, appTest, "Passt nicht zum Anwendungstyp"));
            release.MapProcedure(web, softwareTest);
            release.Publish(Clock.GetUtcNow());
            db.Releases.Add(release);
            await db.SaveChangesAsync(cancellationToken);
        }

        await using var readScope = Api.Factory.Services.CreateAsyncScope();
        var readDb = readScope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        var loaded = await readDb.Releases
            .Include(r => r.Exclusions).ThenInclude(e => e.Exclusion).ThenInclude(e => e.SourceUseCase)
            .Include(r => r.Exclusions).ThenInclude(e => e.Exclusion).ThenInclude(e => e.SourceAppType)
            .Include(r => r.Exclusions).ThenInclude(e => e.Exclusion).ThenInclude(e => e.TargetAppType)
            .Include(r => r.Exclusions).ThenInclude(e => e.Exclusion).ThenInclude(e => e.TargetTestType)
            .Include(r => r.Procedures).ThenInclude(p => p.AppType)
            .Include(r => r.Procedures).ThenInclude(p => p.TestType)
            .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.Criterion)
            .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.Applicability).ThenInclude(a => a.ProductPart)
            .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.Priorities).ThenInclude(p => p.AppType)
            .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.Priorities).ThenInclude(p => p.UseCase)
            .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.ProcedureTexts).ThenInclude(t => t.TestType)
            .AsSplitQuery()
            .SingleAsync(r => r.Version == "T.R1", cancellationToken);

        loaded.Exclusions.Select(e => (e.Exclusion.Source!.Key, e.Exclusion.Target!.Key))
            .ShouldBe([("hr", "document"), ("web", "appTest")], ignoreOrder: true);
        var procedure = loaded.Procedures.ShouldHaveSingleItem();
        (procedure.AppType.Key, procedure.TestType.Key).ShouldBe(("web", "softwareTest"));

        var revisionLoaded = loaded.Criteria.ShouldHaveSingleItem().Revision;
        revisionLoaded.Applicability.Select(a => a.ProductPart?.Key).ShouldBe([null, "video"], ignoreOrder: true);
        var specific = revisionLoaded.Priorities.Single(p => p.AppType is not null);
        (specific.AppType!.Key, specific.UseCase!.Key, specific.Level, specific.Weight, specific.RationaleDe)
            .ShouldBe(("web", "hr", PriorityLevel.A, (int?)2, "Für HR im Web"));
        var fallback = revisionLoaded.Priorities.Single(p => p.AppType is null);
        fallback.UseCase.ShouldBeNull();
        fallback.Weight.ShouldBeNull();
        (fallback.Level, fallback.Source).ShouldBe((PriorityLevel.B, PrioritySource.ExpertJudgement));
        var text = revisionLoaded.ProcedureTexts.ShouldHaveSingleItem();
        text.TestType.Key.ShouldBe("softwareTest");
        text.AllowedRatings.ShouldBe(Rating.Fulfilled | Rating.SlightRestriction | Rating.NotApplicable);
        (text.OriginalNumber, text.OriginalTitleDe, text.ApplicabilityDe, text.NegativeExamplesDe)
            .ShouldBe(("11.2.1.1", "Originaltitel", "Anwendbarkeit", "Negativ"));
        text.RequirementDe.ShouldBeNull();
        text.CreatedAt.ShouldBe(Clock.GetUtcNow());
    }

    [Fact(DisplayName = "INV:CAT-05 " + nameof(Load_StoredRevision_KeepsItsRulesAndRefusesNewOnes))]
    [Trait("Invariant", "CAT-05")]
    public async Task Load_StoredRevision_KeepsItsRulesAndRefusesNewOnes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using (var scope = Api.Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var revision = CriterionRevision.Create(Criterion.Create("T-REQ-R2", CriterionKind.Requirement), 1, "Titel");
            revision.AddApplicability(await db.AppTypes.SingleAsync(a => a.Key == "web", cancellationToken));
            var release = CatalogRelease.Create("T.R2");
            release.Include(revision);
            release.Publish(Clock.GetUtcNow());
            db.Releases.Add(release);
            await db.SaveChangesAsync(cancellationToken);
        }

        await using (var scope = Api.Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var release = await db.Releases
                .Include(r => r.Procedures)
                .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.Criterion)
                .Include(r => r.Criteria).ThenInclude(c => c.Revision).ThenInclude(r => r.Applicability).ThenInclude(a => a.AppType)
                .SingleAsync(r => r.Version == "T.R2", cancellationToken);
            var revision = release.Criteria.ShouldHaveSingleItem().Revision;
            var document = await db.AppTypes.SingleAsync(a => a.Key == "document", cancellationToken);
            var pdfTest = await db.TestTypes.SingleAsync(t => t.Key == "pdfTest", cancellationToken);

            Should.Throw<PublishedCatalogChangeException>(() => revision.AddApplicability(document));
            Should.Throw<PublishedCatalogChangeException>(() => revision.AddPriority(null, null, PriorityLevel.A, "Neu"));
            Should.Throw<PublishedCatalogChangeException>(() => release.MapProcedure(document, pdfTest));
            await db.SaveChangesAsync(cancellationToken);
        }

        await using (var scope = Api.Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            // The database is reset per class, so count only the rows of this test's release.
            (await db.Set<CriterionApplicability>().CountAsync(a => a.Revision.Criterion.Key == "T-REQ-R2", cancellationToken))
                .ShouldBe(1);
            (await db.Set<PriorityRecommendation>().CountAsync(p => p.Revision.Criterion.Key == "T-REQ-R2", cancellationToken))
                .ShouldBe(0);
            (await db.Set<CatalogReleaseAppTypeProcedure>().CountAsync(p => p.Release.Version == "T.R2", cancellationToken))
                .ShouldBe(0);
        }
    }

    [Theory(DisplayName = "INV:CAT-13 " + nameof(InsertExclusion_TargetLayerNotLower_IsRejectedByTheDatabase))]
    [Trait("Invariant", "CAT-13")]
    [InlineData("SourceAppTypeId", "TargetAppTypeId")]
    [InlineData("SourceProductPartId", "TargetAppTypeId")]
    [InlineData("SourceProductPartId", "TargetProductPartId")]
    public async Task InsertExclusion_TargetLayerNotLower_IsRejectedByTheDatabase(string sourceColumn, string targetColumn)
    {
        var error = await Should.ThrowAsync<PostgresException>(() => InsertExclusionAsync(sourceColumn, targetColumn));

        error.SqlState.ShouldBe(PostgresErrorCodes.CheckViolation);
        error.ConstraintName.ShouldBe(FilterExclusionConfiguration.TargetLayerLowerCheck);
    }

    [Theory(DisplayName = "INV:CAT-13 " + nameof(InsertExclusion_TargetLayerLower_IsAccepted))]
    [Trait("Invariant", "CAT-13")]
    [InlineData("SourceUseCaseId", "TargetAppTypeId")]
    [InlineData("SourceUseCaseId", "TargetProductPartId")]
    [InlineData("SourceUseCaseId", "TargetTestTypeId")]
    [InlineData("SourceAppTypeId", "TargetProductPartId")]
    [InlineData("SourceAppTypeId", "TargetTestTypeId")]
    [InlineData("SourceProductPartId", "TargetTestTypeId")]
    public async Task InsertExclusion_TargetLayerLower_IsAccepted(string sourceColumn, string targetColumn)
    {
        (await InsertExclusionAsync(sourceColumn, targetColumn)).ShouldBe(1);
    }

    [Fact]
    public async Task InsertExclusion_TwoSources_IsRejectedByTheDatabase()
    {
        var error = await Should.ThrowAsync<PostgresException>(() => ExecuteAsync(
            """
            INSERT INTO catalog."FilterExclusions" ("CreatedAt", "SourceUseCaseId", "SourceAppTypeId", "TargetTestTypeId", "ReasonDe")
            VALUES (now(), 1, 1, 1, 'Grund')
            """));

        error.ConstraintName.ShouldBe(FilterExclusionConfiguration.OneSourceOneTargetCheck);
    }

    [Theory]
    [InlineData("Requirement", "1")]
    [InlineData("TestProcedure", "NULL")]
    public async Task InsertCriterion_TestTypeNotMatchingKind_IsRejectedByTheDatabase(string kind, string testTypeId)
    {
        var error = await Should.ThrowAsync<PostgresException>(() => ExecuteAsync(
            $"""
            INSERT INTO catalog."Criteria" ("CreatedAt", "Key", "Kind", "TestTypeId")
            VALUES (now(), 'T-CHK-{kind}', '{kind}', {testTypeId})
            """));

        error.ConstraintName.ShouldBe(CriterionConfiguration.TestTypeOnlyForProcedureCheck);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(64)]
    [InlineData(1 | 64)]
    public async Task InsertProcedureText_AllowedRatingsEmptyOrUnknownBit_IsRejectedByTheDatabase(int allowedRatings)
    {
        var revisionId = await StoreRevisionAsync($"T-RAT-{allowedRatings}");

        var error = await Should.ThrowAsync<PostgresException>(() => ExecuteAsync(
            $"""
            INSERT INTO catalog."CriterionProcedureTexts"
                ("CreatedAt", "CriterionRevisionId", "TestTypeId", "OriginalNumber", "OriginalTitleDe", "AllowedRatings", "SourceUrl")
            VALUES (now(), {revisionId}, 1, '1.1', 'Titel', {allowedRatings}, 'https://example.org/')
            """));

        error.ConstraintName.ShouldBe(CriterionProcedureTextConfiguration.AllowedRatingsCheck);
    }

    [Theory]
    [InlineData("PriorityRecommendations", """ "AppTypeId", "UseCaseTemplateId", "Level", "RationaleDe", "Source" """, "1, NULL, 'A', 'Grund', 'ExpertJudgement'")]
    [InlineData("PriorityRecommendations", """ "AppTypeId", "UseCaseTemplateId", "Level", "RationaleDe", "Source" """, "NULL, NULL, 'B', 'Grund', 'ExpertJudgement'")]
    [InlineData("CriterionApplicability", """ "AppTypeId", "ProductPartId" """, "1, NULL")]
    public async Task InsertRule_SameRevisionAndValuesWithNull_IsRejectedAsDuplicate(string table, string columns, string values)
    {
        // „No app type“, „no use case“ and „no product part“ count as values of their own, so a
        // second catalog default or a second app-type-only row is a duplicate (nulls not distinct).
        var revisionId = await StoreRevisionAsync($"T-DUP-{table[0]}{values.Length}");
        var insert = $"""
            INSERT INTO catalog."{table}" ("CreatedAt", "CriterionRevisionId", {columns})
            VALUES (now(), {revisionId}, {values})
            """;
        await ExecuteAsync(insert);

        var error = await Should.ThrowAsync<PostgresException>(() => ExecuteAsync(insert));

        error.SqlState.ShouldBe(PostgresErrorCodes.UniqueViolation);
    }

    /// <summary>Stores a published release with one requirement revision and returns the revision's id.</summary>
    private async Task<int> StoreRevisionAsync(string key)
    {
        var revision = CriterionRevision.Create(Criterion.Create(key, CriterionKind.Requirement), 1, "Titel");
        var release = CatalogRelease.Create($"T.{key}");
        release.Include(revision);
        release.Publish(Clock.GetUtcNow());
        await StoreAsync(release);
        return revision.Id;
    }

    private Task<int> InsertExclusionAsync(string sourceColumn, string targetColumn) => ExecuteAsync(
        $"""
        INSERT INTO catalog."FilterExclusions" ("CreatedAt", "{sourceColumn}", "{targetColumn}", "ReasonDe")
        VALUES (now(), 1, 1, 'Grund')
        """);

    private async Task<int> ExecuteAsync(string sql)
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        // The SQL comes from constants and InlineData of this class only.
        return await db.Database.ExecuteSqlRawAsync(sql, TestContext.Current.CancellationToken);
    }
}
