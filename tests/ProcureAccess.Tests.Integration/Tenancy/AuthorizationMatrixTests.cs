// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using ProcureAccess.Modules.IdentityAccess.Organizations;
using ProcureAccess.Modules.Catalog;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Configurations;
using ProcureAccess.Modules.Configurator.Domain;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.Platform.Contracts.Sharing;
using ProcureAccess.Modules.Platform.Sharing;
using ProcureAccess.Modules.Workspace.Domain;
using ProcureAccess.Tests.Integration.Export;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Tenancy;

/// <summary>
/// TEN-05 (ARCHITECTURE §9.1, SPEC R-ORG-05): the matrix Owner / Editor / Reader of organisation A,
/// Owner of B, user without organisation, anonymous × every organisation endpoint on organisation A,
/// with the expected status codes (non-member 404, member below the role 403, anonymous 401). The
/// organisation comes from the route (the resource); a client-supplied organisation header changes
/// nothing. Since S5-16a on the real organisation endpoints (the test-only probe of S5-15 is gone),
/// since S5-17a with the invitation endpoints, since S5-18 with the list of the user's own
/// organisations (every signed-in user; which ones it lists is TEN-01 in <c>OrganizationEndpointsTests</c>), since S5-20a
/// with the saved criteria lists (save: Editor; read one, list: Reader; the organisation named in the body or query, or taken
/// from the stored list), since S5-21a with changing, deleting and copying a list (Editor, the organisation taken from the stored
/// list) and its export (every member); since S5-23 with the share links of a list (Editor; the organisation taken from the stored
/// list or link); since S5-22 every answer of the matrix also carries <c>no-store</c>
/// (TEN-04 workspace part); since S6-11 with the procurement projects (list, read: Reader; create, rename, status: Editor; the
/// organisation named in the route or taken from the stored project); since S6-15 with the candidates and their evidences (read:
/// Reader; create, change, test status, add, change and remove an evidence: Editor; the organisation taken from the stored
/// project, candidate or evidence); since S6-12 with the project's copy of a list (Editor); since S6-18 with the assessments of a
/// candidate (read: Reader; set: Editor — the project of the matrix has no list, so a role that may set one gets the 400 of the
/// check that follows the role check; the 200 is <c>AssessmentEndpointsTests</c>). Every user has a confirmed account, so the gate of saving
/// (SEC-10) passes; the other workspace endpoints add their rows with their tasks of step 5.
/// </summary>
public sealed class AuthorizationMatrixTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Anonymous = "anonymous";

    /// <summary><c>GET /organizations</c>, the user's own organisations (S5-18): every signed-in user.</summary>
    private const string ListMine = "list-mine";

    /// <summary><c>GET members</c>: Reader and up.</summary>
    private const string Read = "read";

    /// <summary><c>PATCH members/{reader-a}</c> to Editor: Owner only.</summary>
    private const string ChangeRole = "change-role";

    /// <summary><c>DELETE members/{someone else of A}</c>: Owner only.</summary>
    private const string RemoveOther = "remove-other";

    /// <summary><c>DELETE members/{own id}</c> (leave): every member; the last Owner gets 409 (TEN-11).</summary>
    private const string Leave = "leave";

    /// <summary><c>POST invitations</c> (S5-17a): Owner only.</summary>
    private const string Invite = "invite";

    /// <summary><c>GET invitations</c>, the pending list (S5-17a): Owner only (owner decision S5-17 D4 (a)).</summary>
    private const string ListInvitations = "list-invitations";

    /// <summary><c>DELETE invitations/{id}</c> of an open invitation of A (S5-17a): Owner only.</summary>
    private const string RevokeInvitation = "revoke-invitation";

    /// <summary><c>POST /configurations</c> for A (S5-20a): Editor and Owner.</summary>
    private const string SaveList = "save-list";

    /// <summary><c>GET /configurations/{id}</c> of a list of A (S5-20a): every member of A.</summary>
    private const string ReadList = "read-list";

    /// <summary><c>GET /configurations?organizationId=A</c> (S5-20a): Reader and up.</summary>
    private const string ListLists = "list-lists";

    /// <summary><c>PATCH /configurations/{id}/criteria/{revision}</c> of a list of A (S5-21a): Editor and Owner.</summary>
    private const string DecideInList = "decide-in-list";

    /// <summary><c>PUT /configurations/{id}/selection</c> of a list of A (S5-21a): Editor and Owner.</summary>
    private const string ChangeSelection = "change-selection";

    /// <summary><c>DELETE /configurations/{id}</c> of a list of A (S5-21a): Editor and Owner.</summary>
    private const string DeleteList = "delete-list";

    /// <summary><c>POST /configurations/{id}/copies</c> of a list of A on <c>T.1</c> with <c>T.2</c> active (S5-21a): Editor and Owner.</summary>
    private const string CopyList = "copy-list";

    /// <summary><c>POST /configurations/{id}/exports</c> of a list of A (S5-21a): every member of A.</summary>
    private const string ExportList = "export-list";

    /// <summary><c>POST /configurations/{id}/share-links</c> of a list of A (S5-23): Editor and Owner.</summary>
    private const string CreateShareLink = "create-share-link";

    /// <summary><c>GET /configurations/{id}/share-links</c> of a list of A (S5-23): Editor and Owner (owner decision S5-23 4a).</summary>
    private const string ListShareLinks = "list-share-links";

    /// <summary><c>DELETE /share-links/{id}</c> of a link of A (S5-23): Editor and Owner.</summary>
    private const string RevokeShareLink = "revoke-share-link";

    /// <summary><c>GET /organizations/{A}/projects</c> (S6-11): Reader and up.</summary>
    private const string ListProjects = "list-projects";

    /// <summary><c>POST /organizations/{A}/projects</c> (S6-11): Editor and Owner.</summary>
    private const string CreateProject = "create-project";

    /// <summary><c>GET /projects/{id}</c> of a project of A (S6-11): every member of A.</summary>
    private const string ReadProject = "read-project";

    /// <summary><c>PATCH /projects/{id}</c> with a name, of a project of A (S6-11): Editor and Owner.</summary>
    private const string RenameProject = "rename-project";

    /// <summary><c>PATCH /projects/{id}</c> with a status, of a project of A (S6-11): Editor and Owner.</summary>
    private const string ChangeProjectStatus = "change-project-status";

    /// <summary><c>PUT /projects/{id}/criteria-configuration</c> with a list of A, of a project of A (S6-12): Editor and Owner.</summary>
    private const string AssignProjectList = "assign-project-list";

    /// <summary><c>POST /projects/{id}/candidates</c> of a project of A (S6-15): Editor and Owner.</summary>
    private const string CreateCandidate = "create-candidate";

    /// <summary><c>GET /candidates/{id}</c> of a candidate of A (S6-15): every member of A.</summary>
    private const string ReadCandidate = "read-candidate";

    /// <summary><c>PATCH /candidates/{id}</c> of a candidate of A (S6-15): Editor and Owner.</summary>
    private const string ChangeCandidate = "change-candidate";

    /// <summary><c>PUT /candidates/{id}/test-status</c> of a candidate of A (S6-15): Editor and Owner.</summary>
    private const string SetTestStatus = "set-test-status";

    /// <summary><c>POST /candidates/{id}/test-evidences</c> of a candidate of A (S6-15): Editor and Owner.</summary>
    private const string AddEvidence = "add-evidence";

    /// <summary><c>PUT /test-evidences/{id}</c> of an evidence of A (S6-15): Editor and Owner.</summary>
    private const string ChangeEvidence = "change-evidence";

    /// <summary><c>DELETE /test-evidences/{id}</c> of an evidence of A (S6-15): Editor and Owner.</summary>
    private const string RemoveEvidence = "remove-evidence";

    /// <summary><c>GET /candidates/{id}/assessments</c> of a candidate of A (S6-18): every member of A.</summary>
    private const string ReadAssessments = "read-assessments";

    /// <summary><c>PUT /candidates/{id}/assessments/{criterionRevisionId}</c> of a candidate of A (S6-18): Editor and Owner.</summary>
    private const string SetAssessment = "set-assessment";

    /// <summary>The revision of the one criterion of <see cref="_listOfA"/>; the domain needs no catalog for it.</summary>
    private const int RevisionOfListOfA = 1;

    private OrganizationInvitation? _invitation;

    private CriteriaConfiguration? _listOfA;

    private ShareLink? _linkOfA;

    private ProcurementProject? _projectOfA;

    /// <summary>A candidate of <see cref="_projectOfA"/> with the status „Ja“ and <see cref="_evidenceOfA"/>.</summary>
    private CandidateProduct? _candidateOfA;

    private TestEvidence? _evidenceOfA;

    private bool _catalogImported;

    private Tenants? _tenants;

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    public static TheoryData<string, string, HttpStatusCode> Matrix => new()
    {
        { TestUsers.OwnerA.Name, ListMine, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, ListMine, HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, ListMine, HttpStatusCode.OK },
        { TestUsers.OwnerB.Name, ListMine, HttpStatusCode.OK },
        { TestUsers.Member.Name, ListMine, HttpStatusCode.OK },
        { Anonymous, ListMine, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, Read, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, Read, HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, Read, HttpStatusCode.OK },
        { TestUsers.OwnerB.Name, Read, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, Read, HttpStatusCode.NotFound },
        { Anonymous, Read, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, ChangeRole, HttpStatusCode.NoContent },
        { TestUsers.EditorA.Name, ChangeRole, HttpStatusCode.Forbidden },
        { TestUsers.ReaderA.Name, ChangeRole, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, ChangeRole, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, ChangeRole, HttpStatusCode.NotFound },
        { Anonymous, ChangeRole, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, RemoveOther, HttpStatusCode.NoContent },
        { TestUsers.EditorA.Name, RemoveOther, HttpStatusCode.Forbidden },
        { TestUsers.ReaderA.Name, RemoveOther, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, RemoveOther, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, RemoveOther, HttpStatusCode.NotFound },
        { Anonymous, RemoveOther, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, Leave, HttpStatusCode.Conflict },
        { TestUsers.EditorA.Name, Leave, HttpStatusCode.NoContent },
        { TestUsers.ReaderA.Name, Leave, HttpStatusCode.NoContent },
        { TestUsers.OwnerB.Name, Leave, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, Leave, HttpStatusCode.NotFound },
        { TestUsers.OwnerA.Name, Invite, HttpStatusCode.Created },
        { TestUsers.EditorA.Name, Invite, HttpStatusCode.Forbidden },
        { TestUsers.ReaderA.Name, Invite, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, Invite, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, Invite, HttpStatusCode.NotFound },
        { Anonymous, Invite, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, ListInvitations, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, ListInvitations, HttpStatusCode.Forbidden },
        { TestUsers.ReaderA.Name, ListInvitations, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, ListInvitations, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, ListInvitations, HttpStatusCode.NotFound },
        { Anonymous, ListInvitations, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, RevokeInvitation, HttpStatusCode.NoContent },
        { TestUsers.EditorA.Name, RevokeInvitation, HttpStatusCode.Forbidden },
        { TestUsers.ReaderA.Name, RevokeInvitation, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, RevokeInvitation, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, RevokeInvitation, HttpStatusCode.NotFound },
        { Anonymous, RevokeInvitation, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, SaveList, HttpStatusCode.Created },
        { TestUsers.EditorA.Name, SaveList, HttpStatusCode.Created },
        { TestUsers.ReaderA.Name, SaveList, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, SaveList, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, SaveList, HttpStatusCode.NotFound },
        { Anonymous, SaveList, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, ReadList, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, ReadList, HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, ReadList, HttpStatusCode.OK },
        { TestUsers.OwnerB.Name, ReadList, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, ReadList, HttpStatusCode.NotFound },
        { Anonymous, ReadList, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, ListLists, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, ListLists, HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, ListLists, HttpStatusCode.OK },
        { TestUsers.OwnerB.Name, ListLists, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, ListLists, HttpStatusCode.NotFound },
        { Anonymous, ListLists, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, DecideInList, HttpStatusCode.NoContent },
        { TestUsers.EditorA.Name, DecideInList, HttpStatusCode.NoContent },
        { TestUsers.ReaderA.Name, DecideInList, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, DecideInList, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, DecideInList, HttpStatusCode.NotFound },
        { Anonymous, DecideInList, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, ChangeSelection, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, ChangeSelection, HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, ChangeSelection, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, ChangeSelection, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, ChangeSelection, HttpStatusCode.NotFound },
        { Anonymous, ChangeSelection, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, DeleteList, HttpStatusCode.NoContent },
        { TestUsers.EditorA.Name, DeleteList, HttpStatusCode.NoContent },
        { TestUsers.ReaderA.Name, DeleteList, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, DeleteList, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, DeleteList, HttpStatusCode.NotFound },
        { Anonymous, DeleteList, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, CopyList, HttpStatusCode.Created },
        { TestUsers.EditorA.Name, CopyList, HttpStatusCode.Created },
        { TestUsers.ReaderA.Name, CopyList, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, CopyList, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, CopyList, HttpStatusCode.NotFound },
        { Anonymous, CopyList, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, ExportList, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, ExportList, HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, ExportList, HttpStatusCode.OK },
        { TestUsers.OwnerB.Name, ExportList, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, ExportList, HttpStatusCode.NotFound },
        { Anonymous, ExportList, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, CreateShareLink, HttpStatusCode.Created },
        { TestUsers.EditorA.Name, CreateShareLink, HttpStatusCode.Created },
        { TestUsers.ReaderA.Name, CreateShareLink, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, CreateShareLink, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, CreateShareLink, HttpStatusCode.NotFound },
        { Anonymous, CreateShareLink, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, ListShareLinks, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, ListShareLinks, HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, ListShareLinks, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, ListShareLinks, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, ListShareLinks, HttpStatusCode.NotFound },
        { Anonymous, ListShareLinks, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, RevokeShareLink, HttpStatusCode.NoContent },
        { TestUsers.EditorA.Name, RevokeShareLink, HttpStatusCode.NoContent },
        { TestUsers.ReaderA.Name, RevokeShareLink, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, RevokeShareLink, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, RevokeShareLink, HttpStatusCode.NotFound },
        { Anonymous, RevokeShareLink, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, ListProjects, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, ListProjects, HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, ListProjects, HttpStatusCode.OK },
        { TestUsers.OwnerB.Name, ListProjects, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, ListProjects, HttpStatusCode.NotFound },
        { Anonymous, ListProjects, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, CreateProject, HttpStatusCode.Created },
        { TestUsers.EditorA.Name, CreateProject, HttpStatusCode.Created },
        { TestUsers.ReaderA.Name, CreateProject, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, CreateProject, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, CreateProject, HttpStatusCode.NotFound },
        { Anonymous, CreateProject, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, ReadProject, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, ReadProject, HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, ReadProject, HttpStatusCode.OK },
        { TestUsers.OwnerB.Name, ReadProject, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, ReadProject, HttpStatusCode.NotFound },
        { Anonymous, ReadProject, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, RenameProject, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, RenameProject, HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, RenameProject, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, RenameProject, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, RenameProject, HttpStatusCode.NotFound },
        { Anonymous, RenameProject, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, ChangeProjectStatus, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, ChangeProjectStatus, HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, ChangeProjectStatus, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, ChangeProjectStatus, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, ChangeProjectStatus, HttpStatusCode.NotFound },
        { Anonymous, ChangeProjectStatus, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, AssignProjectList, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, AssignProjectList, HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, AssignProjectList, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, AssignProjectList, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, AssignProjectList, HttpStatusCode.NotFound },
        { Anonymous, AssignProjectList, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, CreateCandidate, HttpStatusCode.Created },
        { TestUsers.EditorA.Name, CreateCandidate, HttpStatusCode.Created },
        { TestUsers.ReaderA.Name, CreateCandidate, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, CreateCandidate, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, CreateCandidate, HttpStatusCode.NotFound },
        { Anonymous, CreateCandidate, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, ReadCandidate, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, ReadCandidate, HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, ReadCandidate, HttpStatusCode.OK },
        { TestUsers.OwnerB.Name, ReadCandidate, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, ReadCandidate, HttpStatusCode.NotFound },
        { Anonymous, ReadCandidate, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, ChangeCandidate, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, ChangeCandidate, HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, ChangeCandidate, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, ChangeCandidate, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, ChangeCandidate, HttpStatusCode.NotFound },
        { Anonymous, ChangeCandidate, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, SetTestStatus, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, SetTestStatus, HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, SetTestStatus, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, SetTestStatus, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, SetTestStatus, HttpStatusCode.NotFound },
        { Anonymous, SetTestStatus, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, AddEvidence, HttpStatusCode.Created },
        { TestUsers.EditorA.Name, AddEvidence, HttpStatusCode.Created },
        { TestUsers.ReaderA.Name, AddEvidence, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, AddEvidence, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, AddEvidence, HttpStatusCode.NotFound },
        { Anonymous, AddEvidence, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, ChangeEvidence, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, ChangeEvidence, HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, ChangeEvidence, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, ChangeEvidence, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, ChangeEvidence, HttpStatusCode.NotFound },
        { Anonymous, ChangeEvidence, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, RemoveEvidence, HttpStatusCode.NoContent },
        { TestUsers.EditorA.Name, RemoveEvidence, HttpStatusCode.NoContent },
        { TestUsers.ReaderA.Name, RemoveEvidence, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, RemoveEvidence, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, RemoveEvidence, HttpStatusCode.NotFound },
        { Anonymous, RemoveEvidence, HttpStatusCode.Unauthorized },
        { TestUsers.OwnerA.Name, ReadAssessments, HttpStatusCode.OK },
        { TestUsers.EditorA.Name, ReadAssessments, HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, ReadAssessments, HttpStatusCode.OK },
        { TestUsers.OwnerB.Name, ReadAssessments, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, ReadAssessments, HttpStatusCode.NotFound },
        { Anonymous, ReadAssessments, HttpStatusCode.Unauthorized },
        // Past the role check: the project has no list, so the criterion is refused (400).
        { TestUsers.OwnerA.Name, SetAssessment, HttpStatusCode.BadRequest },
        { TestUsers.EditorA.Name, SetAssessment, HttpStatusCode.BadRequest },
        { TestUsers.ReaderA.Name, SetAssessment, HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, SetAssessment, HttpStatusCode.NotFound },
        { TestUsers.Member.Name, SetAssessment, HttpStatusCode.NotFound },
        { Anonymous, SetAssessment, HttpStatusCode.Unauthorized },
    };

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        _tenants = Tenants.Build(Clock.GetUtcNow());
        // The accounts first: organisations and memberships reference them (foreign keys, S6-10b).
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => TestAccounts.Of(u, confirmed: true))]);
        _invitation = OrganizationInvitation.Create(
            _tenants.A.Id, "eingeladen@example.test", OrganizationRole.Reader, new byte[32], Clock.GetUtcNow(), TimeSpan.FromDays(14));
        // A list of A to read; the domain needs no catalog for it, the save rows import one when they run.
        _listOfA = CriteriaConfiguration.Create(
            _tenants.A.Id, "Liste von A", TestUsers.OwnerA.Id, Clock.GetUtcNow(), MiniCatalog.T1Version, null, ["web"], [], [],
            [new ConfigurationCriterionDraft(RevisionOfListOfA, PriorityLevel.B)]);
        _linkOfA = ShareLink.Create(
            _tenants.A.Id, ShareableResourceType.Configuration, _listOfA.Id, new byte[32], Clock.GetUtcNow(), Clock.GetUtcNow().AddDays(30));
        _projectOfA = ProcurementProject.Create(_tenants.A.Id, "Projekt von A", TestUsers.OwnerA.Id, Clock.GetUtcNow());
        _candidateOfA = CandidateProduct.Create(_projectOfA, new CandidateFacts("Produkt von A", null, null, null), Clock.GetUtcNow());
        _candidateOfA.ChangeTestStatus(TestStatus.Tested, removeEvidences: false, Clock.GetUtcNow());
        _evidenceOfA = _candidateOfA.AddEvidence(EvidenceOfA, Clock.GetUtcNow()).Evidence!;
        await StoreAsync(
        [
            .. _tenants.Entities, _invitation, _listOfA, _linkOfA, _projectOfA, _candidateOfA,
            CandidatePrivateSection.Of(_candidateOfA, null, null), TestEvidencePrivateNote.Of(_evidenceOfA, null),
        ]);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Theory(DisplayName = "INV:TEN-05 " + nameof(Endpoint_UserAndAction_AnswersTheMatrixStatus))]
    [Trait("Invariant", "TEN-05")]
    [MemberData(nameof(Matrix))]
    public async Task Endpoint_UserAndAction_AnswersTheMatrixStatus(string userName, string action, HttpStatusCode expected)
    {
        var user = userName == Anonymous ? null : TestUsers.ByName[userName];
        using var client = CreateClient(user);

        using var response = await SendAsync(client, action, user);

        response.StatusCode.ShouldBe(expected);
    }

    /// <summary>
    /// TEN-04 workspace part (S5-22): every answer of the matrix — success, 401, 403 and 404 alike —
    /// carries <c>no-store</c>, from the platform default or, for the export, its own header.
    /// </summary>
    [Theory(DisplayName = "INV:TEN-04 " + nameof(Endpoint_UserAndAction_AnswerCarriesNoStore))]
    [Trait("Invariant", "TEN-04")]
    [MemberData(nameof(Matrix))]
    public async Task Endpoint_UserAndAction_AnswerCarriesNoStore(string userName, string action, HttpStatusCode expected)
    {
        var user = userName == Anonymous ? null : TestUsers.ByName[userName];
        using var client = CreateClient(user);

        using var response = await SendAsync(client, action, user);

        response.StatusCode.ShouldBe(expected);
        response.Headers.CacheControl.ShouldNotBeNull().NoStore.ShouldBeTrue();
    }

    [Fact(DisplayName = "INV:TEN-05 " + nameof(Read_MemberOfA_SeesTheOrganizationName))]
    [Trait("Invariant", "TEN-05")]
    public async Task Read_MemberOfA_SeesTheOrganizationName()
    {
        using var client = CreateClient(TestUsers.ReaderA);

        var answer = await client.GetFromJsonAsync<OrganizationMembersResponse>(MembersPath(Fixture.A.Id), Cancellation);

        answer.ShouldNotBeNull().Organization.Name.ShouldBe("Organisation A");
    }

    [Theory(DisplayName = "INV:TEN-05 " + nameof(Endpoint_ClientOrganizationHeader_HasNoEffect))]
    [Trait("Invariant", "TEN-05")]
    [InlineData("owner-b", "A", HttpStatusCode.NotFound)]   // names A in the header, is no member of A
    [InlineData("reader-a", "B", HttpStatusCode.OK)]        // names B in the header, reads A through the route
    [InlineData("owner-a", "B", HttpStatusCode.OK)]
    public async Task Endpoint_ClientOrganizationHeader_HasNoEffect(string userName, string headerOrganization, HttpStatusCode expected)
    {
        using var client = CreateClient(TestUsers.ByName[userName]);
        var named = headerOrganization == "A" ? Fixture.A.Id : Fixture.B.Id;
        client.DefaultRequestHeaders.Add("X-Organization-Id", named.ToString());

        using var response = await client.GetAsync(MembersPath(Fixture.A.Id), Cancellation);

        response.StatusCode.ShouldBe(expected);
    }

    [Theory(DisplayName = "INV:TEN-05 " + nameof(Change_MemberOfAnotherOrganizationAsTarget_Returns404))]
    [Trait("Invariant", "TEN-05")]
    [InlineData("PATCH")]
    [InlineData("DELETE")]
    public async Task Change_MemberOfAnotherOrganizationAsTarget_Returns404(string method)
    {
        // Owner of A names a member of B on A's route: no such member of A exists.
        using var client = CreateClient(TestUsers.OwnerA);
        using var request = new HttpRequestMessage(new HttpMethod(method), MemberPath(Fixture.A.Id, TestUsers.ReaderB.Id))
        {
            Content = method == "PATCH" ? JsonContent.Create(new { role = "editor" }) : null,
        };

        using var response = await client.SendAsync(request, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory(DisplayName = "INV:TEN-01 " + nameof(Members_Request_ListsOnlyTheUsersOrganization))]
    [Trait("Invariant", "TEN-01")]
    [InlineData("owner-a", "A")]
    [InlineData("reader-b", "B")]
    public async Task Members_Request_ListsOnlyTheUsersOrganization(string userName, string organization)
    {
        using var client = CreateClient(TestUsers.ByName[userName]);
        var own = organization == "A" ? Fixture.A : Fixture.B;
        var other = organization == "A" ? Fixture.B : Fixture.A;
        // Naming the other organisation changes nothing: the filter follows the memberships.
        client.DefaultRequestHeaders.Add("X-Organization-Id", other.Id.ToString());

        var answer = await client.GetFromJsonAsync<OrganizationMembersResponse>(MembersPath(own.Id), Cancellation);
        using var foreign = await client.GetAsync(MembersPath(other.Id), Cancellation);

        var expected = Fixture.Memberships.Where(m => m.OrganizationId == own.Id).Select(m => m.UserId).Order(StringComparer.Ordinal);
        answer.ShouldNotBeNull().Members.Select(m => m.UserId).Order(StringComparer.Ordinal).ShouldBe(expected);
        foreign.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "INV:TEN-01 " + nameof(Members_UserWithoutOrganization_SeesNoOrganization))]
    [Trait("Invariant", "TEN-01")]
    public async Task Members_UserWithoutOrganization_SeesNoOrganization()
    {
        using var client = CreateClient(TestUsers.Member);

        using var a = await client.GetAsync(MembersPath(Fixture.A.Id), Cancellation);
        using var b = await client.GetAsync(MembersPath(Fixture.B.Id), Cancellation);

        a.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        b.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private static string MembersPath(Guid organizationId) => $"/api/v1/organizations/{organizationId}/members";

    private static string MemberPath(Guid organizationId, string userId) => $"{MembersPath(organizationId)}/{userId}";

    private static string InvitationsPath(Guid organizationId) => $"/api/v1/organizations/{organizationId}/invitations";

    private const string ConfigurationsPath = "/api/v1/configurations";

    private static string ProjectsPath(Guid organizationId) => $"/api/v1/organizations/{organizationId}/projects";

    private string ProjectOfAPath => $"/api/v1/projects/{_projectOfA!.Id}";

    private string CandidateOfAPath => $"/api/v1/candidates/{_candidateOfA!.Id}";

    private string EvidenceOfAPath => $"/api/v1/test-evidences/{_evidenceOfA!.Id}";

    private static TestEvidenceFacts EvidenceOfA =>
        new(TestKind.Procedure, "softwareTest", null, "Prüfstelle", new DateOnly(2026, 2, 1), null, null);

    private static object EvidenceBody(string testKind, string? testTypeId = null) =>
        new { testKind, testTypeId, testedBy = "Prüfstelle", testDate = "2026-02-01" };

    /// <summary>A valid save of the whole <c>T.1</c> draft for A; imports the test releases on first use.</summary>
    private async Task<JsonObject> SaveBodyAsync(HttpClient client)
    {
        if (!_catalogImported)
        {
            await ExportTestCatalog.ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
            _catalogImported = true;
        }

        // The anonymous criteria endpoint, so every row builds the same body, the anonymous one too.
        using var anonymous = CreateClient();
        var criteria = await ExportTestCatalog.CriteriaAsync(anonymous, MiniCatalog.T1Version, ExportTestCatalog.WebSelection, Cancellation);
        var body = ExportTestCatalog.Body("xlsx", MiniCatalog.T1Version, ExportTestCatalog.WebSelection, criteria);
        body.Remove("format");
        body["organizationId"] = Fixture.A.Id;
        body["name"] = "Neue Liste";
        return body;
    }

    /// <summary>
    /// A list of A saved through the API with the whole <c>T.1</c> draft — a list the catalog resolves,
    /// which a selection change, a copy and an export need —, with <paramref name="activeVersion"/>
    /// active afterwards.
    /// </summary>
    private async Task<Guid> SavedListOfAAsync(string activeVersion)
    {
        using var owner = CreateClient(TestUsers.OwnerA);
        using var response = await owner.PostAsJsonAsync(ConfigurationsPath, await SaveBodyAsync(owner), Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(Cancellation));
        var id = (await response.Content.ReadFromJsonAsync<CreatedConfiguration>(Cancellation)).ShouldNotBeNull().Id;
        if (activeVersion != MiniCatalog.T1Version)
        {
            var error = new StringWriter();
            var exitCode = await CatalogModule.RunCommandAsync(
                Api.Factory.Services, ["catalog", "activate", activeVersion], TextWriter.Null, error, Cancellation);
            exitCode.ShouldBe(0, error.ToString());
        }

        return id;
    }

    /// <summary>The body of a selection change of a <c>T.1</c> list: the web selection without a test type.</summary>
    private static JsonObject ChangeSelectionBody() => new()
    {
        ["selection"] = new JsonObject
        {
            ["releaseVersion"] = MiniCatalog.T1Version,
            ["useCaseTemplateId"] = "hr",
            ["appTypeIds"] = new JsonArray("web"),
            ["productPartIds"] = new JsonArray("base"),
            ["testTypeIds"] = new JsonArray(),
        },
    };

    /// <summary>The request of <paramref name="action"/> on organisation A for <paramref name="user"/>.</summary>
    private async Task<HttpResponseMessage> SendAsync(HttpClient client, string action, TestUser? user)
    {
        var a = Fixture.A.Id;
        return action switch
        {
            SaveList => await client.PostAsJsonAsync(ConfigurationsPath, await SaveBodyAsync(client), Cancellation),
            ReadList => await client.GetAsync($"{ConfigurationsPath}/{_listOfA!.Id}", Cancellation),
            ListLists => await client.GetAsync($"{ConfigurationsPath}?organizationId={a}", Cancellation),
            DecideInList => await client.PatchAsJsonAsync(
                $"{ConfigurationsPath}/{_listOfA!.Id}/criteria/{RevisionOfListOfA}", new { isIncluded = false }, Cancellation),
            ChangeSelection => await client.PutAsJsonAsync(
                $"{ConfigurationsPath}/{await SavedListOfAAsync(MiniCatalog.T1Version)}/selection", ChangeSelectionBody(), Cancellation),
            DeleteList => await client.DeleteAsync($"{ConfigurationsPath}/{_listOfA!.Id}", Cancellation),
            CopyList => await client.PostAsJsonAsync(
                $"{ConfigurationsPath}/{await SavedListOfAAsync(MiniCatalog.T2Version)}/copies", new { release = "active" }, Cancellation),
            ExportList => await client.PostAsJsonAsync(
                $"{ConfigurationsPath}/{await SavedListOfAAsync(MiniCatalog.T1Version)}/exports", new { format = "csv" }, Cancellation),
            CreateShareLink => await client.PostAsJsonAsync(
                $"{ConfigurationsPath}/{_listOfA!.Id}/share-links",
                new { expiresOn = DateOnly.FromDateTime(Clock.GetUtcNow().UtcDateTime).AddDays(30).ToString("yyyy-MM-dd") },
                Cancellation),
            ListShareLinks => await client.GetAsync($"{ConfigurationsPath}/{_listOfA!.Id}/share-links", Cancellation),
            RevokeShareLink => await client.DeleteAsync($"/api/v1/share-links/{_linkOfA!.Id}", Cancellation),
            ListProjects => await client.GetAsync(ProjectsPath(a), Cancellation),
            CreateProject => await client.PostAsJsonAsync(ProjectsPath(a), new { name = "Neues Projekt" }, Cancellation),
            ReadProject => await client.GetAsync(ProjectOfAPath, Cancellation),
            RenameProject => await client.PatchAsJsonAsync(ProjectOfAPath, new { name = "Umbenannt" }, Cancellation),
            ChangeProjectStatus => await client.PatchAsJsonAsync(ProjectOfAPath, new { status = "archived" }, Cancellation),
            AssignProjectList => await client.PutAsJsonAsync(
                $"{ProjectOfAPath}/criteria-configuration", new { sourceConfigurationId = _listOfA!.Id }, Cancellation),
            CreateCandidate => await client.PostAsJsonAsync($"{ProjectOfAPath}/candidates", new { name = "Neues Produkt" }, Cancellation),
            ReadCandidate => await client.GetAsync(CandidateOfAPath, Cancellation),
            ChangeCandidate => await client.PatchAsJsonAsync(CandidateOfAPath, new { name = "Umbenannt" }, Cancellation),
            SetTestStatus => await client.PutAsJsonAsync($"{CandidateOfAPath}/test-status", new { testStatus = "tested" }, Cancellation),
            AddEvidence => await client.PostAsJsonAsync($"{CandidateOfAPath}/test-evidences", EvidenceBody("own"), Cancellation),
            ChangeEvidence => await client.PutAsJsonAsync(EvidenceOfAPath, EvidenceBody("procedure", "softwareTest"), Cancellation),
            RemoveEvidence => await client.DeleteAsync(EvidenceOfAPath, Cancellation),
            ReadAssessments => await client.GetAsync($"{CandidateOfAPath}/assessments", Cancellation),
            SetAssessment => await client.PutAsJsonAsync(
                $"{CandidateOfAPath}/assessments/{RevisionOfListOfA}", new { value = "fulfilled" }, Cancellation),
            _ => await SendOrganizationRequestAsync(client, action, user),
        };
    }

    /// <summary>The request of an organisation endpoint.</summary>
    private Task<HttpResponseMessage> SendOrganizationRequestAsync(HttpClient client, string action, TestUser? user)
    {
        var a = Fixture.A.Id;
        return action switch
        {
            ListMine => client.GetAsync("/api/v1/organizations", Cancellation),
            Read => client.GetAsync(MembersPath(a), Cancellation),
            ChangeRole => client.PatchAsJsonAsync(MemberPath(a, TestUsers.ReaderA.Id), new { role = "editor" }, Cancellation),
            // Someone else than the caller: the Editor for everybody but the Editor, who names the Reader.
            RemoveOther => client.DeleteAsync(
                MemberPath(a, user == TestUsers.EditorA ? TestUsers.ReaderA.Id : TestUsers.EditorA.Id), Cancellation),
            Leave => client.DeleteAsync(MemberPath(a, user!.Id), Cancellation),
            Invite => client.PostAsJsonAsync(InvitationsPath(a), new { email = "neu@example.test", role = "reader" }, Cancellation),
            ListInvitations => client.GetAsync(InvitationsPath(a), Cancellation),
            RevokeInvitation => client.DeleteAsync($"{InvitationsPath(a)}/{_invitation!.Id}", Cancellation),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, "Unknown action."),
        };
    }
}
