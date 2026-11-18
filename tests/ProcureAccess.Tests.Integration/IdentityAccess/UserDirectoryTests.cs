// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.IdentityAccess;

/// <summary>
/// <see cref="IUserDirectory"/> (S6-10): what another module may show of a user it knows only by
/// id. The display name is the e-mail address (owner decision S6-10 4); a deleted or unknown
/// account has none.
/// </summary>
public sealed class UserDirectoryTests(ApiFixture api) : IntegrationTest(api)
{
    [Fact]
    public async Task GetDisplayNames_KnownDeletedAndUnknownIds_NamesOnlyTheExistingAccounts()
    {
        await Api.ResetDatabaseAsync();
        var deleted = TestAccounts.Of(TestUsers.EditorA, confirmed: true);
        deleted.IsDeleted = true;
        await StoreAsync(
            TestAccounts.Of(TestUsers.OwnerA, confirmed: true),
            TestAccounts.Of(TestUsers.ReaderB, confirmed: false),
            deleted);

        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var directory = scope.ServiceProvider.GetRequiredService<IUserDirectory>();
        var names = await directory.GetDisplayNamesAsync(
            [TestUsers.OwnerA.Id, TestUsers.ReaderB.Id, TestUsers.EditorA.Id, "no-such-user", TestUsers.OwnerA.Id],
            TestContext.Current.CancellationToken);

        names.Count.ShouldBe(2);
        names[TestUsers.OwnerA.Id].ShouldBe(TestUsers.OwnerA.Email);
        names[TestUsers.ReaderB.Id].ShouldBe(TestUsers.ReaderB.Email);
    }

    [Fact]
    public async Task GetDisplayNames_NoIds_AnswersEmptyWithoutAQuery()
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var directory = scope.ServiceProvider.GetRequiredService<IUserDirectory>();

        (await directory.GetDisplayNamesAsync([], TestContext.Current.CancellationToken)).ShouldBeEmpty();
    }
}
