// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.IdentityAccess.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.IdentityAccess;

/// <summary>
/// The clean-up of the account tokens (S6-05, TD-59, #383; task decision D1 (a)): a token, used or
/// not, is deleted once it has expired; a valid one stays, also when it was used, so a second click
/// on a confirmation link still answers as confirmed. Time is the fake clock.
/// </summary>
public sealed class AccountTokenCleanupTests(ApiFixture api) : IntegrationTest(api)
{
    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    [Fact]
    public async Task DeleteExpired_UsedAndUnusedTokensAroundTheirExpiry_DeletesOnlyTheExpired()
    {
        var now = Clock.GetUtcNow();
        const string userId = "user-of-the-token-clean-up";
        var expiredUnused = Token(userId, AccountTokenPurpose.PasswordReset, now.AddHours(-3), TimeSpan.FromHours(2));
        var expiredUsed = Token(userId, AccountTokenPurpose.EmailConfirmation, now.AddHours(-25), TimeSpan.FromHours(24));
        var expiresNow = Token(userId, AccountTokenPurpose.PasswordReset, now.AddHours(-2), TimeSpan.FromHours(2));
        var validUnused = Token(userId, AccountTokenPurpose.PasswordReset, now, TimeSpan.FromHours(2));
        var validUsed = Token(userId, AccountTokenPurpose.EmailConfirmation, now.AddHours(-1), TimeSpan.FromHours(24));
        await StoreAsync(TestAccounts.WithId(userId), expiredUnused, expiredUsed, expiresNow, validUnused, validUsed);
        await MarkUsedAsync(now.AddHours(-24), expiredUsed);
        await MarkUsedAsync(now.AddMinutes(-30), validUsed);

        int deleted;
        await using (var scope = Api.Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
            deleted = await AccountTokenCleanupService.DeleteExpiredAsync(db, now, Cancellation);
        }

        deleted.ShouldBe(3);
        (await RemainingAsync(userId)).ShouldBe([validUnused.Id, validUsed.Id], ignoreOrder: true);
    }

    [Fact]
    public async Task HostedService_ClockAdvancedByCleanupInterval_DeletesTokensThatExpiredMeanwhile()
    {
        var interval = Api.Factory.Services.GetRequiredService<AccountTokenSettings>().CleanupInterval;
        var now = Clock.GetUtcNow();
        const string userId = "user-of-the-hosted-token-clean-up";
        // Valid now, expired once the clock has moved on by one interval.
        var expires = Token(userId, AccountTokenPurpose.PasswordReset, now - TimeSpan.FromHours(2) + interval / 2, TimeSpan.FromHours(2));
        var stays = Token(userId, AccountTokenPurpose.EmailConfirmation, now, TimeSpan.FromHours(24));
        await StoreAsync(TestAccounts.WithId(userId), expires, stays);

        Clock.Advance(interval);

        // The hosted service runs on its own thread when its timer fires.
        var deadline = DateTime.UtcNow.AddSeconds(30);
        var remaining = await RemainingAsync(userId);
        while (remaining.Contains(expires.Id) && DateTime.UtcNow < deadline)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100), Cancellation);
            remaining = await RemainingAsync(userId);
        }

        remaining.ShouldBe([stays.Id]);
    }

    private static AccountToken Token(string userId, AccountTokenPurpose purpose, DateTimeOffset createdAt, TimeSpan lifetime) =>
        AccountToken.Create(userId, purpose, RandomNumberGenerator.GetBytes(AccountToken.HashLength), createdAt, lifetime);

    private async Task MarkUsedAsync(DateTimeOffset usedAt, AccountToken token)
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
        await db.AccountTokens
            .Where(t => t.Id == token.Id)
            .ExecuteUpdateAsync(set => set.SetProperty(t => t.UsedAt, usedAt), Cancellation);
    }

    private async Task<List<Guid>> RemainingAsync(string userId)
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
        return await db.AccountTokens.Where(t => t.UserId == userId).Select(t => t.Id).ToListAsync(Cancellation);
    }
}
