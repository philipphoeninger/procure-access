// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text.Json;
using API.ProcureAccess.Account;
using MODELS.ProcureAccess.Entities;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.IdentityAccess;

/// <summary>
/// The personal data export <c>GET /api/v1/account/export</c> (S5-27; SPEC R-AUTH-09, §8.4;
/// ARCHITECTURE C79; PRV-07, account part). Every test starts from the Tenants fixture with the
/// legacy accounts of its users stored. Editor A is also a Reader of organisation B and has a
/// contributor profile; Owner B has one too, which Editor A's export must not show. The parts of
/// PRV-07 that later tasks fill (criteria lists S5-19, preferences S5-25, projects step 5,
/// contributions and purged content step 6) are empty or <c>null</c> here (owner decision S5-27 1a).
/// </summary>
public sealed class AccountExportTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Export = "/api/v1/account/export";

    private Tenants? _tenants;
    private ContributorProfile? _ownProfile;
    private ContributorProfile? _otherProfile;
    private DateTimeOffset _createdAt;

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        var now = Clock.GetUtcNow();
        _createdAt = now.AddDays(-30);
        _tenants = Tenants.Build(now);

        // Editor A joined organisation B a day later as a Reader: two memberships, both shared with other users.
        var secondMembership = OrganizationMembership.Create(
            _tenants.B.Id, TestUsers.EditorA.Id, OrganizationRole.Reader, now, now.AddDays(1));
        _ownProfile = ContributorProfile.Create("Erfahrungen Anna", ContributorKind.Individual, now.AddHours(-2));
        _otherProfile = ContributorProfile.Create("Prüfstelle Nord", ContributorKind.Auditor, now.AddHours(-1));

        await StoreAsync([.. _tenants.Entities, secondMembership]);
        await StoreAsync(
            _ownProfile, UserContributorLink.Create(TestUsers.EditorA.Id, _ownProfile),
            _otherProfile, UserContributorLink.Create(TestUsers.OwnerB.Id, _otherProfile));
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => AccountOf(u, u == TestUsers.EditorA ? _createdAt : null))]);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact(DisplayName = "INV:PRV-07 " + nameof(Export_UserWithMembershipsAndProfile_ContainsTheUsersOwnDataOnly))]
    [Trait("Invariant", "PRV-07")]
    public async Task Export_UserWithMembershipsAndProfile_ContainsTheUsersOwnDataOnly()
    {
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.GetAsync(Export, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var text = await response.Content.ReadAsStringAsync(Cancellation);
        using var body = JsonDocument.Parse(text);
        var root = body.RootElement;

        var account = root.GetProperty("account");
        account.GetProperty("email").GetString().ShouldBe(TestUsers.EditorA.Email);
        account.GetProperty("createdAt").GetDateTimeOffset().ShouldBe(_createdAt);
        account.GetProperty("emailConfirmed").GetBoolean().ShouldBeTrue();

        var now = Clock.GetUtcNow();
        root.GetProperty("memberships").EnumerateArray()
            .Select(m => (m.GetProperty("organization").GetString(), m.GetProperty("role").GetString(), m.GetProperty("since").GetDateTimeOffset()))
            .ShouldBe([("Organisation A", "editor", now), ("Organisation B", "reader", now.AddDays(1))]);
        root.GetProperty("contributorProfiles").EnumerateArray()
            .Select(p => (p.GetProperty("displayName").GetString(), p.GetProperty("kind").GetString(), p.GetProperty("createdAt").GetDateTimeOffset()))
            .ShouldBe([("Erfahrungen Anna", "individual", _ownProfile!.CreatedAt)]);

        // Nothing of another user, and no id at all — neither the user's own nor an organisation's or a profile's.
        foreach (var other in TestUsers.ByName.Values.Where(u => u != TestUsers.EditorA))
        {
            text.ShouldNotContain(other.Email, Case.Insensitive);
            text.ShouldNotContain(other.Id, Case.Insensitive);
        }

        text.ShouldNotContain(TestUsers.EditorA.Id, Case.Insensitive);
        text.ShouldNotContain(Fixture.A.Id.ToString(), Case.Insensitive);
        text.ShouldNotContain(Fixture.B.Id.ToString(), Case.Insensitive);
        text.ShouldNotContain(_ownProfile.Id.ToString(), Case.Insensitive);
        text.ShouldNotContain(_otherProfile!.Id.ToString(), Case.Insensitive);
        text.ShouldNotContain("Prüfstelle Nord");
    }

    [Fact(DisplayName = "INV:PRV-07 " + nameof(Export_UserWithoutMembershipOrProfile_HasEverySectionOfSpec84AndTheUnbuiltOnesEmpty))]
    [Trait("Invariant", "PRV-07")]
    public async Task Export_UserWithoutMembershipOrProfile_HasEverySectionOfSpec84AndTheUnbuiltOnesEmpty()
    {
        using var client = CreateClient(TestUsers.Member);

        using var response = await client.GetAsync(Export, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        var root = body.RootElement;
        root.EnumerateObject().Select(p => p.Name).ShouldBe(
            ["generatedAt", "formatVersion", "account", "memberships", "contributorProfiles", "contributions", "createdByMe"]);
        root.GetProperty("generatedAt").GetDateTimeOffset().ShouldBe(Clock.GetUtcNow());
        root.GetProperty("formatVersion").GetInt32().ShouldBe(1);

        var account = root.GetProperty("account");
        account.EnumerateObject().Select(p => p.Name).ShouldBe(["email", "createdAt", "emailConfirmed", "preferences"]);
        account.GetProperty("email").GetString().ShouldBe(TestUsers.Member.Email);
        account.GetProperty("createdAt").ValueKind.ShouldBe(JsonValueKind.Null); // an account from before S5-27
        account.GetProperty("preferences").ValueKind.ShouldBe(JsonValueKind.Null); // until S5-25

        root.GetProperty("memberships").GetArrayLength().ShouldBe(0);
        root.GetProperty("contributorProfiles").GetArrayLength().ShouldBe(0);
        root.GetProperty("contributions").GetArrayLength().ShouldBe(0);
        var createdByMe = root.GetProperty("createdByMe");
        createdByMe.EnumerateObject().Select(p => p.Name).ShouldBe(["criteriaLists", "projects"]);
        createdByMe.GetProperty("criteriaLists").GetArrayLength().ShouldBe(0);
        createdByMe.GetProperty("projects").GetArrayLength().ShouldBe(0);
    }

    [Fact]
    public async Task Export_SignedIn_IsAJsonAttachmentNamedByTheDayAndNeverStored()
    {
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.GetAsync(Export, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/json");
        var disposition = response.Content.Headers.ContentDisposition.ShouldNotBeNull();
        disposition.DispositionType.ShouldBe("attachment");
        var day = Clock.GetUtcNow().UtcDateTime.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        disposition.FileName.ShouldBe($"ProcureAccess_Daten_{day}.json");
        AccountExportEndpoints.FileNameOf(Clock.GetUtcNow()).ShouldBe($"ProcureAccess_Daten_{day}.json");
        response.Headers.CacheControl.ShouldNotBeNull().NoStore.ShouldBeTrue();
    }

    [Fact]
    public async Task Export_Anonymous_Returns401()
    {
        using var client = CreateClient();

        using var response = await client.GetAsync(Export, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Export_DeletedAccount_Returns404NotStored()
    {
        await Api.ResetDatabaseAsync();
        await StoreAsync(AccountOf(TestUsers.Member, createdAt: null, deleted: true));
        using var client = CreateClient(TestUsers.Member);

        using var response = await client.GetAsync(Export, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        response.Headers.CacheControl.ShouldNotBeNull().NoStore.ShouldBeTrue();
    }

    private static User AccountOf(TestUser user, DateTimeOffset? createdAt, bool deleted = false) => new()
    {
        Id = user.Id,
        UserName = user.Email,
        NormalizedUserName = user.Email.ToUpperInvariant(),
        Email = user.Email,
        NormalizedEmail = user.Email.ToUpperInvariant(),
        EmailConfirmed = true,
        SecurityStamp = Guid.NewGuid().ToString(),
        CreatedAt = createdAt,
        IsDeleted = deleted,
    };
}
