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
/// The legacy account actions of <c>UsersController</c> need a signed-in user (S4-03, #205): before,
/// an anonymous request reached the action and answered 400. The token-less
/// <c>POST /api/Users/reset-password</c> is gone (owner decision O-S4-7 (a)); the password reset
/// goes through the token-based <c>POST /api/reset-password</c>.
/// </summary>
public sealed class UsersControllerAuthorizationTests(ApiFixture api) : IntegrationTest(api)
{
    /// <summary>Every action of the controller with a body it would accept.</summary>
    public static TheoryData<string, string, string?> AccountActions => new()
    {
        { "POST", "/api/Users/change-email", """{"newEmail":"new@example.test"}""" },
        { "POST", "/api/Users/change-password", """{"currentPassword":"Old-Pass1","newPassword":"New-Pass1"}""" },
        { "POST", "/api/Users/revoke-sessions", null },
        { "DELETE", "/api/Users", null },
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
    public async Task RevokeSessions_SignedIn_Returns200AndRenewsSecurityStamp()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Api.ResetDatabaseAsync();
        const string stamp = "stamp-before-revoke";
        await StoreAsync(new User
        {
            Id = TestUsers.Member.Id,
            UserName = TestUsers.Member.Email,
            NormalizedUserName = TestUsers.Member.Email.ToUpperInvariant(),
            Email = TestUsers.Member.Email,
            NormalizedEmail = TestUsers.Member.Email.ToUpperInvariant(),
            SecurityStamp = stamp,
        });
        using var client = CreateClient(TestUsers.Member);

        using var response = await client.PostAsync("/api/Users/revoke-sessions", content: null, cancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await SecurityStampAsync(TestUsers.Member.Id, cancellationToken)).ShouldNotBe(stamp);
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

    private async Task<string?> SecurityStampAsync(string userId, CancellationToken cancellationToken)
    {
        using var scope = Api.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDBContext>();
        return await db.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.SecurityStamp)
            .SingleAsync(cancellationToken);
    }

    private static HttpRequestMessage Request(string method, string path, string? body) =>
        new(new HttpMethod(method), path)
        {
            Content = body is null ? null : new StringContent(body, Encoding.UTF8, "application/json"),
        };
}
