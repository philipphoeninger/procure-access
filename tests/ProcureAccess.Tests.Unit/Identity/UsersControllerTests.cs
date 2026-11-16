// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using API.ProcureAccess.Controllers.Identity;
using DAL.ProcureAccess.Repos.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Identity;

/// <summary>
/// The legacy account deletion <c>DELETE /api/Users</c>: a principal that passes <c>[Authorize]</c>
/// but carries no user id counts as not signed in (S4-03, #205); a failure other than a missing user
/// throws, so the exception handler answers 500 Problem Details without the error text, never the
/// legacy <c>{ code, message }</c> body (S5-10, API-01). The 404 of a missing user runs through the
/// real pipeline in <c>UsersControllerAuthorizationTests</c>.
/// </summary>
public sealed class UsersControllerTests
{
    private readonly IUserRepo _repo = Substitute.For<IUserRepo>();

    [Fact]
    public async Task DeleteUser_SignedInWithoutUserId_Returns401AndSkipsRepository()
    {
        var controller = Controller(new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Email, "no-id@example.test")], authenticationType: "Test")));

        var result = await controller.DeleteUserAsync();

        result.ShouldBeOfType<UnauthorizedResult>();
        _repo.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task DeleteUser_OtherFailure_ThrowsForThe500Problem()
    {
        _repo.DeleteUserAsync("user-1").Returns(IdentityResult.Failed(
            new IdentityError { Code = "ConcurrencyFailure", Description = "Optimistic concurrency failure." }));

        var failure = await Should.ThrowAsync<InvalidOperationException>(
            () => Controller(SignedIn("user-1")).DeleteUserAsync());

        failure.Message.ShouldContain("ConcurrencyFailure");
        failure.Message.ShouldNotContain("Optimistic concurrency failure.");
    }

    private static ClaimsPrincipal SignedIn(string userId) =>
        new(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId)], authenticationType: "Test"));

    private UsersController Controller(ClaimsPrincipal user) =>
        new(_repo)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } },
        };
}
