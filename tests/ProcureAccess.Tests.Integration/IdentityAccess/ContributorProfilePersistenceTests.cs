// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.IdentityAccess;

/// <summary>
/// The tables <c>identity."ContributorProfiles"</c> and <c>identity."UserContributorLinks"</c>
/// (S4-25, ARCHITECTURE §5.2): display names are unique regardless of case (SPEC §7.2), a profile
/// has at most one user, and the link goes with its profile.
/// </summary>
public sealed class ContributorProfilePersistenceTests(ApiFixture api) : IntegrationTest(api)
{
    private const string UniqueViolation = "23505";

    [Fact]
    public async Task Store_ProfileWithLink_ReadsBackTheSameValues()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Api.ResetDatabaseAsync();
        var profile = ContributorProfile.Create("Leselupe", ContributorKind.Auditor, Clock.GetUtcNow());

        await StoreAsync(profile, UserContributorLink.Create("user-1", profile));

        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
        var stored = await db.ContributorProfiles.AsNoTracking().SingleAsync(cancellationToken);
        stored.Id.ShouldBe(profile.Id);
        stored.DisplayName.ShouldBe("Leselupe");
        stored.Kind.ShouldBe(ContributorKind.Auditor);
        stored.CreatedAt.ShouldBe(profile.CreatedAt);
        (await db.UserContributorLinks.AsNoTracking().SingleAsync(cancellationToken)).UserId.ShouldBe("user-1");
    }

    [Theory]
    [InlineData("leselupe")]
    [InlineData("LESELUPE")]
    public async Task Store_DisplayNameDifferingOnlyInCase_IsRefused(string secondName)
    {
        await Api.ResetDatabaseAsync();
        await StoreAsync(ContributorProfile.Create("Leselupe", ContributorKind.Individual, Clock.GetUtcNow()));

        var exception = await Should.ThrowAsync<DbUpdateException>(() =>
            StoreAsync(ContributorProfile.Create(secondName, ContributorKind.Vendor, Clock.GetUtcNow())));

        exception.InnerException.ShouldBeOfType<PostgresException>().SqlState.ShouldBe(UniqueViolation);
    }

    [Fact]
    public async Task Store_DisplayNamesDifferingInAnUmlaut_AreBothKept()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Api.ResetDatabaseAsync();

        await StoreAsync(
            ContributorProfile.Create("Bar", ContributorKind.Individual, Clock.GetUtcNow()),
            ContributorProfile.Create("Bär", ContributorKind.Individual, Clock.GetUtcNow()));

        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
        (await db.ContributorProfiles.CountAsync(cancellationToken)).ShouldBe(2);
    }

    [Fact]
    public async Task Store_SecondLinkForTheSameProfile_IsRefused()
    {
        await Api.ResetDatabaseAsync();
        var profile = ContributorProfile.Create("Leselupe", ContributorKind.Individual, Clock.GetUtcNow());
        await StoreAsync(profile, UserContributorLink.Create("user-1", profile));

        var exception = await Should.ThrowAsync<DbUpdateException>(() =>
            StoreAsync(UserContributorLink.Create("user-2", profile)));

        exception.InnerException.ShouldBeOfType<PostgresException>().SqlState.ShouldBe(UniqueViolation);
    }

    [Fact]
    public async Task Store_SeveralProfilesOfOneUser_AreAllLinked()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Api.ResetDatabaseAsync();
        var individual = ContributorProfile.Create("Leselupe", ContributorKind.Individual, Clock.GetUtcNow());
        var vendor = ContributorProfile.Create("Beispiel GmbH", ContributorKind.Vendor, Clock.GetUtcNow());

        await StoreAsync(individual, vendor, UserContributorLink.Create("user-1", individual), UserContributorLink.Create("user-1", vendor));

        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
        (await db.UserContributorLinks.CountAsync(l => l.UserId == "user-1", cancellationToken)).ShouldBe(2);
    }

    [Fact]
    public async Task Delete_Profile_DeletesItsLink()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await Api.ResetDatabaseAsync();
        var profile = ContributorProfile.Create("Leselupe", ContributorKind.Individual, Clock.GetUtcNow());
        await StoreAsync(profile, UserContributorLink.Create("user-1", profile));

        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityAccessDbContext>();
        await db.ContributorProfiles.Where(p => p.Id == profile.Id).ExecuteDeleteAsync(cancellationToken);

        (await db.UserContributorLinks.AnyAsync(cancellationToken)).ShouldBeFalse();
    }
}
