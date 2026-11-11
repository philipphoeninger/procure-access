// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using API.ProcureAccess.Controllers.Identity;
using DAL.ProcureAccess.Repos.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MODELS.ProcureAccess.Entities;
using MODELS.ProcureAccess.Entities.Identity;
using NSubstitute;
using SERVICES.ProcureAccess.Logging.Interfaces;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Identity;

/// <summary>
/// A principal that passes <c>[Authorize]</c> but carries no user id counts as not signed in
/// (S4-03, #205): the action answers 401 and never reaches the repository with a null id.
/// </summary>
public sealed class UsersControllerTests
{
    private readonly IUserRepo _repo = Substitute.For<IUserRepo>();

    [Theory]
    [InlineData("change-email")]
    [InlineData("change-password")]
    [InlineData("revoke-sessions")]
    [InlineData("delete")]
    public async Task AccountAction_SignedInWithoutUserId_Returns401AndSkipsRepository(string action)
    {
        var controller = Controller(new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.Email, "no-id@example.test")], authenticationType: "Test")));

        var result = action switch
        {
            "change-email" => await controller.ChangeEmailAsync(new ChangeEmailRequest { NewEmail = "new@example.test" }),
            "change-password" => await controller.ChangePasswordAsync(
                new ChangePasswordRequest { CurrentPassword = "Old-Pass1", NewPassword = "New-Pass1" }),
            "revoke-sessions" => await controller.RevokeSessionsAsync(),
            "delete" => await controller.DeleteUserAsync(),
            _ => throw new ArgumentOutOfRangeException(nameof(action), action, null),
        };

        result.ShouldBeOfType<UnauthorizedResult>();
        _repo.ReceivedCalls().ShouldBeEmpty();
    }

    private UsersController Controller(ClaimsPrincipal user) =>
        new(
            Substitute.For<IAppLogging<UsersController>>(),
            Substitute.For<IHttpContextAccessor>(),
            _repo,
            Substitute.For<UserManager<User>>(Substitute.For<IUserStore<User>>(), null, null, null, null, null, null, null, null))
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } },
        };
}
