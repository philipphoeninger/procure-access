// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Modules.IdentityAccess.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Tests.Shared.Auth;
using Shouldly;

namespace ProcureAccess.Tests.Integration.Infrastructure;

/// <summary>
/// The accounts of the fixture users (S5-20a), so the addresses and the confirmation gate (SEC-10)
/// are real for a test that signs in through the test scheme. Store them with <c>StoreAsync</c>.
/// The one helper for it since S6-10 (TD-75; formerly <c>LegacyAccounts</c> and three copies).
/// </summary>
internal static class TestAccounts
{
    /// <summary>The account of <paramref name="user"/>, its address confirmed or not.</summary>
    public static User Of(TestUser user, bool confirmed) => new()
    {
        Id = user.Id,
        UserName = user.Email,
        NormalizedUserName = user.Email.ToUpperInvariant(),
        Email = user.Email,
        NormalizedEmail = user.Email.ToUpperInvariant(),
        EmailConfirmed = confirmed,
        SecurityStamp = Guid.NewGuid().ToString(),
    };

    /// <summary>Confirms the stored account's address, as the confirmation link does.</summary>
    public static async Task ConfirmAsync(IServiceProvider services, TestUser user)
    {
        await using var scope = services.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var account = (await users.FindByIdAsync(user.Id)).ShouldNotBeNull();
        account.EmailConfirmed = true;
        (await users.UpdateAsync(account)).Succeeded.ShouldBeTrue();
    }
}
