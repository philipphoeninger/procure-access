// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
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
/// The project endpoints (S6-11; SPEC R-PRJ-01, R-PRJ-05, §3, §7.2; ARCHITECTURE §7.2): listing the
/// projects of one organisation, creating, reading, renaming a project and changing its status;
/// an archived project only changes its status; nothing deletes a project. Every test starts from
/// the Tenants fixture with the accounts of its users stored (all confirmed but Reader B). The
/// authorisation matrix is <c>AuthorizationMatrixTests</c> (TEN-05).
/// </summary>
public sealed class ProjectEndpointsTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private Tenants? _tenants;

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        _tenants = Tenants.Build(Clock.GetUtcNow());
        // The accounts first: organisations and memberships reference them (foreign keys, S6-10b).
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => TestAccounts.Of(u, confirmed: u != TestUsers.ReaderB))]);
        await StoreAsync(_tenants.Entities);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Create_EditorOfA_StoresTheOpenProjectAndReadsItBack()
    {
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PostAsJsonAsync(ProjectsPath(Fixture.A.Id), new { name = "  Neues Bewerbermanagement 2027  " }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(Cancellation));
        var created = (await response.Content.ReadFromJsonAsync<ProjectResponse>(Cancellation)).ShouldNotBeNull();
        response.Headers.Location.ShouldNotBeNull().ToString().ShouldBe(ProjectPath(created.Id));
        var read = (await client.GetFromJsonAsync<ProjectResponse>(ProjectPath(created.Id), Cancellation)).ShouldNotBeNull();
        read.ShouldBeEquivalentTo(created);
        read.ShouldSatisfyAllConditions(
            p => p.OrganizationId.ShouldBe(Fixture.A.Id),
            p => p.Name.ShouldBe("Neues Bewerbermanagement 2027"),
            p => p.Status.ShouldBe("open"),
            p => p.ProductCount.ShouldBe(0),
            p => p.Candidates.ShouldBeEmpty(),
            p => p.CreatedAt.ShouldBe(Clock.GetUtcNow()),
            p => p.ModifiedAt.ShouldBe(Clock.GetUtcNow()));

        await using var db = Workspace(Fixture.A.Id);
        (await db.ProcurementProjects.SingleAsync(p => p.Id == created.Id, Cancellation)).CreatedByUserId.ShouldBe(TestUsers.EditorA.Id);
    }

    [Fact]
    public async Task Create_NameOf120Characters_Returns201()
    {
        using var client = CreateClient(TestUsers.OwnerA);

        using var response = await client.PostAsJsonAsync(ProjectsPath(Fixture.A.Id), new { name = new string('a', 120) }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Theory(DisplayName = "INV:API-01 " + nameof(Create_MissingEmptyOrTooLongName_Returns400AtNameAndStoresNothing))]
    [Trait("Invariant", "API-01")]
    [InlineData(null)]
    [InlineData("   ")]
    [InlineData("121")]
    public async Task Create_MissingEmptyOrTooLongName_Returns400AtNameAndStoresNothing(string? name)
    {
        using var client = CreateClient(TestUsers.EditorA);
        var sent = name == "121" ? new string('a', 121) : name;

        using var response = await client.PostAsJsonAsync(ProjectsPath(Fixture.A.Id), new { name = sent }, Cancellation);

        await ShouldBeValidationProblemAsync(response, "name");
        (await CountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Create_UnknownMemberOrMalformedOrganization_Returns400()
    {
        using var client = CreateClient(TestUsers.EditorA);

        using var unknownMember = await client.PostAsJsonAsync(
            ProjectsPath(Fixture.A.Id), new { name = "Projekt", status = "archived" }, Cancellation);
        using var malformed = await client.PostAsJsonAsync("/api/v1/organizations/keine-id/projects", new { name = "Projekt" }, Cancellation);

        unknownMember.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        malformed.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await CountAsync()).ShouldBe(0);
    }

    [Fact(DisplayName = "INV:SEC-10 " + nameof(CreateAndChange_UnconfirmedAddress_Return403EmailUnconfirmedAndChangeNothing))]
    [Trait("Invariant", "SEC-10")]
    public async Task CreateAndChange_UnconfirmedAddress_Return403EmailUnconfirmedAndChangeNothing()
    {
        await StoreAsync(OrganizationMembership.Create(Fixture.A.Id, TestUsers.ReaderB.Id, OrganizationRole.Editor, Clock.GetUtcNow(), Clock.GetUtcNow()));
        var project = await StoreProjectAsync(Fixture.A.Id, "Projekt");
        using var client = CreateClient(TestUsers.ReaderB);

        using var create = await client.PostAsJsonAsync(ProjectsPath(Fixture.A.Id), new { name = "Neu" }, Cancellation);
        using var rename = await client.PatchCurrentAsync(ProjectPath(project.Id), new { name = "Umbenannt" }, Cancellation);
        using var status = await client.PatchCurrentAsync(ProjectPath(project.Id), new { status = "done" }, Cancellation);
        // Reading needs no confirmed address.
        var read = await client.GetFromJsonAsync<ProjectResponse>(ProjectPath(project.Id), Cancellation);

        foreach (var response in new[] { create, rename, status })
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
            (await TypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.EmailUnconfirmed);
        }

        read.ShouldNotBeNull().ShouldSatisfyAllConditions(p => p.Name.ShouldBe("Projekt"), p => p.Status.ShouldBe("open"));
        (await CountAsync()).ShouldBe(1);
    }

    [Fact(DisplayName = "INV:SEC-10 " + nameof(Create_AfterConfirmingTheAddress_Succeeds))]
    [Trait("Invariant", "SEC-10")]
    public async Task Create_AfterConfirmingTheAddress_Succeeds()
    {
        await StoreAsync(OrganizationMembership.Create(Fixture.A.Id, TestUsers.ReaderB.Id, OrganizationRole.Editor, Clock.GetUtcNow(), Clock.GetUtcNow()));
        using var client = CreateClient(TestUsers.ReaderB);
        (await client.PostAsJsonAsync(ProjectsPath(Fixture.A.Id), new { name = "Neu" }, Cancellation)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        await TestAccounts.ConfirmAsync(Api.Factory.Services, TestUsers.ReaderB);
        using var response = await client.PostAsJsonAsync(ProjectsPath(Fixture.A.Id), new { name = "Neu" }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact(DisplayName = "INV:TEN-01 " + nameof(ReadAndChange_ProjectOfAnotherOrganization_Return404AndChangeNothing))]
    [Trait("Invariant", "TEN-01")]
    public async Task ReadAndChange_ProjectOfAnotherOrganization_Return404AndChangeNothing()
    {
        var ofB = await StoreProjectAsync(Fixture.B.Id, "Projekt von B");
        using var client = CreateClient(TestUsers.OwnerA);
        // Naming B in a header changes nothing: the organisation comes from the stored project.
        client.DefaultRequestHeaders.Add("X-Organization-Id", Fixture.B.Id.ToString());

        using var read = await client.GetAsync(ProjectPath(ofB.Id), Cancellation);
        using var change = await client.PatchCurrentAsync(ProjectPath(ofB.Id), new { name = "Übernommen", status = "archived" }, Cancellation);

        read.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        change.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        await using var db = Workspace(Fixture.B.Id);
        (await db.ProcurementProjects.SingleAsync(Cancellation)).ShouldSatisfyAllConditions(
            p => p.Name.ShouldBe("Projekt von B"), p => p.Status.ShouldBe(ProjectStatus.Open));
    }

    [Fact]
    public async Task Read_UnknownOrMalformedId_Returns404Or400()
    {
        using var client = CreateClient(TestUsers.ReaderA);

        using var unknown = await client.GetAsync(ProjectPath(Guid.NewGuid()), Cancellation);
        using var malformed = await client.GetAsync("/api/v1/projects/keine-id", Cancellation);

        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        malformed.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INV:TEN-01 " + nameof(List_Organization_ListsOnlyItsProjectsAlsoArchivedTheMostRecentlyChangedFirst))]
    [Trait("Invariant", "TEN-01")]
    public async Task List_Organization_ListsOnlyItsProjectsAlsoArchivedTheMostRecentlyChangedFirst()
    {
        var older = await StoreProjectAsync(Fixture.A.Id, "Älteres Projekt");
        Clock.Advance(TimeSpan.FromHours(1));
        var archived = await StoreProjectAsync(Fixture.A.Id, "Archiviertes Projekt", ProjectStatus.Archived);
        Clock.Advance(TimeSpan.FromHours(1));
        var newer = await StoreProjectAsync(Fixture.A.Id, "Neueres Projekt", ProjectStatus.Done);
        await StoreProjectAsync(Fixture.B.Id, "Projekt von B");
        using var client = CreateClient(TestUsers.ReaderA);

        var projects = await client.GetFromJsonAsync<List<ProjectSummaryResponse>>(ProjectsPath(Fixture.A.Id), Cancellation);

        projects.ShouldNotBeNull().ShouldBe(
        [
            new ProjectSummaryResponse(newer.Id, "Neueres Projekt", "done", 0, newer.ModifiedAt),
            new ProjectSummaryResponse(archived.Id, "Archiviertes Projekt", "archived", 0, archived.ModifiedAt),
            new ProjectSummaryResponse(older.Id, "Älteres Projekt", "open", 0, older.ModifiedAt),
        ]);
    }

    /// <summary>
    /// The organisation switch of R-SHL-03 (TESTING V232): a member of A and B gets the projects of
    /// the organisation the route names, never both.
    /// </summary>
    [Fact(DisplayName = "INV:TEN-01 " + nameof(List_MemberOfTwoOrganizations_ListsOnlyTheNamedOne))]
    [Trait("Invariant", "TEN-01")]
    public async Task List_MemberOfTwoOrganizations_ListsOnlyTheNamedOne()
    {
        await StoreAsync(OrganizationMembership.Create(Fixture.B.Id, TestUsers.EditorA.Id, OrganizationRole.Reader, Clock.GetUtcNow(), Clock.GetUtcNow()));
        var ofA = await StoreProjectAsync(Fixture.A.Id, "Projekt von A");
        var ofB = await StoreProjectAsync(Fixture.B.Id, "Projekt von B");
        using var client = CreateClient(TestUsers.EditorA);

        var a = await client.GetFromJsonAsync<List<ProjectSummaryResponse>>(ProjectsPath(Fixture.A.Id), Cancellation);
        var b = await client.GetFromJsonAsync<List<ProjectSummaryResponse>>(ProjectsPath(Fixture.B.Id), Cancellation);

        a.ShouldNotBeNull().Select(p => p.Id).ShouldBe([ofA.Id]);
        b.ShouldNotBeNull().Select(p => p.Id).ShouldBe([ofB.Id]);
    }

    [Fact]
    public async Task List_OrganizationWithoutProjects_ReturnsAnEmptyList()
    {
        using var client = CreateClient(TestUsers.ReaderA);

        var projects = await client.GetFromJsonAsync<List<ProjectSummaryResponse>>(ProjectsPath(Fixture.A.Id), Cancellation);

        projects.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task Change_NameAndStatus_ChangesBothAndTheLastChange()
    {
        var project = await StoreProjectAsync(Fixture.A.Id, "Projekt");
        Clock.Advance(TimeSpan.FromMinutes(5));
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PatchCurrentAsync(ProjectPath(project.Id), new { name = "  Umbenannt  ", status = "done" }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        var changed = (await response.Content.ReadFromJsonAsync<ProjectResponse>(Cancellation)).ShouldNotBeNull();
        changed.ShouldSatisfyAllConditions(
            p => p.Name.ShouldBe("Umbenannt"),
            p => p.Status.ShouldBe("done"),
            p => p.CreatedAt.ShouldBe(project.CreatedAt),
            p => p.ModifiedAt.ShouldBe(Clock.GetUtcNow()));
        (await client.GetFromJsonAsync<ProjectResponse>(ProjectPath(project.Id), Cancellation)).ShouldBeEquivalentTo(changed);
    }

    [Fact]
    public async Task Change_OnlyTheStatus_KeepsTheName()
    {
        var project = await StoreProjectAsync(Fixture.A.Id, "Projekt");
        using var client = CreateClient(TestUsers.OwnerA);

        using var response = await client.PatchCurrentAsync(ProjectPath(project.Id), new { status = "archived" }, Cancellation);

        var changed = (await response.Content.ReadFromJsonAsync<ProjectResponse>(Cancellation)).ShouldNotBeNull();
        changed.ShouldSatisfyAllConditions(p => p.Name.ShouldBe("Projekt"), p => p.Status.ShouldBe("archived"));
    }

    [Fact]
    public async Task Change_NothingNamedOrTheSameValues_KeepsTheLastChange()
    {
        var project = await StoreProjectAsync(Fixture.A.Id, "Projekt");
        Clock.Advance(TimeSpan.FromMinutes(5));
        using var client = CreateClient(TestUsers.EditorA);

        using var empty = await client.PatchCurrentAsync(ProjectPath(project.Id), new { }, Cancellation);
        using var same = await client.PatchCurrentAsync(ProjectPath(project.Id), new { name = "Projekt", status = "open" }, Cancellation);

        empty.StatusCode.ShouldBe(HttpStatusCode.OK);
        same.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await same.Content.ReadFromJsonAsync<ProjectResponse>(Cancellation)).ShouldNotBeNull().ModifiedAt.ShouldBe(project.ModifiedAt);
    }

    [Theory(DisplayName = "INV:API-01 " + nameof(Change_InvalidNameOrStatus_Returns400AtTheFieldAndChangesNothing))]
    [Trait("Invariant", "API-01")]
    [InlineData("   ", null, "name")]
    [InlineData("121", null, "name")]
    [InlineData(null, "Archiviert", "status")]
    [InlineData(null, "Open", "status")]
    [InlineData("121", "deleted", "name,status")]
    public async Task Change_InvalidNameOrStatus_Returns400AtTheFieldAndChangesNothing(string? name, string? status, string fields)
    {
        var project = await StoreProjectAsync(Fixture.A.Id, "Projekt");
        using var client = CreateClient(TestUsers.EditorA);
        var sent = name == "121" ? new string('a', 121) : name;

        using var response = await client.PatchCurrentAsync(ProjectPath(project.Id), new { name = sent, status }, Cancellation);

        await ShouldBeValidationProblemAsync(response, fields.Split(','));
        (await client.GetFromJsonAsync<ProjectResponse>(ProjectPath(project.Id), Cancellation)).ShouldNotBeNull()
            .ShouldSatisfyAllConditions(p => p.Name.ShouldBe("Projekt"), p => p.Status.ShouldBe("open"));
    }

    /// <summary>R-PRJ-05: an archived project is read-only for everyone, the Owner too.</summary>
    [Theory]
    [InlineData("owner-a", false)]
    [InlineData("editor-a", false)]
    [InlineData("owner-a", true)]
    public async Task Change_NameOfAnArchivedProject_Returns409ProjectArchivedAndChangesNothing(string userName, bool withStatus)
    {
        var project = await StoreProjectAsync(Fixture.A.Id, "Projekt", ProjectStatus.Archived);
        using var client = CreateClient(TestUsers.ByName[userName]);
        object body = withStatus ? new { name = "Umbenannt", status = "open" } : new { name = "Umbenannt" };

        using var response = await client.PatchCurrentAsync(ProjectPath(project.Id), body, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await TypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.ProjectArchived);
        (await client.GetFromJsonAsync<ProjectResponse>(ProjectPath(project.Id), Cancellation)).ShouldNotBeNull()
            .ShouldSatisfyAllConditions(
                p => p.Name.ShouldBe("Projekt"),
                p => p.Status.ShouldBe("archived"),
                p => p.ModifiedAt.ShouldBe(project.ModifiedAt));
    }

    [Theory]
    [InlineData("open")]
    [InlineData("done")]
    public async Task Change_StatusOfAnArchivedProject_ReopensItForARename(string status)
    {
        var project = await StoreProjectAsync(Fixture.A.Id, "Projekt", ProjectStatus.Archived);
        using var client = CreateClient(TestUsers.EditorA);

        using var reopened = await client.PatchCurrentAsync(ProjectPath(project.Id), new { status }, Cancellation);
        using var renamed = await client.PatchCurrentAsync(ProjectPath(project.Id), new { name = "Umbenannt" }, Cancellation);

        reopened.StatusCode.ShouldBe(HttpStatusCode.OK);
        renamed.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await renamed.Content.ReadFromJsonAsync<ProjectResponse>(Cancellation)).ShouldNotBeNull()
            .ShouldSatisfyAllConditions(p => p.Name.ShouldBe("Umbenannt"), p => p.Status.ShouldBe(status));
    }

    /// <summary>SPEC §3: projects are archived, never deleted; no route answers DELETE.</summary>
    [Fact]
    public async Task Delete_Project_HasNoEndpointAndKeepsTheProject()
    {
        var project = await StoreProjectAsync(Fixture.A.Id, "Projekt");
        using var client = CreateClient(TestUsers.OwnerA);

        using var response = await client.DeleteAsync(ProjectPath(project.Id), Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.MethodNotAllowed);
        (await CountAsync()).ShouldBe(1);
        Api.Factory.Services.GetRequiredService<EndpointDataSource>().Endpoints.OfType<RouteEndpoint>()
           .Where(e => e.RoutePattern.RawText?.Contains("/projects", StringComparison.Ordinal) == true)
           .SelectMany(e => e.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods ?? [])
           .ShouldNotContain(HttpMethods.Delete);
    }

    private static string ProjectsPath(Guid organizationId) => $"/api/v1/organizations/{organizationId}/projects";

    private static string ProjectPath(Guid id) => $"/api/v1/projects/{id}";

    /// <summary>Stores a project at the clock's time; its status change, if any, at the same time.</summary>
    private async Task<ProcurementProject> StoreProjectAsync(Guid organizationId, string name, ProjectStatus status = ProjectStatus.Open)
    {
        var project = ProcurementProject.Create(organizationId, name, TestUsers.OwnerA.Id, Clock.GetUtcNow());
        project.ChangeStatus(status, Clock.GetUtcNow());
        await StoreAsync(project);
        return project;
    }

    private async Task<int> CountAsync()
    {
        await using var db = Workspace(Fixture.A.Id, Fixture.B.Id);
        return await db.ProcurementProjects.CountAsync(Cancellation);
    }

    /// <summary>A context of its own outside any request that sees <paramref name="organizationIds"/>.</summary>
    private WorkspaceDbContext Workspace(params Guid[] organizationIds) =>
        new(new DbContextOptionsBuilder<WorkspaceDbContext>().UseNpgsql(Api.ConnectionString).Options,
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
