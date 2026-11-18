// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Catalog;
using ProcureAccess.Modules.Configurator.Configurations;
using ProcureAccess.Modules.Configurator.Persistence;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Integration.Export.ExportTestCatalog;
using Keys = ProcureAccess.Tests.Shared.Fixtures.MiniCatalog.Keys;

namespace ProcureAccess.Tests.Integration.Configurator;

/// <summary>
/// Changing a saved list (S5-21a; SPEC R-SAV-03, R-SAV-06, R-LST-14, §8.10; ARCHITECTURE §7.2,
/// C155): a decision on one criterion, the selection („Auswahl ändern“), deleting, and the copy to
/// the active release (CAT-22). Every test starts from the Tenants fixture with the legacy accounts
/// of its users stored (all confirmed but Reader B), and the test releases with <c>T.1</c> active.
/// The authorisation matrix is <c>AuthorizationMatrixTests</c> (TEN-05).
/// </summary>
public sealed class SavedListChangeTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Path = "/api/v1/configurations";

    /// <summary>A <c>T.1</c> selection without use case, so a later use case changes recommendations.</summary>
    private static readonly JsonObject PlainWebSelection = new()
    {
        ["appTypeIds"] = new JsonArray("web"),
        ["productPartIds"] = new JsonArray("base"),
        ["testTypeIds"] = new JsonArray("softwareTest"),
    };

    /// <summary>
    /// HR with web, base and audio, no test type (audio excludes the software test). <c>T.2</c>
    /// excludes audio under HR, so a copy drops it.
    /// </summary>
    private static readonly JsonObject HrAudioSelection = new()
    {
        ["useCaseTemplateId"] = "hr",
        ["appTypeIds"] = new JsonArray("web"),
        ["productPartIds"] = new JsonArray("base", "audio"),
        ["testTypeIds"] = new JsonArray(),
    };

    private Tenants? _tenants;

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        await ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
        _tenants = Tenants.Build(Clock.GetUtcNow());
        // The accounts first: organisations and memberships reference them (foreign keys, S6-10b).
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => TestAccounts.Of(u, confirmed: u != TestUsers.ReaderB))]);
        await StoreAsync(_tenants.Entities);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact(DisplayName = "INV:CAT-08 " + nameof(Decide_EditorOfA_StoresTheDecisionKeepsTheRecommendationAndStampsTheChange))]
    [Trait("Invariant", "CAT-08")]
    public async Task Decide_EditorOfA_StoresTheDecisionKeepsTheRecommendationAndStampsTheChange()
    {
        var (id, criteria) = await SaveAsync(TestUsers.OwnerA, WebSelection);
        var revision = RevisionOf(criteria, Keys.DefaultOnly);
        Clock.Advance(TimeSpan.FromHours(1));
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PatchAsJsonAsync(
            CriterionPath(id, revision), new { isIncluded = true, overrideLevel = "B", overrideWeight = 8, note = "Mit Anbieter klären" }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent, await response.Content.ReadAsStringAsync(Cancellation));
        var list = await ReadAsync(client, id);
        list.ModifiedAt.ShouldBe(Clock.GetUtcNow());
        list.Criteria.Single(c => c.CriterionRevisionId == revision).ShouldSatisfyAllConditions(
            c => c.ResolvedLevel.ShouldBe("B"),
            c => c.OverrideLevel.ShouldBeNull(), // B is the recommendation: no choice of its own
            c => c.OverrideWeight.ShouldBe(8),
            c => c.Note.ShouldBe("Mit Anbieter klären"));

        using var excluded = await client.PatchAsJsonAsync(
            CriterionPath(id, revision), new { isIncluded = false, overrideLevel = "Info" }, Cancellation);

        excluded.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await ReadAsync(client, id)).Criteria.Single(c => c.CriterionRevisionId == revision).ShouldSatisfyAllConditions(
            c => c.IsIncluded.ShouldBeFalse(),
            c => c.ResolvedLevel.ShouldBe("B"),
            c => c.OverrideLevel.ShouldBe("Info"),
            c => c.OverrideWeight.ShouldBeNull(),
            c => c.Note.ShouldBeNull());
    }

    [Fact]
    public async Task Decide_InvalidValues_Returns400AtEachFieldAndChangesNothing()
    {
        var (id, criteria) = await SaveAsync(TestUsers.OwnerA, WebSelection);
        var revision = RevisionOf(criteria, Keys.DefaultOnly);
        using var client = CreateClient(TestUsers.EditorA);
        var before = await ReadRawAsync(client, id);

        using var response = await client.PatchAsJsonAsync(
            CriterionPath(id, revision),
            new { overrideLevel = "C", overrideWeight = 99, note = new string('n', 1001) },
            Cancellation);

        await ShouldBeProblemAsync(response, ProblemTypes.Validation, "isIncluded", "overrideLevel", "overrideWeight", "note");
        (await ReadRawAsync(client, id)).ShouldBe(before);
    }

    [Fact]
    public async Task Decide_WeightWhileTheLevelIsNotB_Returns400AtOverrideWeight()
    {
        var (id, criteria) = await SaveAsync(TestUsers.OwnerA, WebSelection);
        var recommendedA = criteria.First(c => c!["resolvedPriority"]!["level"]!.GetValue<string>() == "A")!["revisionId"]!.GetValue<int>();
        using var client = CreateClient(TestUsers.EditorA);

        using var withoutLevel = await client.PatchAsJsonAsync(
            CriterionPath(id, recommendedA), new { isIncluded = true, overrideWeight = 3 }, Cancellation);
        using var withB = await client.PatchAsJsonAsync(
            CriterionPath(id, recommendedA), new { isIncluded = true, overrideLevel = "B", overrideWeight = 3 }, Cancellation);

        await ShouldBeProblemAsync(withoutLevel, ProblemTypes.Validation, "overrideWeight");
        withB.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Decide_UnknownMember_Returns400()
    {
        var (id, criteria) = await SaveAsync(TestUsers.OwnerA, WebSelection);
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PatchAsJsonAsync(
            CriterionPath(id, RevisionOf(criteria, Keys.DefaultOnly)), new { isIncluded = true, resolvedLevel = "Info" }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Decide_CriterionNotOnTheList_Returns404()
    {
        var (id, _) = await SaveAsync(TestUsers.OwnerA, WebSelection);
        using var anonymous = CreateClient();
        var other = await CriteriaAsync(anonymous, MiniCatalog.T1Version, HrAudioSelection, Cancellation);
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PatchAsJsonAsync(
            CriterionPath(id, RevisionOf(other, Keys.AudioOnly)), new { isIncluded = false }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Decide_ListOfAnOlderRelease_Succeeds()
    {
        var (id, criteria) = await SaveAsync(TestUsers.OwnerA, WebSelection);
        await ActivateAsync(MiniCatalog.T2Version);
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PatchAsJsonAsync(
            CriterionPath(id, RevisionOf(criteria, Keys.DefaultOnly)), new { isIncluded = false }, Cancellation);

        // Owner decision S5-21 5a: the criteria of an older list stay editable; only its selection is fixed.
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Theory(DisplayName = "INV:TEN-01 " + nameof(Change_ListOfAnotherOrganization_Returns404AndChangesNothing))]
    [Trait("Invariant", "TEN-01")]
    [InlineData("decide")]
    [InlineData("selection")]
    [InlineData("delete")]
    [InlineData("copy")]
    public async Task Change_ListOfAnotherOrganization_Returns404AndChangesNothing(string action)
    {
        var (ofB, criteria) = await SaveAsync(TestUsers.OwnerB, WebSelection, organizationId: Fixture.B.Id);
        using var owner = CreateClient(TestUsers.OwnerB);
        var before = await ReadRawAsync(owner, ofB);
        using var client = CreateClient(TestUsers.OwnerA);
        // Naming B in a header changes nothing: the organisation comes from the stored list.
        client.DefaultRequestHeaders.Add("X-Organization-Id", Fixture.B.Id.ToString());

        using var response = await SendAsync(client, action, ofB, RevisionOf(criteria, Keys.DefaultOnly));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadRawAsync(owner, ofB)).ShouldBe(before);
        (await CountAsync()).ShouldBe(1);
    }

    [Theory(DisplayName = "INV:SEC-10 " + nameof(Change_UnconfirmedAddress_Returns403EmailUnconfirmedAndChangesNothing))]
    [Trait("Invariant", "SEC-10")]
    [InlineData("decide")]
    [InlineData("selection")]
    [InlineData("delete")]
    [InlineData("copy")]
    public async Task Change_UnconfirmedAddress_Returns403EmailUnconfirmedAndChangesNothing(string action)
    {
        var (id, criteria) = await SaveAsync(TestUsers.OwnerA, WebSelection);
        await StoreMembershipAsync(TestUsers.ReaderB, OrganizationRole.Editor);
        using var owner = CreateClient(TestUsers.OwnerA);
        var before = await ReadRawAsync(owner, id);
        using var client = CreateClient(TestUsers.ReaderB);

        using var response = await SendAsync(client, action, id, RevisionOf(criteria, Keys.DefaultOnly));

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await TypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.EmailUnconfirmed);
        (await ReadRawAsync(owner, id)).ShouldBe(before);
        (await CountAsync()).ShouldBe(1);
    }

    [Theory(DisplayName = "INV:SEC-10 " + nameof(Change_AfterConfirmingTheAddress_Succeeds))]
    [Trait("Invariant", "SEC-10")]
    [InlineData("decide", HttpStatusCode.NoContent)]
    [InlineData("selection", HttpStatusCode.OK)]
    [InlineData("delete", HttpStatusCode.NoContent)]
    [InlineData("copy", HttpStatusCode.Created)]
    public async Task Change_AfterConfirmingTheAddress_Succeeds(string action, HttpStatusCode expected)
    {
        var (id, criteria) = await SaveAsync(TestUsers.OwnerA, WebSelection);
        await StoreMembershipAsync(TestUsers.ReaderB, OrganizationRole.Editor);
        using var client = CreateClient(TestUsers.ReaderB);
        var revision = RevisionOf(criteria, Keys.DefaultOnly);
        (await SendAsync(client, action, id, revision)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        if (action == "copy")
        {
            await ActivateAsync(MiniCatalog.T2Version);
        }

        await TestAccounts.ConfirmAsync(Api.Factory.Services, TestUsers.ReaderB);
        using var response = await SendAsync(client, action, id, revision);

        response.StatusCode.ShouldBe(expected, await response.Content.ReadAsStringAsync(Cancellation));
    }

    [Fact]
    public async Task ChangeSelection_EditorOfA_KeepsTheDecisionsThatStayAndNamesTheOnesThatLeave()
    {
        var (id, criteria) = await SaveAsync(TestUsers.OwnerA, PlainWebSelection, (key, entry) =>
        {
            switch (key)
            {
                case Keys.PriorityAllLevels:
                    entry["overrideWeight"] = 7; // recommended B without a use case; A under HR
                    break;
                case Keys.DefaultOnly:
                    entry["note"] = "bleibt";
                    break;
                case Keys.ProcedureSoftwareTest:
                    entry["note"] = "fällt weg";
                    break;
            }
        });
        Clock.Advance(TimeSpan.FromHours(1));
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PutAsJsonAsync(SelectionPath(id), SelectionBody(HrAudioSelection), Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        var answer = (await response.Content.ReadFromJsonAsync<ChangedSelection>(Cancellation)).ShouldNotBeNull();
        answer.DroppedDecisions.Select(c => c.CriterionRevisionId).ShouldBe([RevisionOf(criteria, Keys.ProcedureSoftwareTest)]);
        answer.DroppedDecisions[0].ShouldSatisfyAllConditions(c => c.Number.ShouldBeNull(), c => c.Title.ShouldNotBeNullOrEmpty());

        var expected = await CriteriaAsync(client, MiniCatalog.T1Version, HrAudioSelection, Cancellation);
        var list = await ReadAsync(client, id);
        list.ModifiedAt.ShouldBe(Clock.GetUtcNow());
        list.Selection.ShouldSatisfyAllConditions(
            s => s.UseCaseTemplateId.ShouldBe("hr"),
            s => s.ProductPartIds.ShouldBe(["base", "audio"], ignoreOrder: true),
            s => s.TestTypeIds.ShouldBeEmpty());
        // Exactly the criteria of the new selection, each with the recommendation for it.
        list.Criteria.Select(c => (c.CriterionRevisionId, c.ResolvedLevel)).ShouldBe(
            expected.Select(c => (c!["revisionId"]!.GetValue<int>(), c["resolvedPriority"]!["level"]!.GetValue<string>()))
                    .OrderBy(p => p.Item1));
        var byRevision = list.Criteria.ToDictionary(c => c.CriterionRevisionId);
        byRevision[RevisionOf(expected, Keys.DefaultOnly)].Note.ShouldBe("bleibt");
        // Owner decision S5-21 a: the weight is kept as „B chosen“ now that the recommendation is A.
        byRevision[RevisionOf(expected, Keys.PriorityAllLevels)].ShouldSatisfyAllConditions(
            c => c.ResolvedLevel.ShouldBe("A"), c => c.OverrideLevel.ShouldBe("B"), c => c.OverrideWeight.ShouldBe(7));
        byRevision[RevisionOf(expected, Keys.AudioOnly)].ShouldSatisfyAllConditions(
            c => c.IsIncluded.ShouldBeTrue(), c => c.OverrideLevel.ShouldBeNull(), c => c.Note.ShouldBeNull());
    }

    [Fact]
    public async Task ChangeSelection_ListOfAnOlderRelease_Returns400ReleaseMismatch()
    {
        var (id, _) = await SaveAsync(TestUsers.OwnerA, WebSelection);
        await ActivateAsync(MiniCatalog.T2Version);
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PutAsJsonAsync(SelectionPath(id), SelectionBody(HrAudioSelection), Cancellation);

        await ShouldBeProblemAsync(response, ProblemTypes.ReleaseMismatch, "selection.releaseVersion");
    }

    [Fact]
    public async Task ChangeSelection_AnotherReleaseInTheBody_Returns400ReleaseMismatch()
    {
        var (id, _) = await SaveAsync(TestUsers.OwnerA, WebSelection);
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PutAsJsonAsync(
            SelectionPath(id), SelectionBody(HrAudioSelection, MiniCatalog.T2Version), Cancellation);

        await ShouldBeProblemAsync(response, ProblemTypes.ReleaseMismatch, "selection.releaseVersion");
    }

    [Fact(DisplayName = "INV:CAT-12 " + nameof(ChangeSelection_ExcludedValue_Returns400SelectionExcluded))]
    [Trait("Invariant", "CAT-12")]
    public async Task ChangeSelection_ExcludedValue_Returns400SelectionExcluded()
    {
        var (id, _) = await SaveAsync(TestUsers.OwnerA, WebSelection);
        using var client = CreateClient(TestUsers.EditorA);
        var selection = (JsonObject)HrAudioSelection.DeepClone();
        selection["productPartIds"] = new JsonArray("base", "video"); // HR excludes video in T.1

        using var response = await client.PutAsJsonAsync(SelectionPath(id), SelectionBody(selection), Cancellation);

        await ShouldBeProblemAsync(response, ProblemTypes.SelectionExcluded, "selection.productPartIds[1]");
    }

    [Fact(DisplayName = "INV:CAT-18 " + nameof(ChangeSelection_InactiveUseCase_Returns400AtTheUseCase))]
    [Trait("Invariant", "CAT-18")]
    public async Task ChangeSelection_InactiveUseCase_Returns400AtTheUseCase()
    {
        var (id, _) = await SaveAsync(TestUsers.OwnerA, WebSelection);
        using var client = CreateClient(TestUsers.EditorA);
        var selection = (JsonObject)PlainWebSelection.DeepClone();
        selection["useCaseTemplateId"] = "databaseApplication";
        selection["testTypeIds"] = new JsonArray();

        using var response = await client.PutAsJsonAsync(SelectionPath(id), SelectionBody(selection), Cancellation);

        await ShouldBeProblemAsync(response, ProblemTypes.Validation, "selection.useCaseTemplateId");
    }

    [Fact]
    public async Task Delete_EditorOfA_DeletesTheListAndLeavesTheOthers()
    {
        var (id, _) = await SaveAsync(TestUsers.OwnerA, WebSelection);
        var (other, _) = await SaveAsync(TestUsers.OwnerA, WebSelection, name: "Andere Liste");
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.DeleteAsync($"{Path}/{id}", Cancellation);
        using var again = await client.DeleteAsync($"{Path}/{id}", Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        again.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await client.GetAsync($"{Path}/{id}", Cancellation)).StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadAsync(client, other)).Name.ShouldBe("Andere Liste");
        await using var db = Configurator(Fixture.A.Id);
        (await db.CriteriaConfigurations.CountAsync(Cancellation)).ShouldBe(1);
    }

    [Fact(DisplayName = "INV:CAT-22 " + nameof(Copy_ListOfAnOlderRelease_CarriesTheSelectionAndDecisionsOverAndReportsTheRest))]
    [Trait("Invariant", "CAT-22")]
    public async Task Copy_ListOfAnOlderRelease_CarriesTheSelectionAndDecisionsOverAndReportsTheRest()
    {
        var (id, t1) = await SaveAsync(TestUsers.OwnerA, HrAudioSelection, (key, entry) =>
        {
            switch (key)
            {
                case Keys.PriorityAllLevels:
                    entry["overrideLevel"] = "Info"; // kept as it is
                    break;
                case Keys.Changed:
                    entry["overrideWeight"] = 7; // recommended B in T.1, A in T.2: kept as „B chosen“
                    break;
                case Keys.RenamedOld:
                    entry["note"] = "umbenannt"; // continues as 99.9
                    break;
                case Keys.MergedB:
                    entry["isIncluded"] = false; // continues as 99.10
                    break;
                case Keys.Removed:
                    entry["note"] = "entfällt"; // gone in T.2
                    break;
                case Keys.AudioOnly:
                    entry["overrideLevel"] = "Info"; // audio is dropped in T.2
                    break;
            }
        });
        using var owner = CreateClient(TestUsers.OwnerA);
        var before = await ReadRawAsync(owner, id);
        await ActivateAsync(MiniCatalog.T2Version);
        Clock.Advance(TimeSpan.FromHours(1));
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PostAsJsonAsync($"{Path}/{id}/copies", new { release = "active" }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(Cancellation));
        var copy = (await response.Content.ReadFromJsonAsync<CopiedConfiguration>(Cancellation)).ShouldNotBeNull();
        response.Headers.Location.ShouldNotBeNull().ToString().ShouldBe($"{Path}/{copy.Id}");
        copy.ShouldSatisfyAllConditions(
            c => c.Id.ShouldNotBe(id),
            c => c.ReleaseVersion.ShouldBe(MiniCatalog.T2Version),
            c => c.DroppedValues.ShouldBe(["Audio"]),
            c => c.NotCarried.Select(n => n.CriterionRevisionId).ShouldBe(
                [RevisionOf(t1, Keys.AudioOnly), RevisionOf(t1, Keys.Removed)], ignoreOrder: true),
            c => c.NotCarried.ShouldAllBe(n => !string.IsNullOrEmpty(n.Title)));

        var t2Selection = (JsonObject)HrAudioSelection.DeepClone();
        t2Selection["productPartIds"] = new JsonArray("base");
        var t2 = await CriteriaAsync(client, MiniCatalog.T2Version, t2Selection, Cancellation);
        var list = await ReadAsync(client, copy.Id);
        list.ShouldSatisfyAllConditions(
            l => l.OrganizationId.ShouldBe(Fixture.A.Id),
            l => l.Name.ShouldBe("Liste"),
            l => l.ReleaseVersion.ShouldBe(MiniCatalog.T2Version),
            l => l.CreatedAt.ShouldBe(Clock.GetUtcNow()),
            l => l.Selection.UseCaseTemplateId.ShouldBe("hr"),
            l => l.Selection.ProductPartIds.ShouldBe(["base"]),
            l => l.Selection.TestTypeIds.ShouldBeEmpty());
        list.Criteria.Select(c => (c.CriterionRevisionId, c.ResolvedLevel)).ShouldBe(
            t2.Select(c => (c!["revisionId"]!.GetValue<int>(), c["resolvedPriority"]!["level"]!.GetValue<string>()))
              .OrderBy(p => p.Item1));
        var byRevision = list.Criteria.ToDictionary(c => c.CriterionRevisionId);
        byRevision[RevisionOf(t2, Keys.PriorityAllLevels)].OverrideLevel.ShouldBe("Info");
        byRevision[RevisionOf(t2, Keys.Changed)].ShouldSatisfyAllConditions(
            c => c.ResolvedLevel.ShouldBe("A"), c => c.OverrideLevel.ShouldBe("B"), c => c.OverrideWeight.ShouldBe(7));
        byRevision[RevisionOf(t2, Keys.RenamedNew)].Note.ShouldBe("umbenannt");
        byRevision[RevisionOf(t2, Keys.Merged)].IsIncluded.ShouldBeFalse();
        byRevision.Values.Where(c => c.CriterionRevisionId != RevisionOf(t2, Keys.Merged)).ShouldAllBe(c => c.IsIncluded);

        // The source list is unchanged (ADR-002); the copy belongs to whoever made it.
        (await ReadRawAsync(owner, id)).ShouldBe(before);
        await using var db = Configurator(Fixture.A.Id);
        (await db.CriteriaConfigurations.SingleAsync(c => c.Id == copy.Id, Cancellation)).CreatedByUserId.ShouldBe(TestUsers.EditorA.Id);
    }

    [Fact]
    public async Task Copy_ListOnTheActiveRelease_Returns400AtRelease()
    {
        var (id, _) = await SaveAsync(TestUsers.OwnerA, WebSelection);
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PostAsJsonAsync($"{Path}/{id}/copies", new { release = "active" }, Cancellation);

        await ShouldBeProblemAsync(response, ProblemTypes.Validation, "release");
        (await CountAsync()).ShouldBe(1);
    }

    [Theory]
    [InlineData("T.2")]
    [InlineData(null)]
    public async Task Copy_ReleaseOtherThanActive_Returns400AtRelease(string? release)
    {
        var (id, _) = await SaveAsync(TestUsers.OwnerA, WebSelection);
        await ActivateAsync(MiniCatalog.T2Version);
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PostAsJsonAsync($"{Path}/{id}/copies", new { release }, Cancellation);

        await ShouldBeProblemAsync(response, ProblemTypes.Validation, "release");
        (await CountAsync()).ShouldBe(1);
    }

    /// <summary>The request of a change of the list <paramref name="id"/>, valid for an Editor of its organisation.</summary>
    private static Task<HttpResponseMessage> SendAsync(HttpClient client, string action, Guid id, int revision) => action switch
    {
        "decide" => client.PatchAsJsonAsync(CriterionPath(id, revision), new { isIncluded = false }, Cancellation),
        "selection" => client.PutAsJsonAsync(SelectionPath(id), SelectionBody(HrAudioSelection), Cancellation),
        "delete" => client.DeleteAsync($"{Path}/{id}", Cancellation),
        "copy" => client.PostAsJsonAsync($"{Path}/{id}/copies", new { release = "active" }, Cancellation),
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Unknown action."),
    };

    /// <summary>Saves the whole <c>T.1</c> draft of <paramref name="selection"/>; returns the id and the criteria answer.</summary>
    private async Task<(Guid Id, JsonArray Criteria)> SaveAsync(
        TestUser user,
        JsonObject selection,
        Action<string, JsonObject>? adjust = null,
        Guid? organizationId = null,
        string name = "Liste")
    {
        using var client = CreateClient(user);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, selection, Cancellation);
        var body = Body("xlsx", MiniCatalog.T1Version, selection, criteria, adjust);
        body.Remove("format");
        body["organizationId"] = organizationId ?? Fixture.A.Id;
        body["name"] = name;
        using var response = await client.PostAsJsonAsync(Path, body, Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(Cancellation));
        return ((await response.Content.ReadFromJsonAsync<CreatedConfiguration>(Cancellation)).ShouldNotBeNull().Id, criteria);
    }

    private static string CriterionPath(Guid id, int revision) => $"{Path}/{id}/criteria/{revision}";

    private static string SelectionPath(Guid id) => $"{Path}/{id}/selection";

    private static JsonObject SelectionBody(JsonObject selection, string release = MiniCatalog.T1Version)
    {
        var withRelease = (JsonObject)selection.DeepClone();
        withRelease["releaseVersion"] = release;
        return new JsonObject { ["selection"] = withRelease };
    }

    private static int RevisionOf(JsonArray criteria, string key) =>
        criteria.Single(c => c!["key"]!.GetValue<string>() == key)!["revisionId"]!.GetValue<int>();

    private static async Task<ConfigurationResponse> ReadAsync(HttpClient client, Guid id) =>
        (await client.GetFromJsonAsync<ConfigurationResponse>($"{Path}/{id}", Cancellation)).ShouldNotBeNull();

    /// <summary>The list as the server answers it, to compare before and after.</summary>
    private static async Task<string> ReadRawAsync(HttpClient client, Guid id)
    {
        using var response = await client.GetAsync($"{Path}/{id}", Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.Content.ReadAsStringAsync(Cancellation);
    }

    private async Task ActivateAsync(string version)
    {
        var error = new StringWriter();
        var exitCode = await CatalogModule.RunCommandAsync(
            Api.Factory.Services, ["catalog", "activate", version], TextWriter.Null, error, Cancellation);
        exitCode.ShouldBe(0, error.ToString());
    }

    private async Task StoreMembershipAsync(TestUser user, OrganizationRole role) =>
        await StoreAsync(OrganizationMembership.Create(Fixture.A.Id, user.Id, role, Clock.GetUtcNow(), Clock.GetUtcNow()));

    private async Task<int> CountAsync()
    {
        await using var db = Configurator(Fixture.A.Id, Fixture.B.Id);
        return await db.CriteriaConfigurations.CountAsync(Cancellation);
    }

    /// <summary>A context of its own outside any request that sees <paramref name="organizationIds"/>.</summary>
    private ConfiguratorDbContext Configurator(params Guid[] organizationIds) =>
        new(new DbContextOptionsBuilder<ConfiguratorDbContext>().UseNpgsql(Api.ConnectionString).Options,
            FixedOrganizationContext.Of(organizationIds));

    private static async Task ShouldBeProblemAsync(HttpResponseMessage response, string type, params string[] errorKeys)
    {
        var text = await response.Content.ReadAsStringAsync(Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, text);
        using var body = JsonDocument.Parse(text);
        body.RootElement.GetProperty("type").GetString().ShouldNotBeNull().ShouldEndWith("/problems/" + type);
        body.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name)
            .ShouldBe(errorKeys, ignoreOrder: true);
    }

    private static async Task<string> TypeOf(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        return body.RootElement.GetProperty("type").GetString()!;
    }
}
