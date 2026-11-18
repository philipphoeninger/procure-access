// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text.Json;
using ProcureAccess.Modules.IdentityAccess.Accounts;
using MODELS.ProcureAccess.Entities;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Tests.Integration.Configurator;
using ProcureAccess.Tests.Integration.Export;
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
/// contributor profile; Owner B has one too, which Editor A's export must not show. The criteria
/// lists the user created came with S5-19 (owner decision S5-19 4), the settings with S5-25a (the
/// effective values, owner decision S5-25 4). The parts of PRV-07 that later tasks fill (projects
/// step 5, contributions and purged content step 6) are empty here (owner decision S5-27 1a).
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
        // The accounts first: organisations and memberships reference them (foreign keys, S6-10b).
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => AccountOf(u, u == TestUsers.EditorA ? _createdAt : null))]);

        // Editor A joined organisation B a day later as a Reader: two memberships, both shared with other users.
        var secondMembership = OrganizationMembership.Create(
            _tenants.B.Id, TestUsers.EditorA.Id, OrganizationRole.Reader, now, now.AddDays(1));
        _ownProfile = ContributorProfile.Create("Erfahrungen Anna", ContributorKind.Individual, now.AddHours(-2));
        _otherProfile = ContributorProfile.Create("Prüfstelle Nord", ContributorKind.Auditor, now.AddHours(-1));

        await StoreAsync([.. _tenants.Entities, secondMembership]);
        await StoreAsync(
            _ownProfile, UserContributorLink.Create(TestUsers.EditorA.Id, _ownProfile),
            _otherProfile, UserContributorLink.Create(TestUsers.OwnerB.Id, _otherProfile));
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

    [Fact(DisplayName = "INV:PRV-07 " + nameof(Export_UserWhoCreatedLists_ContainsTheirContentAndNoListOfOthersOrOfALeftOrganization))]
    [Trait("Invariant", "PRV-07")]
    public async Task Export_UserWhoCreatedLists_ContainsTheirContentAndNoListOfOthersOrOfALeftOrganization()
    {
        await ExportTestCatalog.ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
        var selection = await SavedLists.ResolveAsync(Api.Factory.Services, MiniCatalog.T1Version, Cancellation);
        var now = Clock.GetUtcNow();
        var (excluded, changed) = (selection.Criteria[0], selection.Criteria[1]);
        var changedTo = changed.RecommendedPriority.Level == PriorityLevel.B ? PriorityLevel.A : PriorityLevel.B;

        // Editor A created two lists: one in A with decisions, a later one in B (where Editor A only reads by now).
        var own = SavedLists.Build(Fixture.A.Id, TestUsers.EditorA.Id, "Vergabe 2027", now.AddHours(-3), MiniCatalog.T1Version, selection, c =>
            c == excluded ? SavedLists.Draft(c, isIncluded: false)
            : c == changed ? SavedLists.Draft(c, overrideLevel: changedTo, overrideWeight: changedTo == PriorityLevel.B ? 8 : null, note: "Mit Anbieter klären")
            : null);
        var ownInB = SavedLists.Build(Fixture.B.Id, TestUsers.EditorA.Id, "Rahmenvertrag", now.AddHours(-1), MiniCatalog.T1Version, selection);
        // Not Editor A's: a list of a colleague in A, and one Editor A created in an organisation they have left.
        var colleagues = SavedLists.Build(Fixture.A.Id, TestUsers.OwnerA.Id, "Liste der Kollegin", now, MiniCatalog.T1Version, selection);
        var left = Organization.Create("Verlassene Organisation", OrganizationType.Association, TestUsers.OwnerB.Id, now);
        var inLeft = SavedLists.Build(left.Id, TestUsers.EditorA.Id, "Liste von früher", now, MiniCatalog.T1Version, selection);
        await StoreAsync(left, own, ownInB, colleagues, inLeft);
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.GetAsync(Export, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var text = await response.Content.ReadAsStringAsync(Cancellation);
        using var body = JsonDocument.Parse(text);
        var lists = body.RootElement.GetProperty("createdByMe").GetProperty("criteriaLists").EnumerateArray().ToList();
        lists.Select(l => (l.GetProperty("name").GetString(), l.GetProperty("organization").GetString(), l.GetProperty("release").GetString()))
             .ShouldBe([("Vergabe 2027", "Organisation A", "T.1"), ("Rahmenvertrag", "Organisation B", "T.1")]);

        var first = lists[0];
        first.EnumerateObject().Select(p => p.Name).ShouldBe(["name", "organization", "release", "createdAt", "selection", "criteria"]);
        first.GetProperty("createdAt").GetDateTimeOffset().ShouldBe(now.AddHours(-3));
        var chosen = first.GetProperty("selection");
        chosen.EnumerateObject().Select(p => p.Name).ShouldBe(["useCase", "appTypes", "productParts", "testTypes"]);
        chosen.GetProperty("useCase").GetString().ShouldBe(selection.UseCase!.Name);
        chosen.GetProperty("appTypes").EnumerateArray().Select(v => v.GetString()).ShouldBe(selection.AppTypes.Select(v => v.Name));
        chosen.GetProperty("productParts").EnumerateArray().Select(v => v.GetString()).ShouldBe(selection.ProductParts.Select(v => v.Name));
        chosen.GetProperty("testTypes").EnumerateArray().Select(v => v.GetString()).ShouldBe(selection.TestTypes.Select(v => v.Name));

        // Every criterion of the selection in list order, with the user's decision next to the recommendation.
        var criteria = first.GetProperty("criteria").EnumerateArray().ToList();
        criteria[0].EnumerateObject().Select(p => p.Name).ShouldBe(
            ["number", "title", "isIncluded", "recommendedLevel", "effectiveLevel", "weight", "note"]);
        criteria.Select(c => c.GetProperty("title").GetString()).ShouldBe(selection.Criteria.Select(c => c.PlainLanguageTitle ?? c.Title));
        criteria.Select(c => c.GetProperty("number").GetString()).ShouldBe(selection.Criteria.Select(c => c.Number));
        criteria.Select(c => c.GetProperty("recommendedLevel").GetString())
                .ShouldBe(selection.Criteria.Select(c => c.RecommendedPriority.Level.ToString()));
        criteria.Select(c => c.GetProperty("isIncluded").GetBoolean()).ShouldBe(selection.Criteria.Select(c => c != excluded));
        criteria[1].GetProperty("effectiveLevel").GetString().ShouldBe(changedTo.ToString());
        criteria[1].GetProperty("note").GetString().ShouldBe("Mit Anbieter klären");
        if (changedTo == PriorityLevel.B)
        {
            criteria[1].GetProperty("weight").GetInt32().ShouldBe(8);
        }
        else
        {
            criteria[1].GetProperty("weight").ValueKind.ShouldBe(JsonValueKind.Null);
        }

        criteria[2].GetProperty("effectiveLevel").GetString().ShouldBe(selection.Criteria[2].RecommendedPriority.Level.ToString());
        criteria[2].GetProperty("note").ValueKind.ShouldBe(JsonValueKind.Null);

        // Nothing of the other lists, and no id: no list, organisation or user id, no revision id member.
        text.ShouldNotContain("Liste der Kollegin");
        text.ShouldNotContain("Liste von früher");
        text.ShouldNotContain("Verlassene Organisation");
        foreach (var id in new[] { own.Id, ownInB.Id, colleagues.Id, inLeft.Id, Fixture.A.Id, Fixture.B.Id, left.Id })
        {
            text.ShouldNotContain(id.ToString(), Case.Insensitive);
        }

        text.ShouldNotContain(TestUsers.EditorA.Id, Case.Insensitive);
        text.ShouldNotContain(TestUsers.OwnerA.Id, Case.Insensitive);
        text.ShouldNotContain("revisionId", Case.Insensitive);
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
        // Never saved: the defaults (owner decision S5-25 4).
        PreferencesOf(account).ShouldBe(("de", "system", "system", "system"));

        root.GetProperty("memberships").GetArrayLength().ShouldBe(0);
        root.GetProperty("contributorProfiles").GetArrayLength().ShouldBe(0);
        root.GetProperty("contributions").GetArrayLength().ShouldBe(0);
        var createdByMe = root.GetProperty("createdByMe");
        createdByMe.EnumerateObject().Select(p => p.Name).ShouldBe(["criteriaLists", "projects"]);
        createdByMe.GetProperty("criteriaLists").GetArrayLength().ShouldBe(0);
        createdByMe.GetProperty("projects").GetArrayLength().ShouldBe(0);
    }

    [Fact(DisplayName = "INV:PRV-07 " + nameof(Export_UserWhoSavedSettings_ContainsTheUsersOwnSettings))]
    [Trait("Invariant", "PRV-07")]
    public async Task Export_UserWhoSavedSettings_ContainsTheUsersOwnSettings()
    {
        // Editor A chose dark and reduced motion; Owner B, light — Editor A's export shows only Editor A's.
        var own = UserPreferences.Defaults(TestUsers.EditorA.Id);
        own.Change(UserPreferences.German, AppearancePreference.Dark, MotionPreference.Reduced, ContrastPreference.System);
        var other = UserPreferences.Defaults(TestUsers.OwnerB.Id);
        other.Change(UserPreferences.German, AppearancePreference.Light, MotionPreference.System, ContrastPreference.System);
        await StoreAsync(own, other);
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.GetAsync(Export, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var text = await response.Content.ReadAsStringAsync(Cancellation);
        using var body = JsonDocument.Parse(text);
        var account = body.RootElement.GetProperty("account");
        account.GetProperty("preferences").EnumerateObject().Select(p => p.Name).ShouldBe(["language", "appearance", "motion", "contrast"]);
        PreferencesOf(account).ShouldBe(("de", "dark", "reduced", "system"));
        text.ShouldNotContain("\"light\"");
        text.ShouldNotContain(TestUsers.EditorA.Id, Case.Insensitive);
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

    private static (string?, string?, string?, string?) PreferencesOf(JsonElement account)
    {
        var preferences = account.GetProperty("preferences");
        return (
            preferences.GetProperty("language").GetString(),
            preferences.GetProperty("appearance").GetString(),
            preferences.GetProperty("motion").GetString(),
            preferences.GetProperty("contrast").GetString());
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
