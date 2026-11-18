// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Domain;
using ProcureAccess.Modules.Export.Documents;
using ProcureAccess.Modules.Export.Formatting;
using ProcureAccess.Modules.Export.Requests;
using ProcureAccess.Modules.Export.Sources;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Tenancy;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Tests.Integration.Configurator;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Export;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Export;

/// <summary>
/// The two export sources on the real catalog and database (S5-19; ARCHITECTURE §4.4 „One
/// formatter, two sources“): EXP-06 — a saved list and a draft with the same selection and
/// decisions give the same content and the same cells in both formats — and the stored part of
/// CAT-06: a saved list keeps the content of its own release after a newer one is activated. The
/// sources are called directly; the endpoint of the saved export follows with S5-21.
/// </summary>
public sealed class ExportSourceParityTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private Tenants? _tenants;

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        _tenants = Tenants.Build(Clock.GetUtcNow());
        // The accounts first: organisations and memberships reference them (foreign keys, S6-10b).
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => TestAccounts.Of(u, confirmed: true))]);
        await StoreAsync(_tenants.Entities);
        await ExportTestCatalog.ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact(DisplayName = "INV:EXP-06 " + nameof(Sources_SameSelectionAndDecisions_GiveIdenticalContentAndCells))]
    [Trait("Invariant", "EXP-06")]
    public async Task Sources_SameSelectionAndDecisions_GiveIdenticalContentAndCells()
    {
        var selection = await SavedLists.ResolveAsync(Api.Factory.Services, MiniCatalog.T1Version, Cancellation);
        var drafts = Decisions(selection);
        var list = await StoreListAsync(selection, drafts);

        await using var scope = ScopeOf(Fixture.A.Id);
        var persisted = await scope.ServiceProvider.GetRequiredService<PersistedConfigurationSource>().LoadAsync(list.Id, Cancellation);
        var adHoc = await scope.ServiceProvider.GetRequiredService<AdHocSelectionSource>().LoadAsync(RequestOf(drafts), Cancellation);

        persisted.Problem.ShouldBeNull();
        adHoc.Problem.ShouldBeNull();
        var (saved, draft) = (persisted.Content.ShouldNotBeNull(), adHoc.Content.ShouldNotBeNull());
        ShouldBeTheSame(saved, draft);

        // The decisions arrived: one criterion left out, one level and one weight chosen, one note.
        saved.Rows.Count.ShouldBe(selection.Criteria.Count - 1);
        saved.Rows.ShouldContain(r => r.ChosenLevel != null && r.Note == "=Mit Anbieter klären");
        saved.Rows.ShouldNotContain(r => r.Key == selection.Criteria[0].Key);

        var writer = scope.ServiceProvider.GetRequiredService<ExportFileWriter>();
        CsvReader.Read(writer.Write(saved, ExportFormat.Csv).Content)
                 .ShouldBe(CsvReader.Read(writer.Write(draft, ExportFormat.Csv).Content));
        CellsOf(writer.Write(saved, ExportFormat.Xlsx).Content).ShouldBe(CellsOf(writer.Write(draft, ExportFormat.Xlsx).Content));
    }

    [Fact(DisplayName = "INV:CAT-06 " + nameof(SavedList_AfterANewerReleaseIsActivated_KeepsTheContentOfItsOwnRelease))]
    [Trait("Invariant", "CAT-06")]
    public async Task SavedList_AfterANewerReleaseIsActivated_KeepsTheContentOfItsOwnRelease()
    {
        var selection = await SavedLists.ResolveAsync(Api.Factory.Services, MiniCatalog.T1Version, Cancellation);
        var drafts = Decisions(selection);
        var list = await StoreListAsync(selection, drafts);
        var before = await LoadSavedAsync(list.Id, Fixture.A.Id);

        var error = new StringWriter();
        (await CatalogModule.RunCommandAsync(
            Api.Factory.Services, ["catalog", "activate", MiniCatalog.T2Version], TextWriter.Null, error, Cancellation))
            .ShouldBe(0, error.ToString());

        var after = await LoadSavedAsync(list.Id, Fixture.A.Id);
        after.Problem.ShouldBeNull();
        after.Content.ShouldNotBeNull().ReleaseVersion.ShouldBe(MiniCatalog.T1Version);
        ShouldBeTheSame(after.Content, before.Content.ShouldNotBeNull());

        // The draft of the same release is refused by now: the anonymous export takes the active release only.
        await using var scope = ScopeOf(Fixture.A.Id);
        var adHoc = await scope.ServiceProvider.GetRequiredService<AdHocSelectionSource>().LoadAsync(RequestOf(drafts), Cancellation);
        adHoc.Problem.ShouldNotBeNull().Type.ShouldBe(ProblemTypes.ReleaseMismatch);
    }

    [Fact]
    public async Task SavedList_OfAnotherOrganization_IsNotFound()
    {
        var selection = await SavedLists.ResolveAsync(Api.Factory.Services, MiniCatalog.T1Version, Cancellation);
        var list = await StoreListAsync(selection, Decisions(selection));

        var result = await LoadSavedAsync(list.Id, Fixture.B.Id);

        result.Content.ShouldBeNull();
        result.Problem.ShouldNotBeNull().ShouldSatisfyAllConditions(p => p.Status.ShouldBe(404), p => p.Type.ShouldBe(ProblemTypes.NotFound));
    }

    /// <summary>
    /// One decision of every kind over the criteria of the selection: the first left out, the second
    /// with another level (a weight when that level is B) and a note that looks like a formula, a
    /// weight on every other B criterion, and the last with a „choice“ equal to its recommendation.
    /// </summary>
    private static List<ConfigurationCriterionDraft> Decisions(ResolvedCatalogSelection selection)
    {
        var criteria = selection.Criteria;
        criteria.Count.ShouldBeGreaterThan(3);
        return [.. criteria.Select((criterion, index) =>
        {
            var recommended = criterion.RecommendedPriority.Level;
            if (index == 0)
            {
                return SavedLists.Draft(criterion, isIncluded: false);
            }

            if (index == 1)
            {
                return recommended == PriorityLevel.B
                    ? SavedLists.Draft(criterion, overrideLevel: PriorityLevel.A, note: "=Mit Anbieter klären")
                    : SavedLists.Draft(criterion, overrideLevel: PriorityLevel.B, overrideWeight: 9, note: "=Mit Anbieter klären");
            }

            if (index == criteria.Count - 1)
            {
                return SavedLists.Draft(criterion, overrideLevel: recommended);
            }

            return recommended == PriorityLevel.B ? SavedLists.Draft(criterion, overrideWeight: 3) : SavedLists.Draft(criterion);
        })];
    }

    private async Task<CriteriaConfiguration> StoreListAsync(ResolvedCatalogSelection selection, List<ConfigurationCriterionDraft> drafts)
    {
        var byRevision = drafts.ToDictionary(d => d.CriterionRevisionId);
        var list = SavedLists.Build(
            Fixture.A.Id, TestUsers.EditorA.Id, "Vergabe 2027", Clock.GetUtcNow(), MiniCatalog.T1Version, selection,
            criterion => byRevision[criterion.RevisionId]);
        await StoreAsync(list);
        return list;
    }

    /// <summary>The draft the client would send for the same selection and decisions.</summary>
    private static ExportRequest RequestOf(List<ConfigurationCriterionDraft> drafts) => new(
        "xlsx",
        new ExportSelectionRequest(
            MiniCatalog.T1Version,
            SavedLists.WebSelection.UseCase,
            SavedLists.WebSelection.AppTypeIds,
            SavedLists.WebSelection.ProductPartIds,
            SavedLists.WebSelection.TestTypeIds),
        [.. drafts.Select(d => new ExportCriterionRequest(
            d.CriterionRevisionId, d.IsIncluded, d.OverrideLevel?.ToString(), d.OverrideWeight, d.Note))]);

    private async Task<ExportSourceResult> LoadSavedAsync(Guid listId, Guid organizationId)
    {
        await using var scope = ScopeOf(organizationId);
        return await scope.ServiceProvider.GetRequiredService<PersistedConfigurationSource>().LoadAsync(listId, Cancellation);
    }

    /// <summary>A scope wired like a request of a member of <paramref name="organizationId"/>.</summary>
    private AsyncServiceScope ScopeOf(Guid organizationId)
    {
        var scope = Api.Factory.Services.CreateAsyncScope();
        scope.ServiceProvider.GetRequiredService<OrganizationContext>()
             .Load([new KeyValuePair<Guid, OrganizationRole>(organizationId, OrganizationRole.Reader)]);
        return scope;
    }

    private static void ShouldBeTheSame(ExportContent actual, ExportContent expected)
    {
        actual.ReleaseVersion.ShouldBe(expected.ReleaseVersion);
        actual.UseCase.ShouldBe(expected.UseCase);
        actual.AppTypes.ShouldBe(expected.AppTypes);
        actual.ProductParts.ShouldBe(expected.ProductParts);
        actual.TestTypes.ShouldBe(expected.TestTypes);
        actual.Rows.ShouldBe(expected.Rows);
    }

    /// <summary>Every used cell of every sheet: sheet, address, text and hyperlink.</summary>
    private static List<string> CellsOf(byte[] file)
    {
        using var workbook = new XLWorkbook(new MemoryStream(file));
        return [.. workbook.Worksheets.SelectMany(sheet => sheet.CellsUsed().Select(cell =>
            $"{sheet.Name}!{cell.Address}={cell.GetString()}|{(cell.HasHyperlink ? cell.GetHyperlink().ExternalAddress?.ToString() : null)}"))];
    }
}
