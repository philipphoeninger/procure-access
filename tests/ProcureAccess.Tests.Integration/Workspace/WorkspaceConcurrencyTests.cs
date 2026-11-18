// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Modules.Workspace;
using ProcureAccess.Modules.Workspace.Candidates;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Modules.Workspace.Persistence;
using ProcureAccess.Modules.Workspace.Projects;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Workspace;

/// <summary>
/// Optimistic locking of the workspace resources (S6-20a; SPEC R-PRJ-07, R-CAN-11; ARCHITECTURE
/// §7.1; API-03, API-04): a project, a candidate and an evidence answer with an <c>ETag</c> of the
/// 🔧 epoch and their version; a change without <c>If-Match</c> is refused (428), one with a stale
/// tag answers 409 <c>conflict</c> with the competing editor's display name. Every test starts
/// from the Tenants fixture and a project of A with one candidate („Ja“) and one evidence, each
/// stored at version 1. The assessments, which need no <c>If-Match</c>, are in
/// <c>AssessmentEndpointsTests</c>; the assignment of a list in <c>ProjectCriteriaListTests</c>.
/// </summary>
public sealed class WorkspaceConcurrencyTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string FirstTag = IfMatchRequests.FirstTag;

    private Tenants? _tenants;
    private ProcurementProject? _project;
    private CandidateProduct? _candidate;
    private TestEvidence? _evidence;

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    private string ProjectPath => $"/api/v1/projects/{_project!.Id}";

    private string AssignPath => $"{ProjectPath}/criteria-configuration";

    private string CandidatePath => $"/api/v1/candidates/{_candidate!.Id}";

    private string StatusPath => $"{CandidatePath}/test-status";

    private string EvidencesPath => $"{CandidatePath}/test-evidences";

    private string EvidencePath => $"/api/v1/test-evidences/{_evidence!.Id}";

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private static object EvidenceBody(string testedBy = "Prüfstelle") =>
        new { testKind = "procedure", testTypeId = "softwareTest", testedBy, testDate = "2026-01-15" };

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        _tenants = Tenants.Build(Clock.GetUtcNow());
        // The accounts first: organisations and memberships reference them (foreign keys, S6-10b).
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => TestAccounts.Of(u, confirmed: true))]);
        await StoreAsync(_tenants.Entities);

        var now = Clock.GetUtcNow();
        _project = ProcurementProject.Create(_tenants.A.Id, "Projekt", TestUsers.OwnerA.Id, now);
        _candidate = CandidateProduct.Create(_project, new CandidateFacts("Produkt", null, null, null), now);
        _candidate.ChangeTestStatus(TestStatus.Tested, removeEvidences: false, now);
        _evidence = _candidate.AddEvidence(
            new TestEvidenceFacts(TestKind.Procedure, "softwareTest", null, "Prüfstelle", new DateOnly(2026, 1, 15), null, null), now).Evidence!;
        await StoreAsync(
            _project, _candidate, CandidatePrivateSection.Of(_candidate, null, null), TestEvidencePrivateNote.Of(_evidence, null));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact(DisplayName = "INV:API-04 " + nameof(Read_ProjectCandidateAndEvidence_CarryTheEpochAndTheVersion))]
    [Trait("Invariant", "API-04")]
    public async Task Read_ProjectCandidateAndEvidence_CarryTheEpochAndTheVersion()
    {
        using var client = CreateClient(TestUsers.ReaderA);

        using var project = await client.GetAsync(ProjectPath, Cancellation);
        using var candidate = await client.GetAsync(CandidatePath, Cancellation);

        // Concurrency:ETagEpoch is 1 in appsettings.json; every stored row is at version 1.
        project.Headers.ETag.ShouldNotBeNull().ToString().ShouldBe("\"1-1\"");
        candidate.Headers.ETag.ShouldNotBeNull().ToString().ShouldBe("\"1-1\"");
        var read = (await candidate.Content.ReadFromJsonAsync<JsonElement>(Cancellation)).GetProperty("testEvidences")[0];
        read.GetProperty("etag").GetString().ShouldBe("\"1-1\"");
        // The tag of a candidate or a project is its header only.
        (await project.Content.ReadAsStringAsync(Cancellation)).ShouldNotContain("\"etag\"");
    }

    [Theory(DisplayName = "INV:API-04 " + nameof(Change_TagOfAnotherEpochWithTheStoredVersion_Returns409))]
    [Trait("Invariant", "API-04")]
    [InlineData(Change.RenameProject)]
    [InlineData(Change.ChangeCandidate)]
    [InlineData(Change.ChangeEvidence)]
    public async Task Change_TagOfAnotherEpochWithTheStoredVersion_Returns409(Change change)
    {
        using var client = CreateClient(TestUsers.EditorA);

        // The tag a client kept from before the epoch was raised: the same version, another epoch.
        using var response = await SendAsync(client, change, "\"2-1\"");

        // Nobody has changed the rows since they were stored: the project names its creator, the others nobody.
        await ShouldBeConflictAsync(response, change == Change.RenameProject ? TestUsers.OwnerA.Email : null);
        await ShouldBeUnchangedAsync();
    }

    [Theory(DisplayName = "INV:API-03 " + nameof(Change_WithoutIfMatch_Returns428AndChangesNothing))]
    [Trait("Invariant", "API-03")]
    [MemberData(nameof(Changes))]
    public async Task Change_WithoutIfMatch_Returns428AndChangesNothing(Change change)
    {
        using var client = CreateClient(TestUsers.EditorA);

        using var missing = await SendAsync(client, change, ifMatch: null);
        using var blank = await SendAsync(client, change, ifMatch: " ");

        foreach (var response in new[] { missing, blank })
        {
            var text = await response.Content.ReadAsStringAsync(Cancellation);
            response.StatusCode.ShouldBe(HttpStatusCode.PreconditionRequired, text);
            using var body = JsonDocument.Parse(text);
            body.RootElement.GetProperty("type").GetString().ShouldNotBeNull().ShouldEndWith("/problems/" + ProblemTypes.PreconditionRequired);
        }

        await ShouldBeUnchangedAsync();
    }

    [Theory(DisplayName = "INV:API-03 " + nameof(Change_WithoutIfMatchByAReaderOrANonMember_IsRefusedByTheRoleFirst))]
    [Trait("Invariant", "API-03")]
    [MemberData(nameof(Changes))]
    public async Task Change_WithoutIfMatchByAReaderOrANonMember_IsRefusedByTheRoleFirst(Change change)
    {
        using var reader = CreateClient(TestUsers.ReaderA);
        using var ofB = CreateClient(TestUsers.EditorB);

        using var forbidden = await SendAsync(reader, change, ifMatch: null);
        using var notFound = await SendAsync(ofB, change, ifMatch: null);

        // The answer says nothing about a resource its sender may not change (TEN-01, TEN-05).
        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        notFound.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory(DisplayName = "INV:API-03 " + nameof(Change_StaleTag_Returns409WithTheCompetingEditorAndChangesNothing))]
    [Trait("Invariant", "API-03")]
    [MemberData(nameof(Changes))]
    public async Task Change_StaleTag_Returns409WithTheCompetingEditorAndChangesNothing(Change change)
    {
        using var owner = CreateClient(TestUsers.OwnerA);
        using var editor = CreateClient(TestUsers.EditorA);
        // The owner changes what the editor has read at version 1.
        using var first = await SendAsync(owner, CompetingChangeOf(change), FirstTag, competing: true);
        first.IsSuccessStatusCode.ShouldBeTrue(await first.Content.ReadAsStringAsync(Cancellation));
        var stored = await SnapshotAsync();

        using var response = await SendAsync(editor, change, FirstTag);

        await ShouldBeConflictAsync(response, TestUsers.OwnerA.Email);
        (await SnapshotAsync()).ShouldBe(stored);
    }

    [Theory(DisplayName = "INV:API-03 " + nameof(Change_WhatIsNoTagOfTheResource_Returns409))]
    [Trait("Invariant", "API-03")]
    [InlineData("*")]
    [InlineData("W/\"1-1\"")]
    [InlineData("\"1-1\", \"1-2\"")]
    [InlineData("1-1")]
    [InlineData("\"1-7\"")]
    public async Task Change_WhatIsNoTagOfTheResource_Returns409(string ifMatch)
    {
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await SendAsync(client, Change.ChangeCandidate, ifMatch);

        // The candidate was stored without an editor, so the conflict names nobody.
        await ShouldBeConflictAsync(response, editor: null);
        await ShouldBeUnchangedAsync();
    }

    [Fact(DisplayName = "INV:API-03 " + nameof(Change_CurrentTag_AnswersWithTheNewTagAndTheOldOneIsStale))]
    [Trait("Invariant", "API-03")]
    public async Task Change_CurrentTag_AnswersWithTheNewTagAndTheOldOneIsStale()
    {
        using var client = CreateClient(TestUsers.EditorA);

        // The evidence before the status, which removes it; the status is the candidate's second change.
        foreach (var (change, tag, newTag) in new[]
        {
            (Change.RenameProject, FirstTag, "\"1-2\""),
            (Change.ChangeEvidence, FirstTag, "\"1-2\""),
            (Change.ChangeCandidate, FirstTag, "\"1-2\""),
            (Change.SetTestStatus, "\"1-2\"", "\"1-3\""),
        })
        {
            using var response = await SendAsync(client, change, tag);
            response.StatusCode.ShouldBe(HttpStatusCode.OK, $"{change}: {await response.Content.ReadAsStringAsync(Cancellation)}");
            response.Headers.ETag.ShouldNotBeNull().ToString().ShouldBe(newTag, change.ToString());
        }

        using var again = await SendAsync(client, Change.RenameProject, FirstTag);
        await ShouldBeConflictAsync(again, TestUsers.EditorA.Email);
        await using var db = Workspace();
        (await db.ProcurementProjects.SingleAsync(Cancellation)).ShouldSatisfyAllConditions(
            p => p.Version.ShouldBe(2), p => p.ModifiedByUserId.ShouldBe(TestUsers.EditorA.Id));
        // The evidence went with the status, which left „Ja“.
        (await db.TestEvidences.CountAsync(Cancellation)).ShouldBe(0);
    }

    [Fact(DisplayName = "INV:API-03 " + nameof(Change_SameValues_KeepsTheTag))]
    [Trait("Invariant", "API-03")]
    public async Task Change_SameValues_KeepsTheTag()
    {
        using var client = CreateClient(TestUsers.EditorA);

        using var project = await client.SendWithTagAsync(HttpMethod.Patch, ProjectPath, new { name = "Projekt", status = "open" }, FirstTag, Cancellation);
        using var candidate = await client.SendWithTagAsync(HttpMethod.Patch, CandidatePath, new { name = "Produkt" }, FirstTag, Cancellation);
        using var status = await client.SendWithTagAsync(HttpMethod.Put, StatusPath, new { testStatus = "tested" }, FirstTag, Cancellation);
        using var evidence = await client.SendWithTagAsync(HttpMethod.Put, EvidencePath, EvidenceBody(), FirstTag, Cancellation);

        foreach (var response in new[] { project, candidate, status, evidence })
        {
            response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
            response.Headers.ETag.ShouldNotBeNull().ToString().ShouldBe(FirstTag);
        }
    }

    [Fact(DisplayName = "INV:API-03 " + nameof(Change_BelowAResource_LeavesTheTagOfTheResourceValid))]
    [Trait("Invariant", "API-03")]
    public async Task Change_BelowAResource_LeavesTheTagOfTheResourceValid()
    {
        using var owner = CreateClient(TestUsers.OwnerA);
        using var editor = CreateClient(TestUsers.EditorA);
        Clock.Advance(TimeSpan.FromMinutes(1));

        // A colleague changes the candidate and a field of the evidence …
        using var candidateChange = await SendAsync(owner, Change.ChangeCandidate, FirstTag);
        using var evidenceChange = await SendAsync(owner, Change.ChangeEvidence, FirstTag);
        // … which leaves the project and, for the evidence, the candidate at the version the editor read.
        using var project = await SendAsync(editor, Change.RenameProject, FirstTag);
        using var status = await editor.SendWithTagAsync(
            HttpMethod.Put, StatusPath, new { testStatus = "tested" }, candidateChange.Headers.ETag!.ToString(), Cancellation);

        candidateChange.StatusCode.ShouldBe(HttpStatusCode.OK);
        evidenceChange.StatusCode.ShouldBe(HttpStatusCode.OK);
        project.StatusCode.ShouldBe(HttpStatusCode.OK, await project.Content.ReadAsStringAsync(Cancellation));
        status.StatusCode.ShouldBe(HttpStatusCode.OK, await status.Content.ReadAsStringAsync(Cancellation));
        // The changes below still count as the project's last change (owner decision S6-15 5 (a)).
        (await project.Content.ReadFromJsonAsync<ProjectResponse>(Cancellation)).ShouldNotBeNull().ModifiedAt.ShouldBe(Clock.GetUtcNow());
    }

    [Fact(DisplayName = "INV:API-03 " + nameof(AddOrRemoveEvidence_GivesTheCandidateANewTag))]
    [Trait("Invariant", "API-03")]
    public async Task AddOrRemoveEvidence_GivesTheCandidateANewTag()
    {
        using var client = CreateClient(TestUsers.EditorA);

        // Adding needs no If-Match; the new evidence starts at version 1 with its own tag.
        using var added = await client.PostAsJsonAsync(EvidencesPath, new { testKind = "own", testedBy = "Wir", testDate = "2026-01-15" }, Cancellation);
        var afterAdding = await client.TagOfAsync(CandidatePath, Cancellation);
        using var removed = await client.SendWithTagAsync(HttpMethod.Delete, EvidencePath, FirstTag, Cancellation);
        var afterRemoving = await client.TagOfAsync(CandidatePath, Cancellation);
        // The status change of someone who has not seen the set of evidences change is refused (WSP-01).
        using var status = await client.SendWithTagAsync(
            HttpMethod.Put, StatusPath, new { testStatus = "notTested", removeEvidences = true }, FirstTag, Cancellation);

        added.StatusCode.ShouldBe(HttpStatusCode.Created, await added.Content.ReadAsStringAsync(Cancellation));
        added.Headers.ETag.ShouldNotBeNull().ToString().ShouldBe(FirstTag);
        (await added.Content.ReadFromJsonAsync<TestEvidenceResponse>(Cancellation)).ShouldNotBeNull().Etag.ShouldBe(FirstTag);
        afterAdding.ShouldBe("\"1-2\"");
        removed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        afterRemoving.ShouldBe("\"1-3\"");
        await ShouldBeConflictAsync(status, TestUsers.EditorA.Email);
    }

    [Fact(DisplayName = "INV:API-03 " + nameof(Create_ProjectAndCandidate_AnswerWithTheirFirstTagAndStoreTheEditor))]
    [Trait("Invariant", "API-03")]
    public async Task Create_ProjectAndCandidate_AnswerWithTheirFirstTagAndStoreTheEditor()
    {
        using var client = CreateClient(TestUsers.EditorA);

        using var project = await client.PostAsJsonAsync($"/api/v1/organizations/{Fixture.A.Id}/projects", new { name = "Neu" }, Cancellation);
        using var candidate = await client.PostAsJsonAsync($"{ProjectPath}/candidates", new { name = "Neues Produkt" }, Cancellation);

        project.Headers.ETag.ShouldNotBeNull().ToString().ShouldBe(FirstTag);
        candidate.Headers.ETag.ShouldNotBeNull().ToString().ShouldBe(FirstTag);
        await using var db = Workspace();
        (await db.CandidateProducts.SingleAsync(c => c.Name == "Neues Produkt", Cancellation)).ModifiedByUserId.ShouldBe(TestUsers.EditorA.Id);
        // Adding a candidate stamps the project and keeps its version.
        (await db.ProcurementProjects.SingleAsync(p => p.Id == _project!.Id, Cancellation)).Version.ShouldBe(1);
    }

    [Fact(DisplayName = "INV:API-03 " + nameof(Save_RowWithANewVersionSinceItWasLoaded_IsRefusedByTheToken))]
    [Trait("Invariant", "API-03")]
    public async Task Save_RowWithANewVersionSinceItWasLoaded_IsRefusedByTheToken()
    {
        await using var slow = Workspace();
        var loaded = await slow.ProcurementProjects.SingleAsync(Cancellation);
        await RenameElsewhereAsync("Von nebenan");

        loaded.Rename("Zu spät", Clock.GetUtcNow());
        loaded.MarkEdited(TestUsers.EditorA.Id);

        await Should.ThrowAsync<DbUpdateConcurrencyException>(() => slow.SaveChangesAsync(Cancellation));
        await using var db = Workspace();
        (await db.ProcurementProjects.SingleAsync(Cancellation)).Name.ShouldBe("Von nebenan");
    }

    [Fact(DisplayName = "INV:API-03 " + nameof(RunAsync_StampMeetsANewVersion_RunsAgainAndKeepsBoth))]
    [Trait("Invariant", "API-03")]
    public async Task RunAsync_StampMeetsANewVersion_RunsAgainAndKeepsBoth()
    {
        await using var db = Workspace();
        var later = Clock.GetUtcNow().AddMinutes(5);
        var attempts = 0;

        // A change below the project (it only stamps the project) races a rename of the project.
        await ConcurrentChange.RunAsync(db, async () =>
        {
            attempts++;
            var project = await db.ProcurementProjects.SingleAsync(Cancellation);
            if (attempts == 1)
            {
                await RenameElsewhereAsync("Von nebenan");
            }

            project.MarkChanged(later);
            await db.SaveChangesAsync(Cancellation);
            return true;
        });

        attempts.ShouldBe(2);
        await using var check = Workspace();
        (await check.ProcurementProjects.SingleAsync(Cancellation)).ShouldSatisfyAllConditions(
            p => p.Name.ShouldBe("Von nebenan"), p => p.Version.ShouldBe(2), p => p.ModifiedAt.ShouldBe(later));
    }

    [Fact(DisplayName = "INV:API-03 " + nameof(RunAsync_EverySaveMeetsANewVersion_GivesUpAfterThreeAttempts))]
    [Trait("Invariant", "API-03")]
    public async Task RunAsync_EverySaveMeetsANewVersion_GivesUpAfterThreeAttempts()
    {
        await using var db = Workspace();
        var attempts = 0;

        await Should.ThrowAsync<DbUpdateConcurrencyException>(() => ConcurrentChange.RunAsync(db, async () =>
        {
            attempts++;
            var project = await db.ProcurementProjects.SingleAsync(Cancellation);
            await RenameElsewhereAsync($"Fassung {attempts}");
            project.MarkChanged(Clock.GetUtcNow().AddMinutes(attempts));
            await db.SaveChangesAsync(Cancellation);
            return true;
        }));

        attempts.ShouldBe(ConcurrentChange.Attempts);
    }

    public enum Change
    {
        RenameProject,
        AssignList,
        ChangeCandidate,
        SetTestStatus,
        ChangeEvidence,
        RemoveEvidence,
    }

    public static TheoryData<Change> Changes => [.. Enum.GetValues<Change>()];

    /// <summary>What a colleague does first so that <paramref name="change"/> meets a newer version of its resource.</summary>
    private static Change CompetingChangeOf(Change change) => change switch
    {
        Change.RenameProject or Change.AssignList => Change.RenameProject,
        Change.ChangeCandidate or Change.SetTestStatus => Change.ChangeCandidate,
        _ => Change.ChangeEvidence,
    };

    /// <param name="competing">The colleague's change, with other values than the change under test.</param>
    private Task<HttpResponseMessage> SendAsync(HttpClient client, Change change, string? ifMatch, bool competing = false) => change switch
    {
        Change.RenameProject => client.SendWithTagAsync(
            HttpMethod.Patch, ProjectPath, new { name = competing ? "Von der Kollegin" : "Umbenannt" }, ifMatch, Cancellation),
        // No list is needed: the precondition is checked before the source.
        Change.AssignList => client.SendWithTagAsync(
            HttpMethod.Put, AssignPath, new { sourceConfigurationId = Guid.NewGuid() }, ifMatch, Cancellation),
        Change.ChangeCandidate => client.SendWithTagAsync(
            HttpMethod.Patch, CandidatePath, new { name = competing ? "Von der Kollegin" : "Umbenannt" }, ifMatch, Cancellation),
        Change.SetTestStatus => client.SendWithTagAsync(
            HttpMethod.Put, StatusPath, new { testStatus = "notTested", removeEvidences = true }, ifMatch, Cancellation),
        Change.ChangeEvidence => client.SendWithTagAsync(
            HttpMethod.Put, EvidencePath, EvidenceBody(competing ? "Kollegin" : "Andere Prüfstelle"), ifMatch, Cancellation),
        Change.RemoveEvidence => client.SendWithTagAsync(HttpMethod.Delete, EvidencePath, ifMatch, Cancellation),
        _ => throw new ArgumentOutOfRangeException(nameof(change)),
    };

    /// <summary>409 <c>conflict</c> with <c>editorDisplayName</c> — the member is there also when it names nobody.</summary>
    private static async Task ShouldBeConflictAsync(HttpResponseMessage response, string? editor)
    {
        var text = await response.Content.ReadAsStringAsync(Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict, text);
        using var body = JsonDocument.Parse(text);
        body.RootElement.GetProperty("type").GetString().ShouldNotBeNull().ShouldEndWith("/problems/" + ProblemTypes.Conflict);
        body.RootElement.GetProperty("editorDisplayName").GetString().ShouldBe(editor);
        response.Headers.ETag.ShouldBeNull();
    }

    private async Task ShouldBeUnchangedAsync() =>
        (await SnapshotAsync()).ShouldBe(("Projekt", 1, "Produkt", TestStatus.Tested, 1, "Prüfstelle", 1));

    /// <summary>What the changes of this class change, with the versions.</summary>
    private async Task<(string Project, int ProjectVersion, string Candidate, TestStatus Status, int CandidateVersion, string? TestedBy, int? EvidenceVersion)>
        SnapshotAsync()
    {
        await using var db = Workspace();
        var project = await db.ProcurementProjects.AsNoTracking().SingleAsync(Cancellation);
        var candidate = await db.CandidateProducts.AsNoTracking().Include(c => c.TestEvidences).SingleAsync(Cancellation);
        var evidence = candidate.TestEvidences.SingleOrDefault();
        return (project.Name, project.Version, candidate.Name, candidate.TestStatus, candidate.Version, evidence?.TestedByName, evidence?.Version);
    }

    /// <summary>A rename of the project in a context of its own, as another request does it.</summary>
    private async Task RenameElsewhereAsync(string name)
    {
        await using var other = Workspace();
        var project = await other.ProcurementProjects.SingleAsync(Cancellation);
        project.Rename(name, Clock.GetUtcNow());
        project.MarkEdited(TestUsers.OwnerA.Id);
        await other.SaveChangesAsync(Cancellation);
    }

    /// <summary>A context of its own outside any request that sees organisation A.</summary>
    private WorkspaceDbContext Workspace() =>
        new(new DbContextOptionsBuilder<WorkspaceDbContext>().UseNpgsql(Api.ConnectionString).Options,
            FixedOrganizationContext.Of(Fixture.A.Id));
}
