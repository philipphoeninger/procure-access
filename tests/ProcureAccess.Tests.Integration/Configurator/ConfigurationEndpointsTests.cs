// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.Catalog;
using ProcureAccess.Modules.Configurator.Configurations;
using ProcureAccess.Modules.Configurator.Persistence;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Integration.Export.ExportTestCatalog;

namespace ProcureAccess.Tests.Integration.Configurator;

/// <summary>
/// The saved-list endpoints (S5-20a; SPEC R-SAV-01, R-SAV-02, §8.10; ARCHITECTURE §7.2, C153):
/// saving a draft in one call with the check of the anonymous export, reading a list and listing the
/// organisation's lists. Every test starts from the Tenants fixture with the legacy accounts of its
/// users stored (all confirmed but Reader B), and the test releases with <c>T.1</c> active. The
/// authorisation matrix is <c>AuthorizationMatrixTests</c> (TEN-05).
/// </summary>
public sealed class ConfigurationEndpointsTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Path = "/api/v1/configurations";

    private Tenants? _tenants;

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        await ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
        _tenants = Tenants.Build(Clock.GetUtcNow());
        await StoreAsync(_tenants.Entities);
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => TestAccounts.Of(u, confirmed: u != TestUsers.ReaderB))]);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Save_EditorOfA_StoresTheDraftWithTheCatalogsRecommendationAndReadsItBack))]
    [Trait("Invariant", "EXP-07")]
    public async Task Save_EditorOfA_StoresTheDraftWithTheCatalogsRecommendationAndReadsItBack()
    {
        using var client = CreateClient(TestUsers.EditorA);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        var (left, changed) = (KeyAt(criteria, 0), KeyAt(criteria, 1));
        var weighted = criteria.Skip(2).First(c => c!["resolvedPriority"]!["level"]!.GetValue<string>() == "B")!["key"]!.GetValue<string>();
        var body = SaveBody(Fixture.A.Id, "  Vergabe Personalsoftware 2027  ", criteria, (key, entry) =>
        {
            if (key == left)
            {
                entry["isIncluded"] = false;
            }
            else if (key == changed)
            {
                entry["overrideLevel"] = "Info";
                entry["note"] = "Mit Anbieter klären";
            }
            else if (key == weighted)
            {
                entry["overrideWeight"] = 9;
            }
        });

        using var response = await client.PostAsJsonAsync(Path, body, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(Cancellation));
        var created = (await response.Content.ReadFromJsonAsync<CreatedConfiguration>(Cancellation)).ShouldNotBeNull();
        response.Headers.Location.ShouldNotBeNull().ToString().ShouldBe($"{Path}/{created.Id}");

        var list = (await client.GetFromJsonAsync<ConfigurationResponse>($"{Path}/{created.Id}", Cancellation)).ShouldNotBeNull();
        list.ShouldSatisfyAllConditions(
            l => l.OrganizationId.ShouldBe(Fixture.A.Id),
            l => l.Name.ShouldBe("Vergabe Personalsoftware 2027"),
            l => l.ReleaseVersion.ShouldBe(MiniCatalog.T1Version),
            l => l.CreatedAt.ShouldBe(Clock.GetUtcNow()),
            l => l.ModifiedAt.ShouldBe(Clock.GetUtcNow()),
            l => l.Selection.UseCaseTemplateId.ShouldBe("hr"),
            l => l.Selection.AppTypeIds.ShouldBe(["web"]),
            l => l.Selection.ProductPartIds.ShouldBe(["base"]),
            l => l.Selection.TestTypeIds.ShouldBe(["softwareTest"]));
        // One decision per criterion of the selection, by revision id, with the recommendation the catalog resolved.
        var sent = criteria.Select(c => c!).OrderBy(c => c["revisionId"]!.GetValue<int>()).ToList();
        list.Criteria.Select(c => c.CriterionRevisionId).ShouldBe(sent.Select(c => c["revisionId"]!.GetValue<int>()));
        list.Criteria.Select(c => c.ResolvedLevel).ShouldBe(sent.Select(c => c["resolvedPriority"]!["level"]!.GetValue<string>()));
        var byKey = sent.Zip(list.Criteria).ToDictionary(p => p.First["key"]!.GetValue<string>(), p => p.Second);
        byKey[left].IsIncluded.ShouldBeFalse();
        byKey[changed].ShouldSatisfyAllConditions(c => c.OverrideLevel.ShouldBe("Info"), c => c.Note.ShouldBe("Mit Anbieter klären"));
        byKey[weighted].ShouldSatisfyAllConditions(c => c.OverrideLevel.ShouldBeNull(), c => c.OverrideWeight.ShouldBe(9));

        await using var db = Configurator(Fixture.A.Id);
        (await db.CriteriaConfigurations.SingleAsync(c => c.Id == created.Id, Cancellation)).CreatedByUserId.ShouldBe(TestUsers.EditorA.Id);
    }

    [Fact]
    public async Task Save_OwnerOfA_Returns201()
    {
        using var client = CreateClient(TestUsers.OwnerA);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);

        using var response = await client.PostAsJsonAsync(Path, SaveBody(Fixture.A.Id, new string('a', 120), criteria), Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Theory]
    [InlineData(null, "name")]
    [InlineData("   ", "name")]
    [InlineData("121", "name")]
    public async Task Save_InvalidName_Returns400AtNameAndStoresNothing(string? name, string expectedKey)
    {
        using var client = CreateClient(TestUsers.EditorA);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);

        using var response = await client.PostAsJsonAsync(
            Path, SaveBody(Fixture.A.Id, name == "121" ? new string('a', 121) : name, criteria), Cancellation);

        await ShouldBeProblemAsync(response, ProblemTypes.Validation, expectedKey);
        (await CountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Save_InvalidNameAndDraft_NamesBothInOneAnswer()
    {
        using var client = CreateClient(TestUsers.EditorA);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        var first = KeyAt(criteria, 0);
        var body = SaveBody(Fixture.A.Id, "", criteria, (key, entry) =>
        {
            if (key == first)
            {
                entry["overrideLevel"] = "C";
            }
        });

        using var response = await client.PostAsJsonAsync(Path, body, Cancellation);

        await ShouldBeProblemAsync(response, ProblemTypes.Validation, "criteria[0].overrideLevel", "name");
    }

    [Fact]
    public async Task Save_WithoutOrganization_Returns400AtOrganizationId()
    {
        using var client = CreateClient(TestUsers.EditorA);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        var body = SaveBody(Fixture.A.Id, "Liste", criteria);
        body.Remove("organizationId");

        using var response = await client.PostAsJsonAsync(Path, body, Cancellation);

        await ShouldBeProblemAsync(response, ProblemTypes.Validation, "organizationId");
    }

    [Fact(DisplayName = "INV:EXP-07 " + nameof(Save_RecommendedLevelInTheBody_IsRefused))]
    [Trait("Invariant", "EXP-07")]
    public async Task Save_RecommendedLevelInTheBody_IsRefused()
    {
        // The recommendation is the server's: a client cannot hand one in (unknown members are refused).
        using var client = CreateClient(TestUsers.EditorA);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        var first = KeyAt(criteria, 0);
        var body = SaveBody(Fixture.A.Id, "Liste", criteria, (key, entry) =>
        {
            if (key == first)
            {
                entry["resolvedLevel"] = "Info";
            }
        });

        using var response = await client.PostAsJsonAsync(Path, body, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        (await CountAsync()).ShouldBe(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public async Task Save_WeightOutsideTheConfiguredRange_Returns400AtTheWeight(int weight)
    {
        using var client = CreateClient(TestUsers.EditorA);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        var weighted = FirstOfLevel(criteria, "B");
        var body = SaveBody(Fixture.A.Id, "Liste", criteria, (key, entry) =>
        {
            if (key == weighted)
            {
                entry["overrideWeight"] = weight;
            }
        });

        using var response = await client.PostAsJsonAsync(Path, body, Cancellation);

        await ShouldBeProblemAsync(response, ProblemTypes.Validation, $"criteria[{IndexOf(criteria, weighted)}].overrideWeight");
    }

    [Fact]
    public async Task Save_MissingCriterion_Returns400AtCriteria()
    {
        using var client = CreateClient(TestUsers.EditorA);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        var body = SaveBody(Fixture.A.Id, "Liste", criteria);
        body["criteria"]!.AsArray().RemoveAt(0);

        using var response = await client.PostAsJsonAsync(Path, body, Cancellation);

        await ShouldBeProblemAsync(response, ProblemTypes.Validation, "criteria");
    }

    [Fact]
    public async Task Save_ReleaseNoLongerActive_Returns400ReleaseMismatch()
    {
        using var client = CreateClient(TestUsers.EditorA);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        await ActivateAsync(MiniCatalog.T2Version);

        using var response = await client.PostAsJsonAsync(Path, SaveBody(Fixture.A.Id, "Liste", criteria), Cancellation);

        await ShouldBeProblemAsync(response, ProblemTypes.ReleaseMismatch, "selection.releaseVersion");
        (await CountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Save_ExcludedSelection_Returns400SelectionExcluded()
    {
        using var client = CreateClient(TestUsers.EditorA);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        var body = SaveBody(Fixture.A.Id, "Liste", criteria);
        // HR excludes Hardware in T.1 (MiniCatalog).
        body["selection"]!["appTypeIds"] = new JsonArray("web", "hardware");

        using var response = await client.PostAsJsonAsync(Path, body, Cancellation);

        await ShouldBeProblemAsync(response, ProblemTypes.SelectionExcluded, "selection.appTypeIds[1]");
    }

    [Fact(DisplayName = "INV:CAT-18 " + nameof(Save_InactiveUseCase_Returns400AtTheUseCase))]
    [Trait("Invariant", "CAT-18")]
    public async Task Save_InactiveUseCase_Returns400AtTheUseCase()
    {
        // The criteria endpoint and the export accept the inactive use case (CAT-18); a new list refuses it.
        var selection = new JsonObject
        {
            ["useCaseTemplateId"] = "databaseApplication",
            ["appTypeIds"] = new JsonArray("mobileOpen"),
            ["productPartIds"] = new JsonArray("base"),
            ["testTypeIds"] = new JsonArray(),
        };
        using var client = CreateClient(TestUsers.EditorA);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, selection, Cancellation);

        using var response = await client.PostAsJsonAsync(Path, SaveBody(Fixture.A.Id, "Liste", criteria, selection: selection), Cancellation);

        await ShouldBeProblemAsync(response, ProblemTypes.Validation, "selection.useCaseTemplateId");
        (await CountAsync()).ShouldBe(0);
    }

    [Fact(DisplayName = "INV:SEC-10 " + nameof(Save_UnconfirmedAddress_Returns403EmailUnconfirmedAndStoresNothing))]
    [Trait("Invariant", "SEC-10")]
    public async Task Save_UnconfirmedAddress_Returns403EmailUnconfirmedAndStoresNothing()
    {
        await StoreMembershipAsync(TestUsers.ReaderB, OrganizationRole.Editor);
        using var client = CreateClient(TestUsers.ReaderB);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);

        using var response = await client.PostAsJsonAsync(Path, SaveBody(Fixture.A.Id, "Liste", criteria), Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        (await TypeOf(response)).ShouldEndWith("/problems/" + ProblemTypes.EmailUnconfirmed);
        (await CountAsync()).ShouldBe(0);
    }

    [Fact(DisplayName = "INV:SEC-10 " + nameof(Save_AfterConfirmingTheAddress_Succeeds))]
    [Trait("Invariant", "SEC-10")]
    public async Task Save_AfterConfirmingTheAddress_Succeeds()
    {
        await StoreMembershipAsync(TestUsers.ReaderB, OrganizationRole.Editor);
        using var client = CreateClient(TestUsers.ReaderB);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        var body = SaveBody(Fixture.A.Id, "Liste", criteria);
        (await client.PostAsJsonAsync(Path, body, Cancellation)).StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        await TestAccounts.ConfirmAsync(Api.Factory.Services, TestUsers.ReaderB);
        using var response = await client.PostAsJsonAsync(Path, body, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact(DisplayName = "INV:TEN-01 " + nameof(Read_ListOfAnotherOrganization_Returns404))]
    [Trait("Invariant", "TEN-01")]
    public async Task Read_ListOfAnotherOrganization_Returns404()
    {
        var ofB = await SaveAsync(TestUsers.OwnerB, Fixture.B.Id, "Liste von B");
        using var client = CreateClient(TestUsers.OwnerA);
        // Naming B in a header changes nothing: the organisation comes from the stored list.
        client.DefaultRequestHeaders.Add("X-Organization-Id", Fixture.B.Id.ToString());

        using var response = await client.GetAsync($"{Path}/{ofB}", Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Read_UnknownOrMalformedId_Returns404Or400()
    {
        using var client = CreateClient(TestUsers.ReaderA);

        using var unknown = await client.GetAsync($"{Path}/{Guid.NewGuid()}", Cancellation);
        using var malformed = await client.GetAsync($"{Path}/keine-id", Cancellation);

        unknown.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        malformed.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "INV:TEN-01 " + nameof(List_Organization_ListsOnlyItsListsTheMostRecentlyChangedFirst))]
    [Trait("Invariant", "TEN-01")]
    public async Task List_Organization_ListsOnlyItsListsTheMostRecentlyChangedFirst()
    {
        var older = await SaveAsync(TestUsers.EditorA, Fixture.A.Id, "Ältere Liste");
        Clock.Advance(TimeSpan.FromHours(1));
        var newer = await SaveAsync(TestUsers.OwnerA, Fixture.A.Id, "Neuere Liste");
        await SaveAsync(TestUsers.OwnerB, Fixture.B.Id, "Liste von B");
        using var client = CreateClient(TestUsers.ReaderA);

        var lists = await client.GetFromJsonAsync<List<ConfigurationSummaryResponse>>($"{Path}?organizationId={Fixture.A.Id}", Cancellation);

        lists.ShouldNotBeNull().Select(l => (l.Id, l.Name)).ShouldBe([(newer, "Neuere Liste"), (older, "Ältere Liste")]);
        lists[0].ShouldSatisfyAllConditions(
            l => l.ReleaseVersion.ShouldBe(MiniCatalog.T1Version),
            l => l.CreatedAt.ShouldBe(Clock.GetUtcNow()),
            l => l.ModifiedAt.ShouldBe(Clock.GetUtcNow()));
    }

    [Fact]
    public async Task List_OrganizationWithoutLists_ReturnsAnEmptyList()
    {
        using var client = CreateClient(TestUsers.ReaderA);

        var lists = await client.GetFromJsonAsync<List<ConfigurationSummaryResponse>>($"{Path}?organizationId={Fixture.A.Id}", Cancellation);

        lists.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public async Task List_WithoutOrganization_Returns400AtOrganizationId()
    {
        using var client = CreateClient(TestUsers.ReaderA);

        using var response = await client.GetAsync(Path, Cancellation);

        await ShouldBeProblemAsync(response, ProblemTypes.Validation, "organizationId");
    }

    /// <summary>Saves the whole draft of <see cref="WebSelection"/> unchanged and returns the id.</summary>
    private async Task<Guid> SaveAsync(TestUser user, Guid organizationId, string name)
    {
        using var client = CreateClient(user);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        using var response = await client.PostAsJsonAsync(Path, SaveBody(organizationId, name, criteria), Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(Cancellation));
        return (await response.Content.ReadFromJsonAsync<CreatedConfiguration>(Cancellation)).ShouldNotBeNull().Id;
    }

    /// <summary>The body of a save: the export's draft body without its format, with organisation and name.</summary>
    private static JsonObject SaveBody(
        Guid organizationId, string? name, JsonArray criteria, Action<string, JsonObject>? adjust = null, JsonObject? selection = null)
    {
        var body = Body("xlsx", MiniCatalog.T1Version, selection ?? WebSelection, criteria, adjust);
        body.Remove("format");
        body["organizationId"] = organizationId;
        body["name"] = name;
        return body;
    }

    private static string KeyAt(JsonArray criteria, int index) => criteria[index]!["key"]!.GetValue<string>();

    private static int IndexOf(JsonArray criteria, string key) =>
        criteria.Select((c, i) => (Key: c!["key"]!.GetValue<string>(), Index: i)).Single(p => p.Key == key).Index;

    private static string FirstOfLevel(JsonArray criteria, string level) =>
        criteria.First(c => c!["resolvedPriority"]!["level"]!.GetValue<string>() == level)!["key"]!.GetValue<string>();

    private async Task ActivateAsync(string version)
    {
        var error = new StringWriter();
        var exitCode = await CatalogModule.RunCommandAsync(
            Api.Factory.Services, ["catalog", "activate", version], TextWriter.Null, error, Cancellation);
        exitCode.ShouldBe(0, error.ToString());
    }

    private async Task StoreMembershipAsync(TestUser user, OrganizationRole role) =>
        await StoreAsync(OrganizationMembership.Create(Fixture.A.Id, user.Id, role, Clock.GetUtcNow(), Clock.GetUtcNow()));

    private async Task<int> CountAsync()
    {
        await using var db = Configurator(Fixture.A.Id, Fixture.B.Id);
        return await db.CriteriaConfigurations.CountAsync(Cancellation);
    }

    /// <summary>A context of its own outside any request that sees <paramref name="organizationIds"/>.</summary>
    private ConfiguratorDbContext Configurator(params Guid[] organizationIds) =>
        new(new DbContextOptionsBuilder<ConfiguratorDbContext>().UseNpgsql(Api.ConnectionString).Options,
            FixedOrganizationContext.Of(organizationIds));

    private static async Task ShouldBeProblemAsync(HttpResponseMessage response, string type, params string[] errorKeys)
    {
        var text = await response.Content.ReadAsStringAsync(Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest, text);
        using var body = JsonDocument.Parse(text);
        body.RootElement.GetProperty("type").GetString().ShouldNotBeNull().ShouldEndWith("/problems/" + type);
        body.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name)
            .ShouldBe(errorKeys, ignoreOrder: true);
    }

    private static async Task<string> TypeOf(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        return body.RootElement.GetProperty("type").GetString()!;
    }
}
