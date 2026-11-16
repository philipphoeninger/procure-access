// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.ProcureAccess.Organizations;
using Microsoft.EntityFrameworkCore;
using MODELS.ProcureAccess.Entities;
using Npgsql;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Organizations;

/// <summary>
/// The organisation endpoints (S5-16a; SPEC R-ORG-01, R-ORG-03, R-ORG-04, §8.9; ARCHITECTURE §7.1,
/// C80): creating an organisation, the members with their addresses, role changes, removal and
/// leaving, and the last-Owner rule (TEN-11). Every test starts from the Tenants fixture with the
/// legacy accounts of its users stored, so the addresses and the confirmation gate are real. The
/// authorisation matrix of these endpoints is <c>AuthorizationMatrixTests</c> (TEN-05).
/// </summary>
public sealed class OrganizationEndpointsTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Organizations = "/api/v1/organizations";

    private Tenants? _tenants;

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        _tenants = Tenants.Build(Clock.GetUtcNow());
        await StoreAsync(_tenants.Entities);
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => AccountOf(u, confirmed: u != TestUsers.ReaderB))]);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Create_ConfirmedUser_Returns201AndTheCreatorIsTheOnlyOwner()
    {
        using var client = CreateClient(TestUsers.Member);

        using var response = await client.PostAsJsonAsync(
            Organizations, new { name = "  Stadt Musterhausen  ", type = "publicBody" }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = (await response.Content.ReadFromJsonAsync<CreatedOrganization>(Cancellation)).ShouldNotBeNull();
        response.Headers.Location.ShouldNotBeNull().ToString().ShouldBe($"{Organizations}/{created.Id}/members");

        // The creator is a member from the next request on (R-ORG-01).
        var members = await client.GetFromJsonAsync<OrganizationMembersResponse>(MembersPath(created.Id), Cancellation);
        members.ShouldNotBeNull().Organization.ShouldBe(new OrganizationResponse(created.Id, "Stadt Musterhausen", "publicBody"));
        members.Members.ShouldHaveSingleItem().ShouldBe(
            new OrganizationMemberResponse(TestUsers.Member.Id, TestUsers.Member.Email, "owner"));

        await using var db = IdentityAccess();
        var stored = await db.Organizations.IgnoreQueryFilters().SingleAsync(o => o.Id == created.Id, Cancellation);
        stored.CreatedAt.ShouldBe(Clock.GetUtcNow());
        stored.CreatedByUserId.ShouldBe(TestUsers.Member.Id);
        var founder = await db.OrganizationMemberships.IgnoreQueryFilters().SingleAsync(m => m.OrganizationId == created.Id, Cancellation);
        founder.AcceptedAt.ShouldBe(Clock.GetUtcNow());
    }

    [Theory]
    [InlineData("company", "Company")]
    [InlineData("association", "Association")]
    public async Task Create_EachType_IsStored(string type, string expected)
    {
        using var client = CreateClient(TestUsers.Member);

        using var response = await client.PostAsJsonAsync(Organizations, new { name = "Verein", type }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = (await response.Content.ReadFromJsonAsync<CreatedOrganization>(Cancellation)).ShouldNotBeNull();
        await using var db = IdentityAccess();
        (await db.Organizations.IgnoreQueryFilters().SingleAsync(o => o.Id == created.Id, Cancellation)).Type.ToString().ShouldBe(expected);
    }

    [Fact]
    public async Task Create_NameOf120Characters_IsAccepted()
    {
        using var client = CreateClient(TestUsers.Member);

        using var response = await client.PostAsJsonAsync(Organizations, new { name = new string('a', 120), type = "company" }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Theory]
    [InlineData(null, "company", "name")]
    [InlineData("   ", "company", "name")]
    [InlineData("121", "company", "name")]
    [InlineData("Verein", null, "type")]
    [InlineData("Verein", "Company", "type")]
    [InlineData("Verein", "unknown", "type")]
    [InlineData(null, null, "name,type")]
    public async Task Create_InvalidBody_Returns400AtTheFieldAndStoresNothing(string? name, string? type, string expectedKeys)
    {
        using var client = CreateClient(TestUsers.Member);
        var sentName = name == "121" ? new string('a', 121) : name;

        using var response = await client.PostAsJsonAsync(Organizations, new { name = sentName, type }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        var problem = await ProblemOf(response);
        problem.Type.ShouldEndWith("/problems/" + ProblemTypes.Validation);
        problem.ErrorKeys.ShouldBe(expectedKeys.Split(','));
        (await CountOrganizationsAsync()).ShouldBe(2);
    }

    [Fact(DisplayName = "INV:SEC-10 " + nameof(Create_UnconfirmedUser_Returns403EmailUnconfirmedAndStoresNothing))]
    [Trait("Invariant", "SEC-10")]
    public async Task Create_UnconfirmedUser_Returns403EmailUnconfirmedAndStoresNothing()
    {
        using var client = CreateClient(TestUsers.ReaderB);

        using var response = await client.PostAsJsonAsync(Organizations, new { name = "Verein", type = "association" }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await ProblemOf(response)).Type.ShouldEndWith("/problems/" + ProblemTypes.EmailUnconfirmed);
        (await CountOrganizationsAsync()).ShouldBe(2);
    }

    [Fact]
    public async Task Create_Anonymous_Returns401()
    {
        using var client = CreateClient();

        using var response = await client.PostAsJsonAsync(Organizations, new { name = "Verein", type = "association" }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        (await CountOrganizationsAsync()).ShouldBe(2);
    }

    [Fact]
    public async Task Members_Reader_SeesOwnersFirstWithTheirAddresses()
    {
        using var client = CreateClient(TestUsers.ReaderA);

        var answer = await client.GetFromJsonAsync<OrganizationMembersResponse>(MembersPath(Fixture.A.Id), Cancellation);

        answer.ShouldNotBeNull().Organization.ShouldBe(new OrganizationResponse(Fixture.A.Id, "Organisation A", "publicBody"));
        answer.Members.ShouldBe(
        [
            new OrganizationMemberResponse(TestUsers.OwnerA.Id, TestUsers.OwnerA.Email, "owner"),
            new OrganizationMemberResponse(TestUsers.EditorA.Id, TestUsers.EditorA.Email, "editor"),
            new OrganizationMemberResponse(TestUsers.ReaderA.Id, TestUsers.ReaderA.Email, "reader"),
        ]);
    }

    [Fact]
    public async Task ChangeRole_OwnerGivesTheReaderEditor_TheRoleChanges()
    {
        using var owner = CreateClient(TestUsers.OwnerA);

        using var response = await PatchRoleAsync(owner, TestUsers.ReaderA, "editor");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await RoleOfAsync(Fixture.A.Id, TestUsers.ReaderA)).ShouldBe(OrganizationRole.Editor);
    }

    [Fact]
    public async Task ChangeRole_SameRole_Returns204()
    {
        using var owner = CreateClient(TestUsers.OwnerA);

        using var response = await PatchRoleAsync(owner, TestUsers.EditorA, "editor");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await RoleOfAsync(Fixture.A.Id, TestUsers.EditorA)).ShouldBe(OrganizationRole.Editor);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Editor")]
    [InlineData("admin")]
    public async Task ChangeRole_InvalidRole_Returns400AtRole(string? role)
    {
        using var owner = CreateClient(TestUsers.OwnerA);

        using var response = await owner.PatchAsJsonAsync(MemberPath(Fixture.A.Id, TestUsers.ReaderA.Id), new { role }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await ProblemOf(response)).ErrorKeys.ShouldBe(["role"]);
        (await RoleOfAsync(Fixture.A.Id, TestUsers.ReaderA)).ShouldBe(OrganizationRole.Reader);
    }

    [Theory]
    [InlineData("PATCH")]
    [InlineData("DELETE")]
    public async Task Change_UnknownMember_Returns404(string method)
    {
        using var owner = CreateClient(TestUsers.OwnerA);
        using var request = new HttpRequestMessage(new HttpMethod(method), MemberPath(Fixture.A.Id, "no-such-user"))
        {
            Content = method == "PATCH" ? JsonContent.Create(new { role = "editor" }) : null,
        };

        using var response = await owner.SendAsync(request, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "INV:TEN-11 " + nameof(ChangeRole_LastOwnerDemotesThemself_Returns409LastOwner))]
    [Trait("Invariant", "TEN-11")]
    public async Task ChangeRole_LastOwnerDemotesThemself_Returns409LastOwner()
    {
        using var owner = CreateClient(TestUsers.OwnerA);

        using var response = await PatchRoleAsync(owner, TestUsers.OwnerA, "editor");

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await ProblemOf(response)).Type.ShouldEndWith("/problems/" + ProblemTypes.LastOwner);
        (await RoleOfAsync(Fixture.A.Id, TestUsers.OwnerA)).ShouldBe(OrganizationRole.Owner);
    }

    [Fact(DisplayName = "INV:TEN-11 " + nameof(Leave_LastOwner_Returns409LastOwnerAndStaysMember))]
    [Trait("Invariant", "TEN-11")]
    public async Task Leave_LastOwner_Returns409LastOwnerAndStaysMember()
    {
        using var owner = CreateClient(TestUsers.OwnerA);

        using var response = await owner.DeleteAsync(MemberPath(Fixture.A.Id, TestUsers.OwnerA.Id), Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await ProblemOf(response)).Type.ShouldEndWith("/problems/" + ProblemTypes.LastOwner);
        (await RoleOfAsync(Fixture.A.Id, TestUsers.OwnerA)).ShouldBe(OrganizationRole.Owner);
    }

    [Fact(DisplayName = "INV:TEN-11 " + nameof(LastOwnerRule_TwoOwners_OneMayGoButNotTheLast))]
    [Trait("Invariant", "TEN-11")]
    public async Task LastOwnerRule_TwoOwners_OneMayGoButNotTheLast()
    {
        using var owner = CreateClient(TestUsers.OwnerA);
        using var editor = CreateClient(TestUsers.EditorA);
        (await PatchRoleAsync(owner, TestUsers.EditorA, "owner")).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // Two Owners: one may demote themself, then the other is the last one.
        using var demoted = await PatchRoleAsync(owner, TestUsers.OwnerA, "reader");
        using var lastDemotes = await PatchRoleAsync(editor, TestUsers.EditorA, "editor");
        using var lastLeaves = await editor.DeleteAsync(MemberPath(Fixture.A.Id, TestUsers.EditorA.Id), Cancellation);

        demoted.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        lastDemotes.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        lastLeaves.StatusCode.ShouldBe(HttpStatusCode.Conflict);
        (await RoleOfAsync(Fixture.A.Id, TestUsers.EditorA)).ShouldBe(OrganizationRole.Owner);
    }

    [Fact(DisplayName = "INV:TEN-11 " + nameof(Remove_OtherOwnerWhileTwoOwners_Succeeds))]
    [Trait("Invariant", "TEN-11")]
    public async Task Remove_OtherOwnerWhileTwoOwners_Succeeds()
    {
        using var owner = CreateClient(TestUsers.OwnerA);
        (await PatchRoleAsync(owner, TestUsers.EditorA, "owner")).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using var removed = await owner.DeleteAsync(MemberPath(Fixture.A.Id, TestUsers.EditorA.Id), Cancellation);
        using var lastLeaves = await owner.DeleteAsync(MemberPath(Fixture.A.Id, TestUsers.OwnerA.Id), Cancellation);

        removed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await RoleOfAsync(Fixture.A.Id, TestUsers.EditorA)).ShouldBeNull();
        lastLeaves.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact(DisplayName = "INV:TEN-11 " + nameof(LastOwnerRule_TwoOwnersDemoteEachOtherAtOnce_OneOwnerRemains))]
    [Trait("Invariant", "TEN-11")]
    public async Task LastOwnerRule_TwoOwnersDemoteEachOtherAtOnce_OneOwnerRemains()
    {
        using var first = CreateClient(TestUsers.OwnerA);
        using var second = CreateClient(TestUsers.EditorA);
        (await PatchRoleAsync(first, TestUsers.EditorA, "owner")).StatusCode.ShouldBe(HttpStatusCode.NoContent);

        // The test holds the organisation's lock, so both requests pass their role check as Owners
        // and then wait for it; released, they run one after the other. Without the lock both
        // would see two Owners and both demotions would go through.
        await using var holder = new NpgsqlConnection(Api.ConnectionString);
        await holder.OpenAsync(Cancellation);
        await using var transaction = await holder.BeginTransactionAsync(Cancellation);
        await using (var command = new NpgsqlCommand("""SELECT 1 FROM identity."Organizations" WHERE "Id" = @id FOR UPDATE""", holder, transaction))
        {
            command.Parameters.AddWithValue("id", Fixture.A.Id);
            await command.ExecuteNonQueryAsync(Cancellation);
        }

        var demoteSecond = PatchRoleAsync(first, TestUsers.EditorA, "editor");
        var demoteFirst = PatchRoleAsync(second, TestUsers.OwnerA, "editor");
        await WaitForLockWaitersAsync(expected: 2);
        await transaction.RollbackAsync(Cancellation);
        var demotions = await Task.WhenAll(demoteSecond, demoteFirst);

        demotions.Select(r => r.StatusCode).Order().ShouldBe([HttpStatusCode.NoContent, HttpStatusCode.Conflict]);
        (await OwnersOfAsync(Fixture.A.Id)).ShouldHaveSingleItem();
        foreach (var response in demotions)
        {
            response.Dispose();
        }
    }

    [Fact]
    public async Task Remove_RemovedMember_LosesAccessWithTheNextRequest()
    {
        using var owner = CreateClient(TestUsers.OwnerA);
        using var reader = CreateClient(TestUsers.ReaderA);
        (await reader.GetAsync(MembersPath(Fixture.A.Id), Cancellation)).StatusCode.ShouldBe(HttpStatusCode.OK);

        using var removed = await owner.DeleteAsync(MemberPath(Fixture.A.Id, TestUsers.ReaderA.Id), Cancellation);
        using var after = await reader.GetAsync(MembersPath(Fixture.A.Id), Cancellation);

        removed.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        after.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("editor-a")]
    [InlineData("reader-a")]
    public async Task Leave_Member_Returns204AndTheMembershipIsGone(string userName)
    {
        var user = TestUsers.ByName[userName];
        using var client = CreateClient(user);

        using var response = await client.DeleteAsync(MemberPath(Fixture.A.Id, user.Id), Cancellation);
        using var after = await client.GetAsync(MembersPath(Fixture.A.Id), Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await RoleOfAsync(Fixture.A.Id, user)).ShouldBeNull();
        after.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "INV:TEN-01 " + nameof(List_MemberOfTwoOrganizations_ListsBothByNameWithItsRoles))]
    [Trait("Invariant", "TEN-01")]
    public async Task List_MemberOfTwoOrganizations_ListsBothByNameWithItsRoles()
    {
        // Reader of A also becomes Editor of B and of a third organisation whose name sorts first
        // only under German collation (Ä before B); the header's switch lists all three (R-SHL-03).
        var now = Clock.GetUtcNow();
        var third = Organization.Create("Ämter Musterhausen", OrganizationType.PublicBody, TestUsers.Member.Id, now);
        await StoreAsync(
        [
            third,
            OrganizationMembership.Founder(third, now),
            OrganizationMembership.Create(third.Id, TestUsers.ReaderA.Id, OrganizationRole.Editor, now, now),
            OrganizationMembership.Create(Fixture.B.Id, TestUsers.ReaderA.Id, OrganizationRole.Editor, now, now),
        ]);
        using var client = CreateClient(TestUsers.ReaderA);

        var answer = await client.GetFromJsonAsync<List<OrganizationSummaryResponse>>(Organizations, Cancellation);

        answer.ShouldBe(
        [
            new OrganizationSummaryResponse(third.Id, "Ämter Musterhausen", "publicBody", "editor"),
            new OrganizationSummaryResponse(Fixture.A.Id, "Organisation A", "publicBody", "reader"),
            new OrganizationSummaryResponse(Fixture.B.Id, "Organisation B", "company", "editor"),
        ]);
    }

    [Theory(DisplayName = "INV:TEN-01 " + nameof(List_Request_ListsOnlyTheUsersOwnOrganizations))]
    [Trait("Invariant", "TEN-01")]
    [InlineData("owner-a", "A", "owner")]
    [InlineData("reader-b", "B", "reader")]
    public async Task List_Request_ListsOnlyTheUsersOwnOrganizations(string userName, string organization, string role)
    {
        using var client = CreateClient(TestUsers.ByName[userName]);
        var own = organization == "A" ? Fixture.A : Fixture.B;
        var other = organization == "A" ? Fixture.B : Fixture.A;
        // Naming the other organisation changes nothing: the list follows the memberships.
        client.DefaultRequestHeaders.Add("X-Organization-Id", other.Id.ToString());

        var answer = await client.GetFromJsonAsync<List<OrganizationSummaryResponse>>(Organizations, Cancellation);

        answer.ShouldNotBeNull().ShouldHaveSingleItem().ShouldBe(
            new OrganizationSummaryResponse(own.Id, own.Name, organization == "A" ? "publicBody" : "company", role));
    }

    [Fact(DisplayName = "INV:TEN-01 " + nameof(List_UserWithoutOrganization_ReturnsAnEmptyList))]
    [Trait("Invariant", "TEN-01")]
    public async Task List_UserWithoutOrganization_ReturnsAnEmptyList()
    {
        using var client = CreateClient(TestUsers.Member);

        var answer = await client.GetFromJsonAsync<List<OrganizationSummaryResponse>>(Organizations, Cancellation);

        answer.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task List_AfterLeaving_NoLongerListsTheOrganization()
    {
        using var client = CreateClient(TestUsers.EditorA);
        using var left = await client.DeleteAsync(MemberPath(Fixture.A.Id, TestUsers.EditorA.Id), Cancellation);
        left.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var answer = await client.GetFromJsonAsync<List<OrganizationSummaryResponse>>(Organizations, Cancellation);

        answer.ShouldNotBeNull().ShouldBeEmpty();
    }

    private static string MembersPath(Guid organizationId) => $"{Organizations}/{organizationId}/members";

    private static string MemberPath(Guid organizationId, string userId) => $"{MembersPath(organizationId)}/{userId}";

    private Task<HttpResponseMessage> PatchRoleAsync(HttpClient client, TestUser member, string role) =>
        client.PatchAsJsonAsync(MemberPath(Fixture.A.Id, member.Id), new { role }, Cancellation);

    /// <summary>The legacy account of a fixture user, so the addresses and the confirmation gate are real.</summary>
    private static User AccountOf(TestUser user, bool confirmed) => new()
    {
        Id = user.Id,
        UserName = user.Email,
        NormalizedUserName = user.Email.ToUpperInvariant(),
        Email = user.Email,
        NormalizedEmail = user.Email.ToUpperInvariant(),
        EmailConfirmed = confirmed,
        SecurityStamp = Guid.NewGuid().ToString(),
    };

    private async Task<OrganizationRole?> RoleOfAsync(Guid organizationId, TestUser user)
    {
        await using var db = IdentityAccess();
        return await db.OrganizationMemberships.IgnoreQueryFilters()
                       .Where(m => m.OrganizationId == organizationId && m.UserId == user.Id)
                       .Select(m => (OrganizationRole?)m.Role)
                       .SingleOrDefaultAsync(Cancellation);
    }

    private async Task<List<string>> OwnersOfAsync(Guid organizationId)
    {
        await using var db = IdentityAccess();
        return await db.OrganizationMemberships.IgnoreQueryFilters()
                       .Where(m => m.OrganizationId == organizationId && m.Role == OrganizationRole.Owner)
                       .Select(m => m.UserId)
                       .ToListAsync(Cancellation);
    }

    /// <summary>
    /// Waits until <paramref name="expected"/> sessions of the database wait for a lock. On a
    /// connection of its own: inside a transaction PostgreSQL keeps the first view of
    /// <c>pg_stat_activity</c>, so the lock holder would never see the waiters arrive.
    /// </summary>
    private async Task WaitForLockWaitersAsync(int expected)
    {
        await using var connection = new NpgsqlConnection(Api.ConnectionString);
        await connection.OpenAsync(Cancellation);
        await using var command = new NpgsqlCommand(
            "SELECT count(*) FROM pg_stat_activity WHERE datname = current_database() AND wait_event_type = 'Lock'", connection);
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while ((long)(await command.ExecuteScalarAsync(Cancellation))! < expected)
        {
            DateTime.UtcNow.ShouldBeLessThan(deadline, "the role changes did not wait for the organisation's lock");
            await Task.Delay(50, Cancellation);
        }
    }

    private async Task<int> CountOrganizationsAsync()
    {
        await using var db = IdentityAccess();
        return await db.Organizations.IgnoreQueryFilters().CountAsync(Cancellation);
    }

    /// <summary>A context of its own outside any request, so tests read past the filter.</summary>
    private IdentityAccessDbContext IdentityAccess() =>
        new(new DbContextOptionsBuilder<IdentityAccessDbContext>().UseNpgsql(Api.ConnectionString).Options, FixedOrganizationContext.None);

    private static async Task<(string Type, string[] ErrorKeys)> ProblemOf(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        var keys = body.RootElement.TryGetProperty("errors", out var errors)
            ? errors.EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal).ToArray()
            : [];
        return (body.RootElement.GetProperty("type").GetString()!, keys);
    }
}
