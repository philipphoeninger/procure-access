// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Modules.Workspace.Assessments;
using ProcureAccess.Modules.Workspace.Candidates;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Modules.Workspace.Persistence;
using ProcureAccess.Modules.Workspace.Projects;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Integration.Export.ExportTestCatalog;
using static ProcureAccess.Tests.Integration.Workspace.ProjectLists;

namespace ProcureAccess.Tests.Integration.Workspace;

/// <summary>
/// The assessments of a candidate product (S6-18; SPEC R-CAN-06 – R-CAN-08; ARCHITECTURE §5.4,
/// §7.2, C30, C31, C34, C53): rows stored lazily, a value checked against the union of the list's
/// app types (CAT-15), procedure criteria derived and never stored or counted (CAT-17), progress
/// and exactly seven counts on the project and candidate views (SIG-13), the last write wins.
/// Every test starts from the Tenants fixture with the accounts of its users stored (all confirmed
/// but Reader B) and the test releases with <c>T.1</c> active; the project gets its list through
/// the API. The lock of the list is <c>ProjectCriteriaListTests</c> (WSP-02, WSP-03), the
/// authorisation matrix <c>AuthorizationMatrixTests</c> (TEN-05).
/// </summary>
public sealed class AssessmentEndpointsTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private static readonly string[] CountNames =
        ["fulfilled", "slightRestriction", "restriction", "barrier", "blockade", "notApplicable", "unknown"];

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

    [Fact]
    public async Task Read_CandidateWithoutAssessments_ListsTheIncludedRequirementCriteriaAsUnknownAndStoresNoRow()
    {
        using var client = CreateClient(TestUsers.ReaderA);
        var (candidate, criteria) = await CandidateWithListAsync(
            WebSelection, adjust: (key, entry) => entry["isIncluded"] = key != MiniCatalog.Keys.DefaultOnly);
        var expected = RequirementKeysOf(criteria).Where(k => k != MiniCatalog.Keys.DefaultOnly).ToList();

        var read = await ReadAsync(client, candidate.Id);

        read.ShouldSatisfyAllConditions(
            r => r.CandidateId.ShouldBe(candidate.Id),
            r => r.ReleaseVersion.ShouldBe(MiniCatalog.T1Version),
            r => r.RatingsFromSeveralProcedures.ShouldBeFalse(),
            r => r.Progress.ShouldBe(new AssessmentProgressResponse(0, expected.Count)),
            r => r.Criteria.Select(c => c.Number).ShouldBe(expected),
            r => r.Criteria.ShouldAllBe(c => c.Value == "unknown" && c.Note == null),
            r => r.ProcedureAnswers.ShouldHaveSingleItem().ShouldBe(new ProcedureAnswerResponse(
                RevisionOf(criteria, MiniCatalog.Keys.ProcedureSoftwareTest), "softwareTest", "Geprüft nach BIT inklusiv Software-Test", "notStated")));
        var first = read.Criteria.Single(c => c.Number == MiniCatalog.Keys.PriorityAllLevels);
        first.ShouldSatisfyAllConditions(
            c => c.CriterionRevisionId.ShouldBe(RevisionOf(criteria, MiniCatalog.Keys.PriorityAllLevels)),
            c => c.Title.ShouldBe("Testkriterium mit Regeln auf allen vier Ebenen"),
            c => c.AllowedRatings.ShouldBe(["fulfilled", "slightRestriction", "restriction", "barrier", "blockade"]));
        (await CountAssessmentsAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Read_ProjectWithoutList_AnswersAnEmptyTabAndNoCounts()
    {
        var (project, candidate) = await StoreCandidateAsync(Fixture.A.Id);
        using var client = CreateClient(TestUsers.ReaderA);

        var read = await ReadAsync(client, candidate.Id);

        read.ShouldSatisfyAllConditions(
            r => r.ReleaseVersion.ShouldBeNull(),
            r => r.Criteria.ShouldBeEmpty(),
            r => r.ProcedureAnswers.ShouldBeEmpty(),
            r => r.Progress.ShouldBe(new AssessmentProgressResponse(0, 0)));
        (await ReadCandidateAsync(client, candidate.Id)).ShouldSatisfyAllConditions(
            c => c.Progress.ShouldBeNull(), c => c.Fulfillment.ShouldBeNull());
        (await ReadProjectAsync(client, project.Id)).Candidates.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            c => c.Progress.ShouldBeNull(), c => c.Fulfillment.ShouldBeNull(), c => c.ProcedureAnswers.ShouldBeNull());
    }

    /// <summary>
    /// CAT-15 on the stored catalog: <c>99.1</c> has a Software-Test text without „Nicht anwendbar“
    /// and an App-Test text without the two restriction levels; <c>99.2</c> has no text; <c>99.6</c>
    /// allows only „Erfüllt“ and „Blockade“. Web is tested by the Software-Test, Mobile by both.
    /// </summary>
    [Theory(DisplayName = "INV:CAT-15 " + nameof(Set_Value_IsCheckedAgainstTheUnionOverTheListsAppTypes))]
    [Trait("Invariant", "CAT-15")]
    [InlineData(false, MiniCatalog.Keys.PriorityAllLevels, "slightRestriction", true)]
    [InlineData(false, MiniCatalog.Keys.PriorityAllLevels, "notApplicable", false)]
    [InlineData(true, MiniCatalog.Keys.PriorityAllLevels, "notApplicable", true)]
    [InlineData(true, MiniCatalog.Keys.PriorityAllLevels, "slightRestriction", true)]
    [InlineData(false, MiniCatalog.Keys.PriorityAppTypes, "notApplicable", true)]
    [InlineData(false, MiniCatalog.Keys.Changed, "blockade", true)]
    [InlineData(false, MiniCatalog.Keys.Changed, "restriction", false)]
    [InlineData(false, MiniCatalog.Keys.Changed, "unknown", true)]
    public async Task Set_Value_IsCheckedAgainstTheUnionOverTheListsAppTypes(bool webAndMobile, string key, string value, bool allowed)
    {
        var (candidate, criteria) = await CandidateWithListAsync(webAndMobile ? WebAndMobileSelection : WebSelection);
        using var client = CreateClient(TestUsers.EditorA);
        var revision = RevisionOf(criteria, key);

        using var response = await SetAssessmentAsync(client, candidate.Id, revision, value, Cancellation);

        var read = await ReadAsync(client, candidate.Id);
        read.RatingsFromSeveralProcedures.ShouldBe(webAndMobile);
        if (allowed)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
            (await response.Content.ReadFromJsonAsync<AssessmentResponse>(Cancellation)).ShouldBe(new AssessmentResponse(revision, value, null));
            read.Criteria.Single(c => c.CriterionRevisionId == revision).Value.ShouldBe(value);
        }
        else
        {
            await ShouldBeValidationProblemAsync(response, "value");
            read.Criteria.Single(c => c.CriterionRevisionId == revision).AllowedRatings.ShouldNotContain(value);
            (await CountAssessmentsAsync()).ShouldBe(0);
        }
    }

    [Fact(DisplayName = "INV:CAT-17 " + nameof(Set_ProcedureCriterion_Returns400AtTheCriterionAndStoresNoRow))]
    [Trait("Invariant", "CAT-17")]
    public async Task Set_ProcedureCriterion_Returns400AtTheCriterionAndStoresNoRow()
    {
        var (candidate, criteria) = await CandidateWithListAsync(WebSelection);
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await SetAssessmentAsync(
            client, candidate.Id, RevisionOf(criteria, MiniCatalog.Keys.ProcedureSoftwareTest), "fulfilled", Cancellation);

        await ShouldBeValidationProblemAsync(response, "criterionRevisionId");
        (await CountAssessmentsAsync()).ShouldBe(0);
    }

    [Theory(DisplayName = "INV:CAT-17 " + nameof(ProcedureAnswer_Derived_FollowsTheEvidencesAndTheTestStatusAndIsNeverCounted))]
    [Trait("Invariant", "CAT-17")]
    [InlineData(nameof(TestStatus.Unknown), null, "notStated")]
    [InlineData(nameof(TestStatus.NotTested), null, "no")]
    [InlineData(nameof(TestStatus.Tested), "softwareTest", "yes")]
    [InlineData(nameof(TestStatus.Tested), "bikWeb", "notStated")]
    public async Task ProcedureAnswer_Derived_FollowsTheEvidencesAndTheTestStatusAndIsNeverCounted(
        string status, string? evidenceOf, string expected)
    {
        var (candidate, criteria) = await CandidateWithListAsync(WebSelection, Enum.Parse<TestStatus>(status), evidenceOf);
        using var client = CreateClient(TestUsers.ReaderA);
        var requirements = RequirementKeysOf(criteria).Count;

        var tab = await ReadAsync(client, candidate.Id);
        var inProject = (await ReadProjectAsync(client, candidate.ProcurementProjectId)).Candidates.ShouldHaveSingleItem();

        tab.ProcedureAnswers.ShouldHaveSingleItem().Answer.ShouldBe(expected);
        inProject.ProcedureAnswers.ShouldBe(tab.ProcedureAnswers);
        // The list has one more criterion, „Geprüft nach …“: neither the progress nor a count has it.
        criteria.Count.ShouldBe(requirements + 1);
        tab.Progress.ShouldBe(new AssessmentProgressResponse(0, requirements));
        inProject.Fulfillment.ShouldBe(new FulfillmentCountsResponse(0, 0, 0, 0, 0, 0, requirements));
        (await CountAssessmentsAsync()).ShouldBe(0);
    }

    [Fact(DisplayName = "INV:SIG-13 " + nameof(Views_OfProjectAndCandidate_CarryExactlySevenCountsOfTheIncludedRequirementCriteria))]
    [Trait("Invariant", "SIG-13")]
    public async Task Views_OfProjectAndCandidate_CarryExactlySevenCountsOfTheIncludedRequirementCriteria()
    {
        var (candidate, criteria) = await CandidateWithListAsync(
            WebSelection, adjust: (key, entry) => entry["isIncluded"] = key != MiniCatalog.Keys.Removed);
        using var client = CreateClient(TestUsers.EditorA);
        var total = RequirementKeysOf(criteria).Count - 1;
        await SetAndCheckAsync(client, candidate.Id, RevisionOf(criteria, MiniCatalog.Keys.PriorityAllLevels), "fulfilled");
        await SetAndCheckAsync(client, candidate.Id, RevisionOf(criteria, MiniCatalog.Keys.PriorityAppTypes), "notApplicable");
        await SetAndCheckAsync(client, candidate.Id, RevisionOf(criteria, MiniCatalog.Keys.DefaultOnly), "barrier");
        await SetAndCheckAsync(client, candidate.Id, RevisionOf(criteria, MiniCatalog.Keys.Changed), "unknown", "Noch offen");

        var project = await ReadJsonAsync(client, $"/api/v1/projects/{candidate.ProcurementProjectId}");
        var read = await ReadJsonAsync(client, $"/api/v1/candidates/{candidate.Id}");

        var expected = new FulfillmentCountsResponse(1, 0, 0, 1, 0, 1, total - 3);
        var inProject = project["candidates"]!.AsArray().ShouldHaveSingleItem()!.AsObject();
        foreach (var view in new[] { inProject, read })
        {
            var counts = view["fulfillment"]!.AsObject();
            counts.Select(p => p.Key).ShouldBe(CountNames);
            counts.Deserialize<FulfillmentCountsResponse>(JsonSerializerOptions.Web).ShouldBe(expected);
            counts.Sum(p => p.Value!.GetValue<int>()).ShouldBe(total);
            view["progress"].Deserialize<AssessmentProgressResponse>(JsonSerializerOptions.Web).ShouldBe(new AssessmentProgressResponse(3, total));
        }

        // Pinned to the release of the project's list (C34): nothing can be „not covered“.
        project["criteriaList"]!["releaseVersion"]!.GetValue<string>().ShouldBe(MiniCatalog.T1Version);
        (await ReadAsync(client, candidate.Id)).ReleaseVersion.ShouldBe(MiniCatalog.T1Version);
        foreach (var text in new[] { project.ToJsonString(), read.ToJsonString(), (await ReadJsonAsync(client, AssessmentsPath(candidate.Id))).ToJsonString() })
        {
            text.ShouldNotContain("notCovered", Case.Insensitive);
        }
    }

    [Fact(DisplayName = "INV:API-03 " + nameof(Set_WithoutIfMatch_SucceedsAndLeavesTheTagsOfCandidateAndProjectValid))]
    [Trait("Invariant", "API-03")]
    public async Task Set_WithoutIfMatch_SucceedsAndLeavesTheTagsOfCandidateAndProjectValid()
    {
        var (candidate, criteria) = await CandidateWithListAsync(WebSelection);
        using var editor = CreateClient(TestUsers.EditorA);
        using var owner = CreateClient(TestUsers.OwnerA);
        var candidatePath = $"/api/v1/candidates/{candidate.Id}";
        var projectPath = $"/api/v1/projects/{candidate.ProcurementProjectId}";
        // The editor has S33 and S32 open …
        var candidateTag = await editor.TagOfAsync(candidatePath, Cancellation);
        var projectTag = await editor.TagOfAsync(projectPath, Cancellation);

        // … while a colleague assesses: the last write wins per row, without If-Match (R-CAN-11).
        await SetAndCheckAsync(owner, candidate.Id, RevisionOf(criteria, MiniCatalog.Keys.DefaultOnly), "barrier", "Notiz");

        (await editor.TagOfAsync(candidatePath, Cancellation)).ShouldBe(candidateTag);
        using var change = await editor.SendWithTagAsync(HttpMethod.Patch, candidatePath, new { name = "Umbenannt" }, candidateTag, Cancellation);
        using var rename = await editor.SendWithTagAsync(HttpMethod.Patch, projectPath, new { name = "Umbenannt" }, projectTag, Cancellation);
        change.StatusCode.ShouldBe(HttpStatusCode.OK, await change.Content.ReadAsStringAsync(Cancellation));
        rename.StatusCode.ShouldBe(HttpStatusCode.OK, await rename.Content.ReadAsStringAsync(Cancellation));
    }

    [Fact]
    public async Task Set_ValueNoteAndBackToNothing_StoresOneRowLazilyTheLastWriteWinsAndTheLastChangeMoves()
    {
        var (candidate, criteria) = await CandidateWithListAsync(WebSelection);
        using var editor = CreateClient(TestUsers.EditorA);
        using var owner = CreateClient(TestUsers.OwnerA);
        var revision = RevisionOf(criteria, MiniCatalog.Keys.DefaultOnly);
        var created = Clock.GetUtcNow();
        Clock.Advance(TimeSpan.FromHours(1));

        // Nothing to store: no row, no change.
        await SetAndCheckAsync(editor, candidate.Id, revision, "unknown", "  ");
        (await CountAssessmentsAsync()).ShouldBe(0);
        (await ReadCandidateAsync(editor, candidate.Id)).ModifiedAt.ShouldBe(created);

        await SetAndCheckAsync(editor, candidate.Id, revision, "restriction", "  Prüfbericht vom Anbieter ");
        // No If-Match: the second editor's value replaces the first (R-CAN-11).
        await SetAndCheckAsync(owner, candidate.Id, revision, "blockade", "Eigener Test");

        (await ReadAsync(editor, candidate.Id)).Criteria.Single(c => c.CriterionRevisionId == revision).ShouldSatisfyAllConditions(
            c => c.Value.ShouldBe("blockade"), c => c.Note.ShouldBe("Eigener Test"));
        (await CountAssessmentsAsync()).ShouldBe(1);
        (await ReadCandidateAsync(editor, candidate.Id)).ModifiedAt.ShouldBe(Clock.GetUtcNow());
        (await ReadProjectAsync(editor, candidate.ProcurementProjectId)).ModifiedAt.ShouldBe(Clock.GetUtcNow());

        // A note without a value keeps the row and is no assessment; without both the row goes.
        await SetAndCheckAsync(editor, candidate.Id, revision, "unknown", "Eigener Test");
        (await CountAssessmentsAsync()).ShouldBe(1);
        (await ReadAsync(editor, candidate.Id)).Progress.Done.ShouldBe(0);
        await SetAndCheckAsync(editor, candidate.Id, revision, "unknown");
        (await CountAssessmentsAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Set_TwoFirstWritesOfOneRowAtOnce_BothSucceedAndOneRowStays()
    {
        var (candidate, criteria) = await CandidateWithListAsync(WebSelection);
        // Two browsers: each client fetches its own antiforgery token.
        using var editor = CreateClient(TestUsers.EditorA);
        using var owner = CreateClient(TestUsers.OwnerA);
        var revision = RevisionOf(criteria, MiniCatalog.Keys.DefaultOnly);

        var responses = await Task.WhenAll(
            SetAssessmentAsync(editor, candidate.Id, revision, "fulfilled", Cancellation),
            SetAssessmentAsync(owner, candidate.Id, revision, "barrier", Cancellation));

        foreach (var response in responses)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
            response.Dispose();
        }

        (await CountAssessmentsAsync()).ShouldBe(1);
        (await ReadAsync(editor, candidate.Id)).Criteria.Single(c => c.CriterionRevisionId == revision).Value
            .ShouldBeOneOf("fulfilled", "barrier");
    }

    [Theory(DisplayName = "INV:API-01 " + nameof(Set_InvalidBody_Returns400AtTheFieldAndStoresNoRow))]
    [Trait("Invariant", "API-01")]
    [InlineData(null, null, "value")]
    [InlineData("Fulfilled", null, "value")]
    [InlineData("erfüllt", null, "value")]
    [InlineData("fulfilled", "long", "note")]
    public async Task Set_InvalidBody_Returns400AtTheFieldAndStoresNoRow(string? value, string? note, string field)
    {
        var (candidate, criteria) = await CandidateWithListAsync(WebSelection);
        using var client = CreateClient(TestUsers.EditorA);
        var revision = RevisionOf(criteria, MiniCatalog.Keys.DefaultOnly);
        var longNote = note == "long" ? new string('n', CandidateAssessment.EvidenceNoteMaxLength + 1) : note;

        using var response = await SetAssessmentAsync(client, candidate.Id, revision, value, Cancellation, longNote);
        using var atTheLimit = await SetAssessmentAsync(
            client, candidate.Id, revision, "fulfilled", Cancellation, new string('n', CandidateAssessment.EvidenceNoteMaxLength));
        using var unknownMember = await client.PutAsJsonAsync(
            $"{AssessmentsPath(candidate.Id)}/{revision}", new { value = "barrier", score = 3 }, Cancellation);

        await ShouldBeValidationProblemAsync(response, field);
        atTheLimit.StatusCode.ShouldBe(HttpStatusCode.OK);
        unknownMember.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ReadAsync(client, candidate.Id)).Criteria.Single(c => c.CriterionRevisionId == revision).Value.ShouldBe("fulfilled");
    }

    [Fact]
    public async Task Set_CriterionThatIsNotAnIncludedOneOfTheList_Returns400AtTheCriterion()
    {
        var (candidate, criteria) = await CandidateWithListAsync(
            WebSelection, adjust: (key, entry) => entry["isIncluded"] = key != MiniCatalog.Keys.DefaultOnly);
        var (_, withoutList) = await StoreCandidateAsync(Fixture.A.Id);
        using var client = CreateClient(TestUsers.EditorA);
        var excluded = RevisionOf(criteria, MiniCatalog.Keys.DefaultOnly);
        var included = RevisionOf(criteria, MiniCatalog.Keys.PriorityAllLevels);

        HttpResponseMessage[] responses =
        [
            await SetAssessmentAsync(client, candidate.Id, excluded, "fulfilled", Cancellation),
            await SetAssessmentAsync(client, candidate.Id, int.MaxValue, "fulfilled", Cancellation),
            await SetAssessmentAsync(client, withoutList.Id, included, "fulfilled", Cancellation),
        ];

        foreach (var response in responses)
        {
            await ShouldBeValidationProblemAsync(response, "criterionRevisionId");
            response.Dispose();
        }

        (await CountAssessmentsAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Set_ArchivedProject_Returns409ProjectArchivedAndKeepsTheAssessment()
    {
        var (candidate, criteria) = await CandidateWithListAsync(WebSelection);
        using var client = CreateClient(TestUsers.OwnerA);
        var revision = RevisionOf(criteria, MiniCatalog.Keys.DefaultOnly);
        await SetAndCheckAsync(client, candidate.Id, revision, "fulfilled");
        (await client.PatchCurrentAsync($"/api/v1/projects/{candidate.ProcurementProjectId}", new { status = "archived" }, Cancellation))
            .StatusCode.ShouldBe(HttpStatusCode.OK);

        using var response = await SetAssessmentAsync(client, candidate.Id, revision, "blockade", Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await TypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.ProjectArchived);
        // The archive is read-only, not empty: the assessment is still read.
        (await ReadAsync(client, candidate.Id)).Criteria.Single(c => c.CriterionRevisionId == revision).Value.ShouldBe("fulfilled");
    }

    [Fact(DisplayName = "INV:SEC-10 " + nameof(Set_UnconfirmedAddress_Returns403EmailUnconfirmedAndStoresNoRow))]
    [Trait("Invariant", "SEC-10")]
    public async Task Set_UnconfirmedAddress_Returns403EmailUnconfirmedAndStoresNoRow()
    {
        await StoreAsync(OrganizationMembership.Create(Fixture.A.Id, TestUsers.ReaderB.Id, OrganizationRole.Editor, Clock.GetUtcNow(), Clock.GetUtcNow()));
        var (candidate, criteria) = await CandidateWithListAsync(WebSelection);
        using var client = CreateClient(TestUsers.ReaderB);

        using var response = await SetAssessmentAsync(
            client, candidate.Id, RevisionOf(criteria, MiniCatalog.Keys.DefaultOnly), "fulfilled", Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await TypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.EmailUnconfirmed);
        (await CountAssessmentsAsync()).ShouldBe(0);
    }

    [Fact(DisplayName = "INV:TEN-01 " + nameof(Requests_CandidateOfAnotherOrganization_Return404AndStoreNoRow))]
    [Trait("Invariant", "TEN-01")]
    public async Task Requests_CandidateOfAnotherOrganization_Return404AndStoreNoRow()
    {
        var (candidate, criteria) = await CandidateWithListAsync(WebSelection);
        using var client = CreateClient(TestUsers.OwnerB);

        using var read = await client.GetAsync(AssessmentsPath(candidate.Id), Cancellation);
        using var set = await SetAssessmentAsync(
            client, candidate.Id, RevisionOf(criteria, MiniCatalog.Keys.DefaultOnly), "fulfilled", Cancellation);

        read.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        set.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await CountAssessmentsAsync()).ShouldBe(0);
    }

    /// <summary>
    /// A project of A with a candidate and its own copy of a list of <paramref name="selection"/>;
    /// with <paramref name="evidenceOf"/> the candidate has an evidence of that procedure.
    /// </summary>
    private async Task<(CandidateProduct Candidate, JsonArray Criteria)> CandidateWithListAsync(
        JsonObject selection,
        TestStatus status = TestStatus.Unknown,
        string? evidenceOf = null,
        Action<string, JsonObject>? adjust = null)
    {
        var (project, candidate) = await StoreCandidateAsync(Fixture.A.Id, status, evidenceOf);
        using var owner = CreateClient(TestUsers.OwnerA);
        var (listId, criteria) = await SaveAsync(owner, Fixture.A.Id, selection, Cancellation, adjust);
        await AssignAsync(owner, project.Id, listId, Cancellation);
        return (candidate, criteria);
    }

    private async Task<(ProcurementProject Project, CandidateProduct Candidate)> StoreCandidateAsync(
        Guid organizationId, TestStatus status = TestStatus.Unknown, string? evidenceOf = null)
    {
        var now = Clock.GetUtcNow();
        var project = ProcurementProject.Create(organizationId, "Projekt", TestUsers.OwnerA.Id, now);
        var candidate = CandidateProduct.Create(project, new CandidateFacts("Produkt", null, null, null), now);
        candidate.ChangeTestStatus(status, removeEvidences: false, now);
        var rows = new List<object> { project, candidate, CandidatePrivateSection.Of(candidate, null, null) };
        if (evidenceOf is not null)
        {
            var evidence = candidate.AddEvidence(
                new TestEvidenceFacts(TestKind.Procedure, evidenceOf, null, "Prüfstelle", new DateOnly(2026, 1, 15), null, null), now).Evidence!;
            rows.Add(TestEvidencePrivateNote.Of(evidence, null));
        }

        await StoreAsync([.. rows]);
        return (project, candidate);
    }

    private static async Task SetAndCheckAsync(HttpClient client, Guid candidateId, int revision, string value, string? note = null)
    {
        using var response = await SetAssessmentAsync(client, candidateId, revision, value, Cancellation, note);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        var trimmed = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        (await response.Content.ReadFromJsonAsync<AssessmentResponse>(Cancellation)).ShouldBe(new AssessmentResponse(revision, value, trimmed));
    }

    private static async Task<CandidateAssessmentsResponse> ReadAsync(HttpClient client, Guid candidateId) =>
        (await client.GetFromJsonAsync<CandidateAssessmentsResponse>(AssessmentsPath(candidateId), Cancellation)).ShouldNotBeNull();

    private static async Task<CandidateResponse> ReadCandidateAsync(HttpClient client, Guid id) =>
        (await client.GetFromJsonAsync<CandidateResponse>($"/api/v1/candidates/{id}", Cancellation)).ShouldNotBeNull();

    private static async Task<ProjectResponse> ReadProjectAsync(HttpClient client, Guid id) =>
        (await client.GetFromJsonAsync<ProjectResponse>($"/api/v1/projects/{id}", Cancellation)).ShouldNotBeNull();

    private static async Task<JsonObject> ReadJsonAsync(HttpClient client, string path) =>
        (await client.GetFromJsonAsync<JsonObject>(path, Cancellation)).ShouldNotBeNull();

    private async Task<int> CountAssessmentsAsync()
    {
        await using var db = new WorkspaceDbContext(
            new DbContextOptionsBuilder<WorkspaceDbContext>().UseNpgsql(Api.ConnectionString).Options,
            FixedOrganizationContext.Of(Fixture.A.Id, Fixture.B.Id));
        return await db.CandidateAssessments.CountAsync(Cancellation);
    }

    private static async Task ShouldBeValidationProblemAsync(HttpResponseMessage response, params string[] errorKeys)
    {
        var text = await response.Content.ReadAsStringAsync(Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, text);
        using var body = JsonDocument.Parse(text);
        body.RootElement.GetProperty("type").GetString().ShouldNotBeNull().ShouldEndWith("/problems/" + ProblemTypes.Validation);
        body.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name).ShouldBe(errorKeys, ignoreOrder: true);
    }

    private static async Task<string> TypeOf(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        return body.RootElement.GetProperty("type").GetString()!;
    }
}
