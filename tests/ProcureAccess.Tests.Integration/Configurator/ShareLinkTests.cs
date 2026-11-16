// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Buffers.Text;
using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Domain;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.Sharing;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Modules.Platform.Persistence;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Configurator;

/// <summary>
/// Share links of a saved list (S5-23; SPEC R-SAV-04, R-SAV-05, SD15, §8.5; ARCHITECTURE §5.3,
/// C59, C81): creating, listing and revoking them (Editor and Owner), and the anonymous read of
/// <c>GET /api/v1/shared/{token}</c>. Every test starts from the Tenants fixture with the legacy
/// accounts of its users stored (all confirmed but Reader B) and one list of A that needs no catalog.
/// The authorisation matrix is <c>AuthorizationMatrixTests</c> (TEN-05), the masking of the token in
/// the log sinks <c>LogSinkTests</c>.
/// </summary>
public sealed class ShareLinkTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string SharedPath = "/api/v1/shared";

    /// <summary>A note of the list: the shared view shows it (SPEC §8.5).</summary>
    private const string Note = "Mit dem Anbieter klären";

    private static readonly TimeZoneInfo Berlin = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");

    private Tenants? _tenants;

    private CriteriaConfiguration? _list;

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    private CriteriaConfiguration List => _list ?? throw new InvalidOperationException("The list is not stored.");

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        _tenants = Tenants.Build(Clock.GetUtcNow());
        _list = CriteriaConfiguration.Create(
            _tenants.A.Id, "Liste für die Rechtsabteilung", TestUsers.EditorA.Id, Clock.GetUtcNow(), MiniCatalog.T1Version, "hr",
            ["web"], ["base"], ["softwareTest"],
            [
                new ConfigurationCriterionDraft(2, PriorityLevel.A),
                new ConfigurationCriterionDraft(1, PriorityLevel.B, OverrideLevel: PriorityLevel.Info, Note: Note),
                new ConfigurationCriterionDraft(3, PriorityLevel.B, IsIncluded: false, OverrideWeight: 7),
            ]);
        await StoreAsync([.. _tenants.Entities, _list]);
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => LegacyAccounts.Of(u, confirmed: u != TestUsers.ReaderB))]);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact(DisplayName = "INV:TEN-08 " + nameof(Create_EditorOfA_ShowsTheTokenOnceAndStoresOnlyItsHash))]
    [Trait("Invariant", "TEN-08")]
    public async Task Create_EditorOfA_ShowsTheTokenOnceAndStoresOnlyItsHash()
    {
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PostAsJsonAsync(LinksPath(List.Id), new { expiresOn = InDays(90) }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(Cancellation));
        response.Headers.Location.ShouldNotBeNull().OriginalString.ShouldBe(LinksPath(List.Id));
        var created = (await response.Content.ReadFromJsonAsync<CreatedShareLink>(Cancellation)).ShouldNotBeNull();
        var bytes = Base64Url.DecodeFromChars(created.Token);
        bytes.Length.ShouldBe(32);

        await using var db = Platform(Fixture.A.Id);
        var row = await db.ShareLinks.SingleAsync(Cancellation);
        row.Id.ShouldBe(created.Id);
        row.OrganizationId.ShouldBe(Fixture.A.Id);
        row.ResourceType.ShouldBe(ShareableResourceType.Configuration);
        row.ResourceId.ShouldBe(List.Id);
        row.TokenHash.ShouldBe(SHA256.HashData(bytes));
        row.ExpiresAt.ShouldBe(created.ExpiresAt);

        // The token is in no column (TEN-08): the raw row as PostgreSQL returns it.
        var raw = await db.Database.SqlQueryRaw<string>(
                              "SELECT row_to_json(l)::text AS \"Value\" FROM sharing.\"ShareLinks\" l")
                          .SingleAsync(Cancellation);
        raw.ShouldNotContain(created.Token);
        raw.ShouldNotContain(Convert.ToBase64String(bytes).TrimEnd('='));

        // Listed later without it.
        using var listed = await client.GetAsync(LinksPath(List.Id), Cancellation);
        (await listed.Content.ReadAsStringAsync(Cancellation)).ShouldNotContain(created.Token);
    }

    [Fact(DisplayName = "INV:TEN-08 INV:TEN-09 " + nameof(Read_ValidLinkWithoutAnyCookie_Returns200WithTheRedactedList))]
    [Trait("Invariant", "TEN-08")]
    [Trait("Invariant", "TEN-09")]
    public async Task Read_ValidLinkWithoutAnyCookie_Returns200WithTheRedactedList()
    {
        var created = await CreateAsync(TestUsers.OwnerA, InDays(90));
        using var anonymous = Api.Factory.CreateDefaultClient(BaseAddress); // no cookie jar, no antiforgery hook

        using var response = await anonymous.GetAsync($"{SharedPath}/{created.Token}", Cancellation);

        var text = await response.Content.ReadAsStringAsync(Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, text);
        response.Headers.CacheControl.ShouldNotBeNull().NoStore.ShouldBeTrue();
        using var body = JsonDocument.Parse(text);
        var root = body.RootElement;
        root.EnumerateObject().Select(p => p.Name)
            .ShouldBe(["name", "releaseVersion", "expiresAt", "selection", "criteria"], ignoreOrder: true);
        root.GetProperty("name").GetString().ShouldBe("Liste für die Rechtsabteilung");
        root.GetProperty("releaseVersion").GetString().ShouldBe(MiniCatalog.T1Version);
        root.GetProperty("expiresAt").GetDateTimeOffset().ShouldBe(created.ExpiresAt);
        var selection = root.GetProperty("selection");
        selection.GetProperty("useCaseTemplateId").GetString().ShouldBe("hr");
        selection.GetProperty("appTypeIds").EnumerateArray().Select(v => v.GetString()).ShouldBe(["web"]);
        var criteria = root.GetProperty("criteria").EnumerateArray().ToList();
        criteria.Select(c => c.GetProperty("criterionRevisionId").GetInt32()).ShouldBe([1, 2, 3]);
        criteria[0].EnumerateObject().Select(p => p.Name).ShouldBe(
            ["criterionRevisionId", "isIncluded", "resolvedLevel", "overrideLevel", "overrideWeight", "note"], ignoreOrder: true);
        criteria[0].GetProperty("overrideLevel").GetString().ShouldBe("Info");
        criteria[0].GetProperty("note").GetString().ShouldBe(Note);
        criteria[2].GetProperty("isIncluded").GetBoolean().ShouldBeFalse();
        criteria[2].GetProperty("overrideWeight").GetInt32().ShouldBe(7);

        // No organisation, user, project or source data (TEN-08), by sentinel.
        string[] sentinels =
        [
            Fixture.A.Id.ToString(), "Organisation A", List.Id.ToString(), created.Id.ToString(),
            .. TestUsers.ByName.Values.SelectMany(u => new[] { u.Id, u.Email }),
            "organization", "createdBy", "project",
        ];
        foreach (var sentinel in sentinels)
        {
            text.ShouldNotContain(sentinel, Case.Insensitive);
        }
    }

    [Theory(DisplayName = "INV:TEN-08 " + nameof(Read_LinkNoLongerValid_Returns404))]
    [Trait("Invariant", "TEN-08")]
    [InlineData("revoked")]
    [InlineData("expired")]
    [InlineData("list-deleted")]
    [InlineData("unknown")]
    [InlineData("malformed")]
    public async Task Read_LinkNoLongerValid_Returns404(string state)
    {
        var created = await CreateAsync(TestUsers.EditorA, InDays(1));
        using var editor = CreateClient(TestUsers.EditorA);
        var token = created.Token;
        switch (state)
        {
            case "revoked":
                (await editor.DeleteAsync($"/api/v1/share-links/{created.Id}", Cancellation)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
                break;
            case "expired":
                Clock.SetUtcNow(created.ExpiresAt);
                break;
            case "list-deleted":
                (await editor.DeleteAsync($"/api/v1/configurations/{List.Id}", Cancellation)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
                break;
            case "unknown":
                token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
                break;
            case "malformed":
                token = token[..20];
                break;
        }

        using var anonymous = CreateClient();
        using var response = await anonymous.GetAsync($"{SharedPath}/{token}", Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await TypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.NotFound);
    }

    [Fact(DisplayName = "INV:TEN-08 " + nameof(Read_JustBeforeTheEndOfTheLastDay_StillReturns200))]
    [Trait("Invariant", "TEN-08")]
    public async Task Read_JustBeforeTheEndOfTheLastDay_StillReturns200()
    {
        var created = await CreateAsync(TestUsers.EditorA, InDays(1));
        Clock.SetUtcNow(created.ExpiresAt.AddSeconds(-1));
        using var anonymous = CreateClient();

        using var response = await anonymous.GetAsync($"{SharedPath}/{created.Token}", Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    /// <summary>
    /// TEN-12 with the clock at 23:30 UTC: already the next day in Berlin, in summer and in winter, so a
    /// server that counted in UTC would take „today“ for tomorrow (owner decision S5-23 2a).
    /// </summary>
    [Theory(DisplayName = "INV:TEN-12 " + nameof(Create_ExpiryDate_TomorrowToOneYearInBerlin))]
    [Trait("Invariant", "TEN-12")]
    [InlineData("missing", false)]
    [InlineData("yesterday", false)]
    [InlineData("today", false)]
    [InlineData("tomorrow", true)]
    [InlineData("one-year", true)]
    [InlineData("one-year-and-a-day", false)]
    public async Task Create_ExpiryDate_TomorrowToOneYearInBerlin(string date, bool accepted)
    {
        var berlinToday = SetClockToLateEveningUtc();
        DateOnly? expiresOn = date switch
        {
            "missing" => null,
            "yesterday" => berlinToday.AddDays(-1),
            "today" => berlinToday,
            "tomorrow" => berlinToday.AddDays(1),
            "one-year" => berlinToday.AddYears(1),
            "one-year-and-a-day" => berlinToday.AddYears(1).AddDays(1),
            _ => throw new ArgumentOutOfRangeException(nameof(date)),
        };
        using var client = CreateClient(TestUsers.EditorA);

        object body = expiresOn is { } day ? new { expiresOn = day.ToString("yyyy-MM-dd") } : new { };

        using var response = await client.PostAsJsonAsync(LinksPath(List.Id), body, Cancellation);

        if (accepted)
        {
            response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(Cancellation));
            var created = (await response.Content.ReadFromJsonAsync<CreatedShareLink>(Cancellation)).ShouldNotBeNull();
            var endOfDay = expiresOn!.Value.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
            created.ExpiresAt.ShouldBe(new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(endOfDay, Berlin)));
            TimeZoneInfo.ConvertTime(created.ExpiresAt, Berlin).TimeOfDay.ShouldBe(TimeSpan.Zero);
        }
        else
        {
            await ShouldBeProblemAsync(response, ProblemTypes.Validation, "expiresOn");
            await using var db = Platform(Fixture.A.Id);
            (await db.ShareLinks.CountAsync(Cancellation)).ShouldBe(0);
        }
    }

    [Fact]
    public async Task Create_UnknownMember_Returns400AndStoresNothing()
    {
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await client.PostAsJsonAsync(
            LinksPath(List.Id), new { expiresOn = InDays(30), scope = "write" }, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        await using var db = Platform(Fixture.A.Id);
        (await db.ShareLinks.CountAsync(Cancellation)).ShouldBe(0);
    }

    [Fact(DisplayName = "INV:TEN-01 " + nameof(List_EditorOfA_ShowsOnlyActiveLinksOfThisList))]
    [Trait("Invariant", "TEN-01")]
    public async Task List_EditorOfA_ShowsOnlyActiveLinksOfThisList()
    {
        var first = await CreateAsync(TestUsers.OwnerA, InDays(10));
        var revoked = await CreateAsync(TestUsers.OwnerA, InDays(10));
        var expiring = await CreateAsync(TestUsers.OwnerA, InDays(1));
        var other = CriteriaConfiguration.Create(
            Fixture.A.Id, "Andere Liste", TestUsers.OwnerA.Id, Clock.GetUtcNow(), MiniCatalog.T1Version, null, ["web"], [], [],
            [new ConfigurationCriterionDraft(1, PriorityLevel.A)]);
        await StoreAsync(other);
        await CreateAsync(TestUsers.OwnerA, InDays(10), other.Id);
        using var client = CreateClient(TestUsers.EditorA);
        (await client.DeleteAsync($"/api/v1/share-links/{revoked.Id}", Cancellation)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        Clock.SetUtcNow(expiring.ExpiresAt);

        var links = await client.GetFromJsonAsync<List<ActiveShareLink>>(LinksPath(List.Id), Cancellation);

        links.ShouldNotBeNull().Select(l => l.Id).ShouldBe([first.Id]);
        links[0].ExpiresAt.ShouldBe(first.ExpiresAt);
    }

    [Fact(DisplayName = "INV:TEN-01 " + nameof(Revoke_LinkOfAnotherOrganization_Returns404AndKeepsItWorking))]
    [Trait("Invariant", "TEN-01")]
    public async Task Revoke_LinkOfAnotherOrganization_Returns404AndKeepsItWorking()
    {
        var created = await CreateAsync(TestUsers.EditorA, InDays(10));
        using var ownerB = CreateClient(TestUsers.OwnerB);

        using var response = await ownerB.DeleteAsync($"/api/v1/share-links/{created.Id}", Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        using var anonymous = CreateClient();
        (await anonymous.GetAsync($"{SharedPath}/{created.Token}", Cancellation)).StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Revoke_Twice_Returns204AndKeepsTheFirstRevocation()
    {
        var created = await CreateAsync(TestUsers.EditorA, InDays(10));
        using var client = CreateClient(TestUsers.OwnerA);
        (await client.DeleteAsync($"/api/v1/share-links/{created.Id}", Cancellation)).StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var revokedAt = Clock.GetUtcNow();
        Clock.Advance(TimeSpan.FromMinutes(5));

        using var again = await client.DeleteAsync($"/api/v1/share-links/{created.Id}", Cancellation);

        again.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        await using var db = Platform(Fixture.A.Id);
        (await db.ShareLinks.SingleAsync(Cancellation)).RevokedAt.ShouldBe(revokedAt);
    }

    [Theory(DisplayName = "INV:SEC-10 " + nameof(CreateAndRevoke_UnconfirmedAddress_Return403EmailUnconfirmed))]
    [Trait("Invariant", "SEC-10")]
    [InlineData("create")]
    [InlineData("revoke")]
    public async Task CreateAndRevoke_UnconfirmedAddress_Return403EmailUnconfirmed(string action)
    {
        var created = await CreateAsync(TestUsers.OwnerA, InDays(10));
        await StoreAsync(OrganizationMembership.Create(Fixture.A.Id, TestUsers.ReaderB.Id, OrganizationRole.Editor, Clock.GetUtcNow(), Clock.GetUtcNow()));
        using var client = CreateClient(TestUsers.ReaderB);

        using var response = await SendAsync(client, action, created.Id);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await TypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.EmailUnconfirmed);
        await using var db = Platform(Fixture.A.Id);
        var rows = await db.ShareLinks.ToListAsync(Cancellation);
        rows.Count.ShouldBe(1);
        rows[0].RevokedAt.ShouldBeNull();

        await LegacyAccounts.ConfirmAsync(Api.Factory.Services, TestUsers.ReaderB);
        using var confirmed = await SendAsync(client, action, created.Id);
        confirmed.StatusCode.ShouldBe(action == "create" ? HttpStatusCode.Created : HttpStatusCode.NoContent);
    }

    private Task<HttpResponseMessage> SendAsync(HttpClient client, string action, Guid linkId) => action switch
    {
        "create" => client.PostAsJsonAsync(LinksPath(List.Id), new { expiresOn = InDays(30) }, Cancellation),
        "revoke" => client.DeleteAsync($"/api/v1/share-links/{linkId}", Cancellation),
        _ => throw new ArgumentOutOfRangeException(nameof(action)),
    };

    private async Task<CreatedShareLink> CreateAsync(TestUser user, string expiresOn, Guid? listId = null)
    {
        using var client = CreateClient(user);
        using var response = await client.PostAsJsonAsync(LinksPath(listId ?? List.Id), new { expiresOn }, Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(Cancellation));
        return (await response.Content.ReadFromJsonAsync<CreatedShareLink>(Cancellation)).ShouldNotBeNull();
    }

    private static string LinksPath(Guid listId) => $"/api/v1/configurations/{listId}/share-links";

    /// <summary>A date <paramref name="days"/> after today in Berlin, as the body names it.</summary>
    private string InDays(int days) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(Clock.GetUtcNow(), Berlin).DateTime).AddDays(days).ToString("yyyy-MM-dd");

    /// <summary>
    /// Moves the clock to the next 23:30 UTC — 00:30 or 01:30 of the next day in Berlin — and returns
    /// that Berlin day. The fixture's clock only moves forward.
    /// </summary>
    private DateOnly SetClockToLateEveningUtc()
    {
        var now = Clock.GetUtcNow();
        var lateEvening = new DateTimeOffset(now.UtcDateTime.Date.AddHours(23).AddMinutes(30), TimeSpan.Zero);
        if (lateEvening <= now)
        {
            lateEvening = lateEvening.AddDays(1);
        }

        Clock.SetUtcNow(lateEvening);
        var berlinToday = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(lateEvening, Berlin).DateTime);
        berlinToday.ShouldBe(DateOnly.FromDateTime(lateEvening.UtcDateTime).AddDays(1));
        return berlinToday;
    }

    /// <summary>A context of its own outside any request that sees <paramref name="organizationIds"/>.</summary>
    private PlatformDbContext Platform(params Guid[] organizationIds) =>
        new(new DbContextOptionsBuilder<PlatformDbContext>().UseNpgsql(Api.ConnectionString).Options,
            FixedOrganizationContext.Of(organizationIds));

    private static async Task ShouldBeProblemAsync(HttpResponseMessage response, string type, params string[] errorKeys)
    {
        var text = await response.Content.ReadAsStringAsync(Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, text);
        using var body = JsonDocument.Parse(text);
        body.RootElement.GetProperty("type").GetString().ShouldNotBeNull().ShouldEndWith("/problems/" + type);
        body.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name).ShouldBe(errorKeys, ignoreOrder: true);
    }

    private static async Task<string> TypeOf(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        return body.RootElement.GetProperty("type").GetString()!;
    }
}
