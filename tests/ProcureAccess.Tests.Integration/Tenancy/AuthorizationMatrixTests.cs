// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Tenancy;

/// <summary>
/// TEN-05, the skeleton (ARCHITECTURE §9.1, SPEC R-ORG-05; S5-15): the matrix Owner / Editor /
/// Reader of organisation A, Owner of B, user without organisation, anonymous × a read, an edit and
/// an owner endpoint on organisation A, with the expected status codes. The organisation comes from
/// the route (the resource); a client-supplied organisation header changes nothing. Until S5-16
/// adds the real organisation endpoints, the endpoints are the test-only
/// <see cref="OrganizationProbeController"/>, which uses the requirement and the filter exactly as
/// a module endpoint will; S5-16 moves the matrix onto the real endpoints.
/// </summary>
public sealed class AuthorizationMatrixTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Anonymous = "anonymous";

    private ProcureAccessApiFactory? _host;
    private Tenants? _tenants;

    private ProcureAccessApiFactory Host => _host ?? throw new InvalidOperationException("The host is not started.");

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    public static TheoryData<string, string, HttpStatusCode> Matrix => new()
    {
        // Read (Reader or above)
        { TestUsers.OwnerA.Name, "GET", HttpStatusCode.OK },
        { TestUsers.EditorA.Name, "GET", HttpStatusCode.OK },
        { TestUsers.ReaderA.Name, "GET", HttpStatusCode.OK },
        { TestUsers.OwnerB.Name, "GET", HttpStatusCode.NotFound },
        { TestUsers.Member.Name, "GET", HttpStatusCode.NotFound },
        { Anonymous, "GET", HttpStatusCode.Unauthorized },
        // Edit (Editor or above)
        { TestUsers.OwnerA.Name, "POST", HttpStatusCode.NoContent },
        { TestUsers.EditorA.Name, "POST", HttpStatusCode.NoContent },
        { TestUsers.ReaderA.Name, "POST", HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, "POST", HttpStatusCode.NotFound },
        { TestUsers.Member.Name, "POST", HttpStatusCode.NotFound },
        { Anonymous, "POST", HttpStatusCode.Unauthorized },
        // Owner only
        { TestUsers.OwnerA.Name, "DELETE", HttpStatusCode.NoContent },
        { TestUsers.EditorA.Name, "DELETE", HttpStatusCode.Forbidden },
        { TestUsers.ReaderA.Name, "DELETE", HttpStatusCode.Forbidden },
        { TestUsers.OwnerB.Name, "DELETE", HttpStatusCode.NotFound },
        { TestUsers.Member.Name, "DELETE", HttpStatusCode.NotFound },
        { Anonymous, "DELETE", HttpStatusCode.Unauthorized },
    };

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        _tenants = Tenants.Build(Clock.GetUtcNow());
        await StoreAsync(_tenants.Entities);
        _host = new ProcureAccessApiFactory(
            Api.ConnectionString,
            Clock,
            configureServices: services => services.AddControllers().AddApplicationPart(typeof(OrganizationProbeController).Assembly));
    }

    public async ValueTask DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.DisposeAsync();
        }
    }

    [Theory(DisplayName = "INV:TEN-05 " + nameof(Endpoint_UserAndMethod_AnswersTheMatrixStatus))]
    [Trait("Invariant", "TEN-05")]
    [MemberData(nameof(Matrix))]
    public async Task Endpoint_UserAndMethod_AnswersTheMatrixStatus(string userName, string method, HttpStatusCode expected)
    {
        using var client = CreateHostClient(userName == Anonymous ? null : TestUsers.ByName[userName]);

        using var response = await client.SendAsync(
            new HttpRequestMessage(new HttpMethod(method), $"{OrganizationProbeController.Path}/{Fixture.A.Id}"),
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(expected);
    }

    [Fact(DisplayName = "INV:TEN-05 " + nameof(Read_MemberOfA_SeesTheOrganizationName))]
    [Trait("Invariant", "TEN-05")]
    public async Task Read_MemberOfA_SeesTheOrganizationName()
    {
        using var client = CreateHostClient(TestUsers.ReaderA);

        var answer = await client.GetFromJsonAsync<OrganizationProbeController.OrganizationAnswer>(
            $"{OrganizationProbeController.Path}/{Fixture.A.Id}", TestContext.Current.CancellationToken);

        answer.ShouldNotBeNull().Name.ShouldBe("Organisation A");
    }

    [Theory(DisplayName = "INV:TEN-05 " + nameof(Endpoint_ClientOrganizationHeader_HasNoEffect))]
    [Trait("Invariant", "TEN-05")]
    [InlineData("owner-b", "A", HttpStatusCode.NotFound)]   // names A in the header, is no member of A
    [InlineData("reader-a", "B", HttpStatusCode.OK)]        // names B in the header, reads A through the route
    [InlineData("owner-a", "B", HttpStatusCode.OK)]
    public async Task Endpoint_ClientOrganizationHeader_HasNoEffect(string userName, string headerOrganization, HttpStatusCode expected)
    {
        using var client = CreateHostClient(TestUsers.ByName[userName]);
        var named = headerOrganization == "A" ? Fixture.A.Id : Fixture.B.Id;
        client.DefaultRequestHeaders.Add("X-Organization-Id", named.ToString());

        using var response = await client.GetAsync($"{OrganizationProbeController.Path}/{Fixture.A.Id}", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(expected);
    }

    [Theory(DisplayName = "INV:TEN-01 " + nameof(Memberships_Request_ListsOnlyTheUsersOrganizations))]
    [Trait("Invariant", "TEN-01")]
    [InlineData("owner-a", "A")]
    [InlineData("reader-b", "B")]
    public async Task Memberships_Request_ListsOnlyTheUsersOrganizations(string userName, string organization)
    {
        using var client = CreateHostClient(TestUsers.ByName[userName]);
        var own = organization == "A" ? Fixture.A.Id : Fixture.B.Id;
        var other = organization == "A" ? Fixture.B.Id : Fixture.A.Id;
        // Naming the other organisation changes nothing: the filter follows the memberships.
        client.DefaultRequestHeaders.Add("X-Organization-Id", other.ToString());

        var memberships = await client.GetFromJsonAsync<List<OrganizationProbeController.MembershipAnswer>>(
            OrganizationProbeController.MembershipsPath, TestContext.Current.CancellationToken);

        memberships.ShouldNotBeNull().Count.ShouldBe(3);
        memberships.ShouldAllBe(m => m.OrganizationId == own);
    }

    [Fact(DisplayName = "INV:TEN-01 " + nameof(Memberships_UserWithoutOrganization_SeesNone))]
    [Trait("Invariant", "TEN-01")]
    public async Task Memberships_UserWithoutOrganization_SeesNone()
    {
        using var client = CreateHostClient(TestUsers.Member);

        var memberships = await client.GetFromJsonAsync<List<OrganizationProbeController.MembershipAnswer>>(
            OrganizationProbeController.MembershipsPath, TestContext.Current.CancellationToken);

        memberships.ShouldNotBeNull().ShouldBeEmpty();
    }

    /// <summary>A client of the probe host with its own cookie jar and the antiforgery hook (as <see cref="IntegrationTest.CreateClient"/>).</summary>
    private HttpClient CreateHostClient(TestUser? user)
    {
        var cookies = new CookieContainer();
        var client = Host.CreateDefaultClient(BaseAddress, new AntiforgeryHandler(cookies), new CookieContainerHandler(cookies));
        if (user is not null)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, user.Name);
        }

        return client;
    }
}

