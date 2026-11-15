// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using API.ProcureAccess.Organizations;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
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
/// since S5-17a with the invitation endpoints; the workspace and configurator endpoints add their
/// rows with their steps.
/// </summary>
public sealed class AuthorizationMatrixTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Anonymous = "anonymous";

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

    private OrganizationInvitation? _invitation;

    private Tenants? _tenants;

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    public static TheoryData<string, string, HttpStatusCode> Matrix => new()
    {
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
    };

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        _tenants = Tenants.Build(Clock.GetUtcNow());
        _invitation = OrganizationInvitation.Create(
            _tenants.A.Id, "eingeladen@example.test", OrganizationRole.Reader, new byte[32], Clock.GetUtcNow(), TimeSpan.FromDays(14));
        await StoreAsync([.. _tenants.Entities, _invitation]);
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

    /// <summary>The request of <paramref name="action"/> on organisation A for <paramref name="user"/>.</summary>
    private Task<HttpResponseMessage> SendAsync(HttpClient client, string action, TestUser? user)
    {
        var a = Fixture.A.Id;
        return action switch
        {
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
