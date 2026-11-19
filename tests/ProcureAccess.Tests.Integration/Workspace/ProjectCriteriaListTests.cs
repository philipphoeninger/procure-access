// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using ProcureAccess.Modules.Configurator.Configurations;
using ProcureAccess.Modules.Configurator.Persistence;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Modules.Platform.Persistence;
using ProcureAccess.Modules.Workspace.Assessments;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Modules.Workspace.Persistence;
using ProcureAccess.Modules.Workspace.Projects;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Integration.Export.ExportTestCatalog;

namespace ProcureAccess.Tests.Integration.Workspace;

/// <summary>
/// The project's own copy of a criteria list (S6-12; SPEC R-PRJ-02, R-PRJ-04, R-PRJ-06, R-SAV-02,
/// R-SAV-04; ARCHITECTURE §5.4, C47, C51, C53, C59): <c>PUT /projects/{id}/criteria-configuration</c>
/// copies an organisation list into the project and replaces its previous copy; the source stays as
/// it was; a copy is never a source, never listed for S14 and never shared; its criteria can be
/// changed, its selection cannot; since S6-18 the lock of the list on the candidates' assessments
/// (WSP-02) and what an adjusted copy means for them (WSP-03). Every test starts from the Tenants fixture with the accounts of
/// its users stored (all confirmed but Reader B) and the test releases with <c>T.1</c> active. The
/// authorisation matrix is <c>AuthorizationMatrixTests</c> (TEN-05).
/// </summary>
public sealed class ProjectCriteriaListTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string ConfigurationsPath = "/api/v1/configurations";

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

    [Fact(DisplayName = "INV:WSP-02 " + nameof(Assign_OrganisationList_CopiesItIntoTheProjectAndLeavesTheSourceUnchanged))]
    [Trait("Invariant", "WSP-02")]
    public async Task Assign_OrganisationList_CopiesItIntoTheProjectAndLeavesTheSourceUnchanged()
    {
        var (sourceId, criteria) = await SaveListAsync("HR-Software 2027", (index, entry) =>
        {
            if (index == 0)
            {
                entry["isIncluded"] = false;
                entry["note"] = "Nicht bei uns";
            }
        });
        var project = await StoreProjectAsync();
        using var client = CreateClient(TestUsers.EditorA);
        var sourceBefore = await ReadRawAsync(client, sourceId);
        Clock.Advance(TimeSpan.FromHours(1));

        using var response = await client.PutBelowCurrentAsync(CriteriaPath(project.Id), new { sourceConfigurationId = sourceId }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        // The assignment is the project's second version; the answer's ETag is the project's (API-03, S6-20).
        response.Headers.ETag.ShouldNotBeNull().ToString().ShouldBe("\"1-2\"");
        (await client.TagOfAsync($"/api/v1/projects/{project.Id}", Cancellation)).ShouldBe("\"1-2\"");
        var assigned = (await response.Content.ReadFromJsonAsync<ProjectCriteriaListResponse>(Cancellation)).ShouldNotBeNull();
        assigned.ShouldSatisfyAllConditions(
            l => l.Id.ShouldNotBe(sourceId),
            l => l.Name.ShouldBe("HR-Software 2027"),
            l => l.ReleaseVersion.ShouldBe(MiniCatalog.T1Version));
        (await ReadProjectAsync(client, project.Id)).CriteriaList.ShouldBe(assigned);

        var source = await ReadAsync(client, sourceId);
        var copy = await ReadAsync(client, assigned.Id);
        copy.ShouldSatisfyAllConditions(
            c => c.ProcurementProjectId.ShouldBe(project.Id),
            c => c.OrganizationId.ShouldBe(Fixture.A.Id),
            c => c.Selection.ShouldBeEquivalentTo(source.Selection),
            c => c.Criteria.ShouldBeEquivalentTo(source.Criteria),
            c => c.CreatedAt.ShouldBe(Clock.GetUtcNow()));
        copy.Criteria.Count.ShouldBe(criteria.Count);
        copy.Criteria.Single(c => !c.IsIncluded).Note.ShouldBe("Nicht bei uns");
        source.ProcurementProjectId.ShouldBeNull();
        (await ReadRawAsync(client, sourceId)).ShouldBe(sourceBefore);
    }

    [Fact(DisplayName = "INV:WSP-02 " + nameof(Assign_SecondList_ReplacesTheProjectsCopyAndKeepsBothSources))]
    [Trait("Invariant", "WSP-02")]
    public async Task Assign_SecondList_ReplacesTheProjectsCopyAndKeepsBothSources()
    {
        var (first, _) = await SaveListAsync("Erste Liste");
        var (second, _) = await SaveListAsync("Zweite Liste");
        var project = await StoreProjectAsync();
        using var client = CreateClient(TestUsers.OwnerA);
        var firstCopy = await AssignAsync(client, project.Id, first);

        var secondCopy = await AssignAsync(client, project.Id, second);

        secondCopy.Name.ShouldBe("Zweite Liste");
        (await ReadProjectAsync(client, project.Id)).CriteriaList.ShouldBe(secondCopy);
        using var oldCopy = await client.GetAsync($"{ConfigurationsPath}/{firstCopy.Id}", Cancellation);
        oldCopy.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        await using var db = Configurator(Fixture.A.Id);
        (await db.CriteriaConfigurations.CountAsync(c => c.ProcurementProjectId == project.Id, Cancellation)).ShouldBe(1);
        (await db.CriteriaConfigurations.Select(c => c.Id).ToListAsync(Cancellation))
            .ShouldBe([first, second, secondCopy.Id], ignoreOrder: true);
    }

    [Fact(DisplayName = "INV:WSP-02 " + nameof(Assign_ProjectCopyAsSource_Returns400AndCopiesAreNotListed))]
    [Trait("Invariant", "WSP-02")]
    public async Task Assign_ProjectCopyAsSource_Returns400AndCopiesAreNotListed()
    {
        var (sourceId, _) = await SaveListAsync("Liste");
        var project = await StoreProjectAsync();
        var other = await StoreProjectAsync("Anderes Projekt");
        using var client = CreateClient(TestUsers.EditorA);
        var copy = await AssignAsync(client, project.Id, sourceId);

        using var response = await client.PutBelowCurrentAsync(CriteriaPath(other.Id), new { sourceConfigurationId = copy.Id }, Cancellation);

        await ShouldBeValidationProblemAsync(response, "sourceConfigurationId");
        (await ReadProjectAsync(client, other.Id)).CriteriaList.ShouldBeNull();
        var listed = await client.GetFromJsonAsync<List<ConfigurationSummaryResponse>>(
            $"{ConfigurationsPath}?organizationId={Fixture.A.Id}", Cancellation);
        listed.ShouldNotBeNull().Select(l => l.Id).ShouldBe([sourceId]);
    }

    [Fact]
    public async Task Assign_MissingUnknownOrForeignSource_Returns400AtTheSourceAndCopiesNothing()
    {
        // Editor A is also a member of B and so sees B's list: still not a source for A's project.
        await StoreAsync(OrganizationMembership.Create(Fixture.B.Id, TestUsers.EditorA.Id, OrganizationRole.Editor, Clock.GetUtcNow(), Clock.GetUtcNow()));
        var (ofB, _) = await SaveListAsync("Liste von B", user: TestUsers.EditorA, organizationId: Fixture.B.Id);
        var project = await StoreProjectAsync();
        using var client = CreateClient(TestUsers.EditorA);

        using var missing = await client.PutBelowCurrentAsync(CriteriaPath(project.Id), new { }, Cancellation);
        using var unknown = await client.PutBelowCurrentAsync(CriteriaPath(project.Id), new { sourceConfigurationId = Guid.NewGuid() }, Cancellation);
        using var foreign = await client.PutBelowCurrentAsync(CriteriaPath(project.Id), new { sourceConfigurationId = ofB }, Cancellation);

        foreach (var response in new[] { missing, unknown, foreign })
        {
            await ShouldBeValidationProblemAsync(response, "sourceConfigurationId");
        }

        await using var db = Configurator(Fixture.A.Id, Fixture.B.Id);
        (await db.CriteriaConfigurations.CountAsync(c => c.ProcurementProjectId != null, Cancellation)).ShouldBe(0);
        // A refused assignment is no version of the project: its sender's tag stays valid (API-03, S6-20).
        (await client.TagOfAsync($"/api/v1/projects/{project.Id}", Cancellation)).ShouldBe(IfMatchRequests.FirstTag);
    }

    /// <summary>R-PRJ-05: an archived project is read-only, its list too.</summary>
    [Fact]
    public async Task Assign_ArchivedProject_Returns409ProjectArchivedAndCopiesNothing()
    {
        var (sourceId, _) = await SaveListAsync("Liste");
        var project = await StoreProjectAsync(status: ProjectStatus.Archived);
        using var client = CreateClient(TestUsers.OwnerA);

        using var response = await client.PutBelowCurrentAsync(CriteriaPath(project.Id), new { sourceConfigurationId = sourceId }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await TypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.ProjectArchived);
        (await ReadProjectAsync(client, project.Id)).CriteriaList.ShouldBeNull();
    }

    /// <summary>
    /// The lock (C47, C53): once a candidate of the project has an assessment with a value, replacing
    /// answers 409 <c>locked-criteria-list</c> with its field error; the copy and the assessment stay.
    /// </summary>
    [Fact(DisplayName = "INV:WSP-02 " + nameof(Replace_OnceACandidateHasAnAssessment_Returns409LockedCriteriaListAndKeepsCopyAndAssessment))]
    [Trait("Invariant", "WSP-02")]
    public async Task Replace_OnceACandidateHasAnAssessment_Returns409LockedCriteriaListAndKeepsCopyAndAssessment()
    {
        var (first, criteria) = await SaveListAsync("Erste Liste");
        var (second, _) = await SaveListAsync("Zweite Liste");
        var project = await StoreProjectAsync();
        var candidate = await StoreCandidateAsync(project);
        using var client = CreateClient(TestUsers.EditorA);
        var copy = await AssignAsync(client, project.Id, first);
        var revision = criteria[0]!["revisionId"]!.GetValue<int>();
        (await ReadProjectAsync(client, project.Id)).CriteriaListLocked.ShouldBeFalse();
        await AssessAsync(client, candidate.Id, revision, "fulfilled");
        // S32 reads the lock with the project and shows `projects.detail.criteriaLocked` (S6-14).
        (await ReadProjectAsync(client, project.Id)).CriteriaListLocked.ShouldBeTrue();

        using var response = await client.PutBelowCurrentAsync(CriteriaPath(project.Id), new { sourceConfigurationId = second }, Cancellation);

        var text = await response.Content.ReadAsStringAsync(Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict, text);
        using var body = JsonDocument.Parse(text);
        body.RootElement.GetProperty("type").GetString().ShouldNotBeNull().ShouldEndWith("/problems/" + ProblemTypes.LockedCriteriaList);
        body.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name).ShouldBe(["sourceConfigurationId"]);
        (await ReadProjectAsync(client, project.Id)).CriteriaList.ShouldBe(copy);
        (await ReadAssessmentsAsync(client, candidate.Id)).Criteria.Single(c => c.CriterionRevisionId == revision).Value.ShouldBe("fulfilled");

        // Back to „Nicht eingeschätzt“: no assessment exists any more, the list can be replaced again.
        await AssessAsync(client, candidate.Id, revision, "unknown");
        (await ReadProjectAsync(client, project.Id)).CriteriaListLocked.ShouldBeFalse();
        (await AssignAsync(client, project.Id, second)).Name.ShouldBe("Zweite Liste");
    }

    /// <summary>
    /// Rows with „Nicht eingeschätzt“ — a note without a value — do not lock (WSP-02). They belong
    /// to the replaced copy and go with it (owner decision S6-18 4 (a)); another project's rows stay.
    /// </summary>
    [Fact(DisplayName = "INV:WSP-02 " + nameof(Replace_WhileOnlyRowsWithoutAValueExist_SucceedsAndRemovesThem))]
    [Trait("Invariant", "WSP-02")]
    public async Task Replace_WhileOnlyRowsWithoutAValueExist_SucceedsAndRemovesThem()
    {
        var (first, criteria) = await SaveListAsync("Erste Liste");
        var (second, _) = await SaveListAsync("Zweite Liste");
        var project = await StoreProjectAsync();
        var other = await StoreProjectAsync("Anderes Projekt");
        var candidate = await StoreCandidateAsync(project);
        var candidateOfOther = await StoreCandidateAsync(other);
        using var client = CreateClient(TestUsers.EditorA);
        await AssignAsync(client, project.Id, first);
        await AssignAsync(client, other.Id, first);
        var revision = criteria[0]!["revisionId"]!.GetValue<int>();
        await AssessAsync(client, candidate.Id, revision, "unknown", "Nachfragen beim Anbieter");
        await AssessAsync(client, candidateOfOther.Id, revision, "barrier");
        (await ReadProjectAsync(client, project.Id)).CriteriaListLocked.ShouldBeFalse();
        (await ReadProjectAsync(client, other.Id)).CriteriaListLocked.ShouldBeTrue();

        var replaced = await AssignAsync(client, project.Id, second);

        replaced.Name.ShouldBe("Zweite Liste");
        (await ReadProjectAsync(client, project.Id)).CriteriaList.ShouldBe(replaced);
        (await ReadAssessmentsAsync(client, candidate.Id)).Criteria.ShouldAllBe(c => c.Value == "unknown" && c.Note == null);
        await using var db = Workspace(Fixture.A.Id);
        (await db.CandidateAssessments.Select(a => a.CandidateProductId).ToListAsync(Cancellation)).ShouldBe([candidateOfOther.Id]);
    }

    [Fact]
    public async Task Assign_TwoFirstAssignmentsAtOnce_OneWinsAndExactlyOneCopyIsLeft()
    {
        var (first, _) = await SaveListAsync("Erste Liste");
        var (second, _) = await SaveListAsync("Zweite Liste");
        var project = await StoreProjectAsync();
        // Two browsers: each client fetches its own antiforgery token.
        using var editor = CreateClient(TestUsers.EditorA);
        using var owner = CreateClient(TestUsers.OwnerA);

        // Both have read the project at its first version.
        var responses = await Task.WhenAll(
            editor.SendWithTagAsync(HttpMethod.Put, CriteriaPath(project.Id), new { sourceConfigurationId = first }, IfMatchRequests.FirstTag, Cancellation),
            owner.SendWithTagAsync(HttpMethod.Put, CriteriaPath(project.Id), new { sourceConfigurationId = second }, IfMatchRequests.FirstTag, Cancellation));

        // One assignment is the project's second version; the other meets it (API-03, S6-20).
        responses.Select(r => r.StatusCode).Order().ShouldBe([HttpStatusCode.OK, HttpStatusCode.Conflict]);
        foreach (var response in responses)
        {
            response.Dispose();
        }

        await using var db = Configurator(Fixture.A.Id);
        (await db.CriteriaConfigurations.CountAsync(c => c.ProcurementProjectId == project.Id, Cancellation)).ShouldBe(1);
    }

    [Fact]
    public async Task Store_TwoCopiesOfOneProject_IsRefusedByTheUniqueIndex()
    {
        var (sourceId, _) = await SaveListAsync("Liste");
        var projectId = Guid.NewGuid();
        await using var db = Configurator(Fixture.A.Id);
        var source = await db.CriteriaConfigurations.AsNoTracking().SingleAsync(c => c.Id == sourceId, Cancellation);
        db.CriteriaConfigurations.Add(source.CopyForProject(projectId, TestUsers.EditorA.Id, Clock.GetUtcNow()));
        db.CriteriaConfigurations.Add(source.CopyForProject(projectId, TestUsers.EditorA.Id, Clock.GetUtcNow()));

        var exception = await Should.ThrowAsync<DbUpdateException>(() => db.SaveChangesAsync(Cancellation));

        exception.InnerException.ShouldBeOfType<PostgresException>().SqlState.ShouldBe(PostgresErrorCodes.UniqueViolation);
    }

    [Fact(DisplayName = "INV:WSP-03 " + nameof(Decide_ProjectCopy_IncludeExcludePriorityAndNoteSucceedAndLeaveTheSource))]
    [Trait("Invariant", "WSP-03")]
    public async Task Decide_ProjectCopy_IncludeExcludePriorityAndNoteSucceedAndLeaveTheSource()
    {
        var (sourceId, criteria) = await SaveListAsync("Liste");
        var project = await StoreProjectAsync();
        using var client = CreateClient(TestUsers.EditorA);
        var copy = await AssignAsync(client, project.Id, sourceId);
        var sourceBefore = await ReadRawAsync(client, sourceId);
        var excluded = criteria[0]!["revisionId"]!.GetValue<int>();
        var prioritised = criteria[1]!["revisionId"]!.GetValue<int>();

        using var exclude = await client.PatchListAsync(
            CriterionPath(copy.Id, excluded), new { isIncluded = false, note = "Für dieses Projekt nicht nötig" }, Cancellation);
        using var priority = await client.PatchListAsync(
            CriterionPath(copy.Id, prioritised), new { isIncluded = true, overrideLevel = "Info" }, Cancellation);
        using var includeAgain = await client.PatchListAsync(
            CriterionPath(copy.Id, excluded), new { isIncluded = true, note = "Doch nötig" }, Cancellation);

        foreach (var response in new[] { exclude, priority, includeAgain })
        {
            response.StatusCode.ShouldBe(HttpStatusCode.NoContent, await response.Content.ReadAsStringAsync(Cancellation));
        }

        var decided = (await ReadAsync(client, copy.Id)).Criteria;
        decided.Single(c => c.CriterionRevisionId == excluded).ShouldSatisfyAllConditions(
            c => c.IsIncluded.ShouldBeTrue(), c => c.Note.ShouldBe("Doch nötig"));
        decided.Single(c => c.CriterionRevisionId == prioritised).OverrideLevel.ShouldBe("Info");
        (await ReadRawAsync(client, sourceId)).ShouldBe(sourceBefore);
    }

    /// <summary>
    /// The candidate clauses of WSP-03 (C51; S6-18): an excluded criterion keeps its assessment,
    /// hidden and uncounted, and shows it unchanged when it is included again; a criterion that was
    /// excluded when the list was assigned is „Nicht eingeschätzt“ for every candidate once included.
    /// </summary>
    [Fact(DisplayName = "INV:WSP-03 " + nameof(ExcludeAndInclude_CriteriaOfAnAssessedProject_KeepTheAssessmentHiddenAndStartNewOnesAsUnknown))]
    [Trait("Invariant", "WSP-03")]
    public async Task ExcludeAndInclude_CriteriaOfAnAssessedProject_KeepTheAssessmentHiddenAndStartNewOnesAsUnknown()
    {
        var (sourceId, criteria) = await SaveListAsync("Liste", (index, entry) => entry["isIncluded"] = index != 1);
        var project = await StoreProjectAsync();
        var first = await StoreCandidateAsync(project);
        var second = await StoreCandidateAsync(project);
        using var client = CreateClient(TestUsers.EditorA);
        var copy = await AssignAsync(client, project.Id, sourceId);
        var assessed = criteria[0]!["revisionId"]!.GetValue<int>();
        var newlyIncluded = criteria[1]!["revisionId"]!.GetValue<int>();
        await AssessAsync(client, first.Id, assessed, "fulfilled", "Prüfbericht");
        var before = await ReadAssessmentsAsync(client, first.Id);

        using var exclude = await client.PatchListAsync(CriterionPath(copy.Id, assessed), new { isIncluded = false }, Cancellation);
        exclude.StatusCode.ShouldBe(HttpStatusCode.NoContent, await exclude.Content.ReadAsStringAsync(Cancellation));
        var whileExcluded = await ReadAssessmentsAsync(client, first.Id);
        using var includeNew = await client.PatchListAsync(CriterionPath(copy.Id, newlyIncluded), new { isIncluded = true }, Cancellation);
        using var includeAgain = await client.PatchListAsync(CriterionPath(copy.Id, assessed), new { isIncluded = true }, Cancellation);

        includeNew.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        includeAgain.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        whileExcluded.ShouldSatisfyAllConditions(
            w => w.Criteria.ShouldNotContain(c => c.CriterionRevisionId == assessed),
            w => w.Progress.ShouldBe(new AssessmentProgressResponse(0, before.Progress.Total - 1)));
        await using (var db = Workspace(Fixture.A.Id))
        {
            (await db.CandidateAssessments.SingleAsync(Cancellation)).Fulfillment.ShouldBe(Fulfillment.Fulfilled);
        }

        var after = await ReadAssessmentsAsync(client, first.Id);
        after.Criteria.Single(c => c.CriterionRevisionId == assessed)
             .ShouldBeEquivalentTo(before.Criteria.Single(c => c.CriterionRevisionId == assessed));
        after.Criteria.Single(c => c.CriterionRevisionId == assessed).ShouldSatisfyAllConditions(
            c => c.Value.ShouldBe("fulfilled"), c => c.Note.ShouldBe("Prüfbericht"));
        after.Progress.ShouldBe(new AssessmentProgressResponse(1, before.Progress.Total + 1));
        foreach (var candidate in new[] { first, second })
        {
            (await ReadAssessmentsAsync(client, candidate.Id)).Criteria.Single(c => c.CriterionRevisionId == newlyIncluded)
                .ShouldSatisfyAllConditions(c => c.Value.ShouldBe("unknown"), c => c.Note.ShouldBeNull());
        }
    }

    [Fact(DisplayName = "INV:WSP-03 " + nameof(ChangeSelection_ProjectCopy_Returns409ProjectCopyAndKeepsTheSelection))]
    [Trait("Invariant", "WSP-03")]
    public async Task ChangeSelection_ProjectCopy_Returns409ProjectCopyAndKeepsTheSelection()
    {
        var (sourceId, _) = await SaveListAsync("Liste");
        var project = await StoreProjectAsync();
        using var client = CreateClient(TestUsers.OwnerA);
        var copy = await AssignAsync(client, project.Id, sourceId);
        var before = await ReadRawAsync(client, copy.Id);
        var selection = (JsonObject)WebSelection.DeepClone();
        selection["releaseVersion"] = MiniCatalog.T1Version;
        selection["useCaseTemplateId"] = null;

        using var response = await client.PutListAsync(
            $"{ConfigurationsPath}/{copy.Id}/selection", new JsonObject { ["selection"] = selection }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict, await response.Content.ReadAsStringAsync(Cancellation));
        (await TypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.ProjectCopy);
        (await ReadRawAsync(client, copy.Id)).ShouldBe(before);
    }

    [Fact]
    public async Task DeleteAndCopyToActiveRelease_ProjectCopy_Return409ProjectCopyAndKeepTheCopy()
    {
        var (sourceId, _) = await SaveListAsync("Liste");
        var project = await StoreProjectAsync();
        using var client = CreateClient(TestUsers.OwnerA);
        var copy = await AssignAsync(client, project.Id, sourceId);

        using var delete = await client.DeleteListAsync($"{ConfigurationsPath}/{copy.Id}", Cancellation);
        using var copyToRelease = await client.PostAsJsonAsync($"{ConfigurationsPath}/{copy.Id}/copies", new { release = "active" }, Cancellation);

        foreach (var response in new[] { delete, copyToRelease })
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Conflict, await response.Content.ReadAsStringAsync(Cancellation));
            (await TypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.ProjectCopy);
        }

        (await ReadProjectAsync(client, project.Id)).CriteriaList.ShouldBe(copy);
        await using var db = Configurator(Fixture.A.Id);
        (await db.CriteriaConfigurations.CountAsync(Cancellation)).ShouldBe(2);
    }

    [Fact(DisplayName = "INV:TEN-08 " + nameof(CreateShareLink_ProjectCopy_Returns409ProjectCopyAndStoresNoLink))]
    [Trait("Invariant", "TEN-08")]
    public async Task CreateShareLink_ProjectCopy_Returns409ProjectCopyAndStoresNoLink()
    {
        var (sourceId, _) = await SaveListAsync("Liste");
        var project = await StoreProjectAsync();
        using var client = CreateClient(TestUsers.OwnerA);
        var copy = await AssignAsync(client, project.Id, sourceId);
        var expiresOn = DateOnly.FromDateTime(Clock.GetUtcNow().UtcDateTime).AddDays(30).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        using var response = await client.PostAsJsonAsync($"{ConfigurationsPath}/{copy.Id}/share-links", new { expiresOn }, Cancellation);
        // The organisation list it came from can still be shared (only the copy is refused).
        using var ofSource = await client.PostAsJsonAsync($"{ConfigurationsPath}/{sourceId}/share-links", new { expiresOn }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict, await response.Content.ReadAsStringAsync(Cancellation));
        (await TypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.ProjectCopy);
        ofSource.StatusCode.ShouldBe(HttpStatusCode.Created);
        await using var platform = new PlatformDbContext(
            new DbContextOptionsBuilder<PlatformDbContext>().UseNpgsql(Api.ConnectionString).Options,
            FixedOrganizationContext.Of(Fixture.A.Id));
        (await platform.ShareLinks.Select(l => l.ResourceId).ToListAsync(Cancellation)).ShouldBe([sourceId]);
    }

    [Fact(DisplayName = "INV:SEC-10 " + nameof(Assign_UnconfirmedAddress_Returns403EmailUnconfirmedAndCopiesNothing))]
    [Trait("Invariant", "SEC-10")]
    public async Task Assign_UnconfirmedAddress_Returns403EmailUnconfirmedAndCopiesNothing()
    {
        await StoreAsync(OrganizationMembership.Create(Fixture.A.Id, TestUsers.ReaderB.Id, OrganizationRole.Editor, Clock.GetUtcNow(), Clock.GetUtcNow()));
        var (sourceId, _) = await SaveListAsync("Liste");
        var project = await StoreProjectAsync();
        using var client = CreateClient(TestUsers.ReaderB);

        using var response = await client.PutBelowCurrentAsync(CriteriaPath(project.Id), new { sourceConfigurationId = sourceId }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await TypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.EmailUnconfirmed);
        (await ReadProjectAsync(client, project.Id)).CriteriaList.ShouldBeNull();
    }

    private static string CriteriaPath(Guid projectId) => $"/api/v1/projects/{projectId}/criteria-configuration";

    private static string CriterionPath(Guid id, int revision) => $"{ConfigurationsPath}/{id}/criteria/{revision}";

    /// <summary>Saves a list of <c>ExportTestCatalog.WebSelection</c> through the API, as S12 does.</summary>
    private async Task<(Guid Id, JsonArray Criteria)> SaveListAsync(
        string name, Action<int, JsonObject>? adjust = null, TestUser? user = null, Guid? organizationId = null)
    {
        using var client = CreateClient(user ?? TestUsers.OwnerA);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        var body = Body("xlsx", MiniCatalog.T1Version, WebSelection, criteria);
        body.Remove("format");
        body["organizationId"] = organizationId ?? Fixture.A.Id;
        body["name"] = name;
        var index = 0;
        foreach (var entry in body["criteria"]!.AsArray())
        {
            adjust?.Invoke(index++, entry!.AsObject());
        }

        using var response = await client.PostAsJsonAsync(ConfigurationsPath, body, Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(Cancellation));
        return ((await response.Content.ReadFromJsonAsync<CreatedConfiguration>(Cancellation)).ShouldNotBeNull().Id, criteria);
    }

    private async Task<ProcurementProject> StoreProjectAsync(string name = "Projekt", ProjectStatus status = ProjectStatus.Open)
    {
        var project = ProcurementProject.Create(Fixture.A.Id, name, TestUsers.OwnerA.Id, Clock.GetUtcNow());
        project.ChangeStatus(status, Clock.GetUtcNow());
        await StoreAsync(project);
        return project;
    }

    private async Task<CandidateProduct> StoreCandidateAsync(ProcurementProject project)
    {
        var candidate = CandidateProduct.Create(project, new CandidateFacts("Produkt", null, null, null), Clock.GetUtcNow());
        await StoreAsync(candidate, CandidatePrivateSection.Of(candidate, null, null));
        return candidate;
    }

    private static async Task AssessAsync(HttpClient client, Guid candidateId, int revision, string value, string? note = null)
    {
        using var response = await ProjectLists.SetAssessmentAsync(client, candidateId, revision, value, Cancellation, note);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
    }

    private static async Task<CandidateAssessmentsResponse> ReadAssessmentsAsync(HttpClient client, Guid candidateId) =>
        (await client.GetFromJsonAsync<CandidateAssessmentsResponse>(ProjectLists.AssessmentsPath(candidateId), Cancellation)).ShouldNotBeNull();

    /// <summary>A workspace context of its own outside any request that sees <paramref name="organizationIds"/>.</summary>
    private WorkspaceDbContext Workspace(params Guid[] organizationIds) =>
        new(new DbContextOptionsBuilder<WorkspaceDbContext>().UseNpgsql(Api.ConnectionString).Options,
            FixedOrganizationContext.Of(organizationIds));

    private static async Task<ProjectCriteriaListResponse> AssignAsync(HttpClient client, Guid projectId, Guid sourceId)
    {
        using var response = await client.PutBelowCurrentAsync(CriteriaPath(projectId), new { sourceConfigurationId = sourceId }, Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        return (await response.Content.ReadFromJsonAsync<ProjectCriteriaListResponse>(Cancellation)).ShouldNotBeNull();
    }

    private static async Task<ProjectResponse> ReadProjectAsync(HttpClient client, Guid projectId) =>
        (await client.GetFromJsonAsync<ProjectResponse>($"/api/v1/projects/{projectId}", Cancellation)).ShouldNotBeNull();

    private static async Task<ConfigurationResponse> ReadAsync(HttpClient client, Guid id) =>
        (await client.GetFromJsonAsync<ConfigurationResponse>($"{ConfigurationsPath}/{id}", Cancellation)).ShouldNotBeNull();

    /// <summary>The list as the server answers it, to compare before and after.</summary>
    private static async Task<string> ReadRawAsync(HttpClient client, Guid id)
    {
        using var response = await client.GetAsync($"{ConfigurationsPath}/{id}", Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.Content.ReadAsStringAsync(Cancellation);
    }

    /// <summary>A context of its own outside any request that sees <paramref name="organizationIds"/>.</summary>
    private ConfiguratorDbContext Configurator(params Guid[] organizationIds) =>
        new(new DbContextOptionsBuilder<ConfiguratorDbContext>().UseNpgsql(Api.ConnectionString).Options,
            FixedOrganizationContext.Of(organizationIds));

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