/// <summary>
/// Test-only endpoints on organisation A's route, built as a module endpoint of S5-16 will be: the
/// organisation is the route's (<see cref="OrganizationReference"/>), the check is the resource-based
/// <see cref="OrganizationRoleRequirement"/>, a refusal answers <see cref="OrganizationAccess.StatusOf"/>,
/// and reads go through the filtered context.
/// </summary>
[ApiController]
[Route(Path)]
[Authorize]
public sealed class OrganizationProbeController(IAuthorizationService authorization) : ControllerBase
{
    public const string Path = "/test/organizations";
    public const string MembershipsPath = "/test/organizations/memberships";

    public sealed record OrganizationAnswer(Guid Id, string Name);

    public sealed record MembershipAnswer(Guid OrganizationId, string UserId, string Role);

    [HttpGet("{orgId:guid}")]
    public async Task<IActionResult> Read(Guid orgId)
    {
        if (await RefusedAsync(orgId, OrganizationRoleRequirement.Reader) is { } status)
        {
            return StatusCode(status);
        }

        var db = HttpContext.RequestServices.GetRequiredService<IdentityAccessDbContext>();
        var organization = await db.Organizations.AsNoTracking().SingleOrDefaultAsync(o => o.Id == orgId, HttpContext.RequestAborted);
        return organization is null ? NotFound() : Ok(new OrganizationAnswer(organization.Id, organization.Name));
    }

    [HttpPost("{orgId:guid}")]
    public async Task<IActionResult> Edit(Guid orgId) =>
        await RefusedAsync(orgId, OrganizationRoleRequirement.Editor) is { } status ? StatusCode(status) : NoContent();

    [HttpDelete("{orgId:guid}")]
    public async Task<IActionResult> Own(Guid orgId) =>
        await RefusedAsync(orgId, OrganizationRoleRequirement.Owner) is { } status ? StatusCode(status) : NoContent();

    /// <summary>Every membership the request may see: the filter alone decides, no role check.</summary>
    [HttpGet("memberships")]
    public async Task<IActionResult> Memberships()
    {
        var db = HttpContext.RequestServices.GetRequiredService<IdentityAccessDbContext>();
        var memberships = await db.OrganizationMemberships.AsNoTracking()
                                  .OrderBy(m => m.UserId)
                                  .Select(m => new MembershipAnswer(m.OrganizationId, m.UserId, m.Role.ToString()))
                                  .ToListAsync(HttpContext.RequestAborted);
        return Ok(memberships);
    }

    private async Task<int?> RefusedAsync(Guid orgId, OrganizationRoleRequirement requirement) =>
        OrganizationAccess.StatusOf(await authorization.AuthorizeAsync(User, new OrganizationReference(orgId), requirement));
}
