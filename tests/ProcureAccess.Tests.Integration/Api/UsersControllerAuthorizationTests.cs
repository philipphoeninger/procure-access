// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text;
using DAL.ProcureAccess.EFStructures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MODELS.ProcureAccess.Entities;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Api;

/// <summary>
/// The legacy account deletion <c>DELETE /api/Users</c> needs a signed-in user (S4-03, #205): before,
/// an anonymous request reached the action and answered 400. Its failures are Problem Details
/// (S5-10, API-01). The token-less <c>POST /api/Users/reset-password</c> is gone (owner decision
/// O-S4-7 (a)); the password reset goes through <c>POST /api/v1/auth/password-reset</c> (S5-13).
/// <c>change-email</c>, <c>change-password</c> and <c>revoke-sessions</c> went with S5-10 (owner
/// decision S5-10 2a): no screen called them.
/// </summary>
public sealed class UsersControllerAuthorizationTests(ApiFixture api) : IntegrationTest(api)
{
    /// <summary>Every action of the controller with a body it would accept.</summary>
    public static TheoryData<string, string, string?> AccountActions => new()
    {
        { "DELETE", "/api/Users", null },
    };

    /// <summary>The legacy account actions S5-10 removed, with the body they accepted.</summary>
    public static TheoryData<string, string> RemovedActions => new()
    {
        { "/api/Users/change-email", """{"newEmail":"new@example.test"}""" },
        { "/api/Users/change-password", """{"currentPassword":"Old-Pass1","newPassword":"New-Pass1"}""" },
        { "/api/Users/revoke-sessions", "{}" },
    };

    [Theory]
    [MemberData(nameof(AccountActions))]
    public async Task AccountAction_Anonymous_Returns401ProblemDetails(string method, string path, string? body)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = CreateClient();

        using var request = Request(method, path, body);
        using var response = await client.SendAsync(request, cancellationToken);

        var problem = await ProblemResponse.ReadAsync(response, cancellationToken);
        problem.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        problem.MediaTypeOfResponse.ShouldBe(ProblemResponse.MediaType);
        problem.Json.ShouldNotBeNull().GetProperty("status").GetInt32().ShouldBe((int)HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// The 401 above could also come from the action's own check of the user id; this pins it to
    /// the authorisation step: every action requires a signed-in user and none allows anonymous calls.
    /// </summary>
    [Fact]
    public void UsersControllerEndpoints_Listed_RequireAuthorizationWithoutAnonymousException()
    {
        var endpoints = Api.Factory.Services.GetRequiredService<EndpointDataSource>().Endpoints
            .Where(e => e.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerName == "Users")
            .ToList();

        endpoints.Count.ShouldBe(AccountActions.Count());
        endpoints.ShouldAllBe(e => e.Metadata.GetOrderedMetadata<IAuthorizeData>().Any()
                                   && e.Metadata.GetMetadata<IAllowAnonymous>() == null);
    }

    [Fact]
    public async Task DeleteUser_SignedIn_Returns200AndSoftDeletesAtTheClockTime()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Api.ResetDatabaseAsync();
        const string stamp = "stamp-before-delete";
        await StoreAsync(new User
        {
            Id = TestUsers.Member.Id,
            UserName = TestUsers.Member.Email,
            NormalizedUserName = TestUsers.Member.Email.ToUpperInvariant(),
            Email = TestUsers.Member.Email,
            NormalizedEmail = TestUsers.Member.Email.ToUpperInvariant(),
            SecurityStamp = stamp,
        });
        Clock.Advance(TimeSpan.FromMinutes(7));
        using var client = CreateClient(TestUsers.Member);

        using var response = await client.DeleteAsync("/api/Users", cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var deleted = await StoredUserAsync(TestUsers.Member.Id, cancellationToken);
        deleted.IsDeleted.ShouldBeTrue();
        deleted.DeletedAt.ShouldBe(Clock.GetUtcNow().UtcDateTime);
        deleted.Email.ShouldNotBe(TestUsers.Member.Email);
        deleted.SecurityStamp.ShouldNotBe(stamp);
    }

    /// <summary>The signed-in principal names a user the database does not hold (any more).</summary>
    [Fact]
    public async Task DeleteUser_UserMissing_Returns404NotFoundProblem()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Api.ResetDatabaseAsync();
        using var client = CreateClient(TestUsers.Member);

        using var response = await client.DeleteAsync("/api/Users", cancellationToken);

        var problem = await ProblemResponse.ReadAsync(response, cancellationToken);
        problem.Deviations(HttpStatusCode.NotFound, TypeUri(ProblemTypes.NotFound), requireErrors: false).ShouldBeEmpty();
        problem.MediaTypeOfResponse.ShouldBe(ProblemResponse.MediaType);
        problem.Json.ShouldNotBeNull().TryGetProperty("code", out _).ShouldBeFalse();
    }

    [Theory]
    [MemberData(nameof(RemovedActions))]
    public async Task RemovedAccountAction_SignedIn_Returns404(string path, string body)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = CreateClient(TestUsers.Member);

        using var request = Request("POST", path, body);
        using var response = await client.SendAsync(request, cancellationToken);

        var problem = await ProblemResponse.ReadAsync(response, cancellationToken);
        problem.Deviations(HttpStatusCode.NotFound, TypeUri(ProblemTypes.NotFound), requireErrors: false).ShouldBeEmpty();
    }

    [Fact]
    public async Task ResetPasswordWithoutToken_Anonymous_Returns404()
    {
        await AssertResetPasswordGoneAsync(user: null);
    }

    [Fact]
    public async Task ResetPasswordWithoutToken_SignedIn_Returns404()
    {
        await AssertResetPasswordGoneAsync(TestUsers.Member);
    }

    private async Task AssertResetPasswordGoneAsync(TestUser? user)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var client = CreateClient(user);

        using var request = Request(
            "POST", "/api/Users/reset-password", """{"email":"member@example.test","newPassword":"New-Pass1"}""");
        using var response = await client.SendAsync(request, cancellationToken);

        var problem = await ProblemResponse.ReadAsync(response, cancellationToken);
        problem.Deviations(HttpStatusCode.NotFound, TypeUri(ProblemTypes.NotFound), requireErrors: false).ShouldBeEmpty();
    }

    private string TypeUri(string name) =>
        $"{Api.Factory.Services.GetRequiredService<IConfiguration>()["ProblemTypes:BaseUri"]}/problems/{name}";

    /// <summary>The stored user, also when soft-deleted (test code, outside the TEN-02 allow-list).</summary>
    private async Task<User> StoredUserAsync(string userId, CancellationToken cancellationToken)
    {
        using var scope = Api.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        return await db.Users.AsNoTracking()
            .IgnoreQueryFilters()
            .SingleAsync(u => u.Id == userId, cancellationToken);
    }

    private static HttpRequestMessage Request(string method, string path, string? body) =>
        new(new HttpMethod(method), path)
        {
            Content = body is null ? null : new StringContent(body, Encoding.UTF8, "application/json"),
        };
}
