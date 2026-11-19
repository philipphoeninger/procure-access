// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Configurations;
using ProcureAccess.Modules.Configurator.Contracts.Drafts;
using ProcureAccess.Modules.Configurator.Drafts;
using ProcureAccess.Modules.Configurator.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Concurrency;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Integration.Export.ExportTestCatalog;
using Keys = ProcureAccess.Tests.Shared.Fixtures.MiniCatalog.Keys;

namespace ProcureAccess.Tests.Integration.Configurator;

/// <summary>
/// Optimistic locking of the saved lists (S6-20b; SPEC R-SAV-03, R-PRJ-07, R-CAN-11; ARCHITECTURE
/// §7.1; API-03, API-04): a list answers with an <c>ETag</c> of the 🔧 epoch and its version; a
/// decision, a selection change and a deletion without <c>If-Match</c> are refused (428), with a
/// stale tag they answer 409 <c>conflict</c> with the competing editor's display name — also when
/// the other change lands while this one is saved (the 500 of S5-21a R1). Every test starts from
/// the Tenants fixture, the test releases with <c>T.1</c> active and one list of A saved by its
/// Owner. The project's copy is changed through the same endpoints
/// (<c>ProjectCriteriaListTests</c>).
/// </summary>
public sealed class SavedListConcurrencyTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Path = "/api/v1/configurations";
    private const string FirstTag = IfMatchRequests.FirstTag;

    /// <summary>HR with web, base and audio, no test type: the software-test criterion of the saved list leaves.</summary>
    private static readonly JsonObject HrAudioSelection = new()
    {
        ["useCaseTemplateId"] = "hr",
        ["appTypeIds"] = new JsonArray("web"),
        ["productPartIds"] = new JsonArray("base", "audio"),
        ["testTypeIds"] = new JsonArray(),
        ["releaseVersion"] = MiniCatalog.T1Version,
    };

    private Tenants? _tenants;
    private Guid _id;
    private JsonArray _criteria = [];

    private Tenants Fixture => _tenants ?? throw new InvalidOperationException("The fixture is not stored.");

    private string ListPath => $"{Path}/{_id}";

    private string SelectionPath => $"{ListPath}/selection";

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    private static object Selection => new JsonObject { ["selection"] = HrAudioSelection.DeepClone() };

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();
        await ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
        _tenants = Tenants.Build(Clock.GetUtcNow());
        // The accounts first: organisations and memberships reference them (foreign keys, S6-10b).
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => TestAccounts.Of(u, confirmed: true))]);
        await StoreAsync(_tenants.Entities);

        using var owner = CreateClient(TestUsers.OwnerA);
        _criteria = await CriteriaAsync(owner, MiniCatalog.T1Version, WebSelection, Cancellation);
        var body = Body("xlsx", MiniCatalog.T1Version, WebSelection, _criteria);
        body.Remove("format");
        body["organizationId"] = _tenants.A.Id;
        body["name"] = "Liste";
        using var saved = await owner.PostAsJsonAsync(Path, body, Cancellation);
        saved.StatusCode.ShouldBe(HttpStatusCode.Created, await saved.Content.ReadAsStringAsync(Cancellation));
        _id = (await saved.Content.ReadFromJsonAsync<CreatedConfiguration>(Cancellation)).ShouldNotBeNull().Id;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact(DisplayName = "INV:API-04 " + nameof(Read_ListAndItsEntryOnS14_CarryTheEpochAndTheVersion))]
    [Trait("Invariant", "API-04")]
    public async Task Read_ListAndItsEntryOnS14_CarryTheEpochAndTheVersion()
    {
        using var client = CreateClient(TestUsers.ReaderA);

        using var list = await client.GetAsync(ListPath, Cancellation);
        var lists = await client.GetFromJsonAsync<JsonElement>($"{Path}?organizationId={Fixture.A.Id}", Cancellation);
        using var otherEpoch = await CreateClient(TestUsers.EditorA).SendWithTagAsync(HttpMethod.Delete, ListPath, "\"2-1\"", Cancellation);

        list.Headers.ETag.ShouldNotBeNull().ToString().ShouldBe(FirstTag);
        (await list.Content.ReadAsStringAsync(Cancellation)).ShouldNotContain("\"etag\"");
        lists[0].GetProperty("etag").GetString().ShouldBe(FirstTag);
        // The stored version under another epoch is no tag of the list.
        await ShouldBeConflictAsync(otherEpoch, TestUsers.OwnerA.Email);
    }

    [Theory(DisplayName = "INV:API-03 " + nameof(Change_WithoutIfMatch_Returns428AfterTheRoleCheckAndChangesNothing))]
    [Trait("Invariant", "API-03")]
    [MemberData(nameof(Changes))]
    public async Task Change_WithoutIfMatch_Returns428AfterTheRoleCheckAndChangesNothing(Change change)
    {
        using var editor = CreateClient(TestUsers.EditorA);
        using var reader = CreateClient(TestUsers.ReaderA);
        using var ofB = CreateClient(TestUsers.EditorB);
        var before = await ReadRawAsync(editor);

        using var missing = await SendAsync(editor, change, ifMatch: null);
        using var blank = await SendAsync(editor, change, ifMatch: " ");
        using var forbidden = await SendAsync(reader, change, ifMatch: null);
        using var notFound = await SendAsync(ofB, change, ifMatch: null);

        foreach (var response in new[] { missing, blank })
        {
            var text = await response.Content.ReadAsStringAsync(Cancellation);
            response.StatusCode.ShouldBe(HttpStatusCode.PreconditionRequired, text);
            using var body = JsonDocument.Parse(text);
            body.RootElement.GetProperty("type").GetString().ShouldNotBeNull().ShouldEndWith("/problems/" + ProblemTypes.PreconditionRequired);
        }

        // The answer says nothing about a list its sender may not change (TEN-01, TEN-05).
        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        notFound.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        (await ReadRawAsync(editor)).ShouldBe(before);
    }

    [Theory(DisplayName = "INV:API-03 " + nameof(Change_StaleTag_Returns409WithTheCompetingEditorAndChangesNothing))]
    [Trait("Invariant", "API-03")]
    [MemberData(nameof(Changes))]
    public async Task Change_StaleTag_Returns409WithTheCompetingEditorAndChangesNothing(Change change)
    {
        using var owner = CreateClient(TestUsers.OwnerA);
        using var editor = CreateClient(TestUsers.EditorA);
        // A colleague decides on another criterion of the list the editor has read at version 1.
        using var first = await owner.SendWithTagAsync(
            HttpMethod.Patch, CriterionPath(Keys.PriorityAllLevels), new { isIncluded = true, note = "Von der Kollegin" }, FirstTag, Cancellation);
        first.StatusCode.ShouldBe(HttpStatusCode.NoContent, await first.Content.ReadAsStringAsync(Cancellation));
        var stored = await ReadRawAsync(editor);

        using var response = await SendAsync(editor, change, FirstTag);

        await ShouldBeConflictAsync(response, TestUsers.OwnerA.Email);
        (await ReadRawAsync(editor)).ShouldBe(stored);
    }

    [Theory(DisplayName = "INV:API-03 " + nameof(Decide_WhatIsNoTagOfTheList_Returns409))]
    [Trait("Invariant", "API-03")]
    [InlineData("*")]
    [InlineData("W/\"1-1\"")]
    [InlineData("\"1-1\", \"1-2\"")]
    [InlineData("\"1-7\"")]
    public async Task Decide_WhatIsNoTagOfTheList_Returns409(string ifMatch)
    {
        using var client = CreateClient(TestUsers.EditorA);

        using var response = await SendAsync(client, Change.Decide, ifMatch);

        await ShouldBeConflictAsync(response, TestUsers.OwnerA.Email);
    }

    [Fact(DisplayName = "INV:API-03 " + nameof(Change_CurrentTag_AnswersWithTheNewTagAndStoresTheEditor))]
    [Trait("Invariant", "API-03")]
    public async Task Change_CurrentTag_AnswersWithTheNewTagAndStoresTheEditor()
    {
        using var client = CreateClient(TestUsers.EditorA);

        using var decided = await SendAsync(client, Change.Decide, FirstTag);
        // The same decision again still counts: every stored decision is a version of the list.
        using var again = await SendAsync(client, Change.Decide, "\"1-2\"");
        using var selection = await SendAsync(client, Change.ChangeSelection, "\"1-3\"");
        var read = await client.TagOfAsync(ListPath, Cancellation);
        using var stale = await SendAsync(client, Change.Delete, "\"1-3\"");
        using var deleted = await SendAsync(client, Change.Delete, "\"1-4\"");

        decided.StatusCode.ShouldBe(HttpStatusCode.NoContent, await decided.Content.ReadAsStringAsync(Cancellation));
        decided.Headers.ETag.ShouldNotBeNull().ToString().ShouldBe("\"1-2\"");
        again.Headers.ETag.ShouldNotBeNull().ToString().ShouldBe("\"1-3\"");
        selection.StatusCode.ShouldBe(HttpStatusCode.OK, await selection.Content.ReadAsStringAsync(Cancellation));
        selection.Headers.ETag.ShouldNotBeNull().ToString().ShouldBe("\"1-4\"");
        read.ShouldBe("\"1-4\"");
        await ShouldBeConflictAsync(stale, TestUsers.EditorA.Email);
        deleted.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "INV:API-03 " + nameof(Decide_RacingASelectionChangeThatRemovesTheCriterion_Returns409InsteadOf500))]
    [Trait("Invariant", "API-03")]
    public async Task Decide_RacingASelectionChangeThatRemovesTheCriterion_Returns409InsteadOf500()
    {
        // S5-21a R1: the decision has loaded the list and passed its If-Match when the colleague's
        // selection change, which removes the criterion, is stored; then the decision is saved.
        using var owner = CreateClient(TestUsers.OwnerA);
        var revision = RevisionOf(Keys.ProcedureSoftwareTest);
        var result = await DecideWhileAsync(revision, async () =>
        {
            using var changed = await owner.SendWithTagAsync(HttpMethod.Put, SelectionPath, Selection, FirstTag, Cancellation);
            changed.StatusCode.ShouldBe(HttpStatusCode.OK, await changed.Content.ReadAsStringAsync(Cancellation));
        });

        result.Outcome.ShouldBe(EditOutcome.Conflict);
        result.EditorUserId.ShouldBe(TestUsers.OwnerA.Id);
        using var client = CreateClient(TestUsers.EditorA);
        var list = (await client.GetFromJsonAsync<ConfigurationResponse>(ListPath, Cancellation)).ShouldNotBeNull();
        list.Criteria.ShouldNotContain(c => c.CriterionRevisionId == revision);
        (await client.TagOfAsync(ListPath, Cancellation)).ShouldBe("\"1-2\"");
    }

    [Fact(DisplayName = "INV:API-03 " + nameof(Decide_RacingADeletionOfTheList_Returns404))]
    [Trait("Invariant", "API-03")]
    public async Task Decide_RacingADeletionOfTheList_Returns404()
    {
        using var owner = CreateClient(TestUsers.OwnerA);

        var result = await DecideWhileAsync(RevisionOf(Keys.DefaultOnly), async () =>
        {
            using var deleted = await owner.SendWithTagAsync(HttpMethod.Delete, ListPath, FirstTag, Cancellation);
            deleted.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        });

        result.Outcome.ShouldBe(EditOutcome.NotFound);
    }

    public enum Change
    {
        Decide,
        ChangeSelection,
        Delete,
    }

    public static TheoryData<Change> Changes => [.. Enum.GetValues<Change>()];

    private Task<HttpResponseMessage> SendAsync(HttpClient client, Change change, string? ifMatch) => change switch
    {
        Change.Decide => client.SendWithTagAsync(
            HttpMethod.Patch, CriterionPath(Keys.DefaultOnly), new { isIncluded = false }, ifMatch, Cancellation),
        Change.ChangeSelection => client.SendWithTagAsync(HttpMethod.Put, SelectionPath, Selection, ifMatch, Cancellation),
        Change.Delete => client.SendWithTagAsync(HttpMethod.Delete, ListPath, ifMatch, Cancellation),
        _ => throw new ArgumentOutOfRangeException(nameof(change)),
    };

    /// <summary>
    /// A decision of Editor A on <paramref name="revision"/> with the first tag, as the endpoint asks
    /// the service for it, during whose save — after the load and the <c>If-Match</c> comparison —
    /// <paramref name="meanwhile"/> happens once.
    /// </summary>
    private async Task<EditResult<bool>> DecideWhileAsync(int revision, Func<Task> meanwhile)
    {
        await using var scope = Api.Factory.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        await using var db = new ConfiguratorDbContext(
            new DbContextOptionsBuilder<ConfiguratorDbContext>()
                .UseNpgsql(Api.ConnectionString)
                .AddInterceptors(new BeforeFirstSave(meanwhile))
                .Options,
            FixedOrganizationContext.Of(Fixture.A.Id));
        var service = new ConfigurationService(
            db,
            services.GetRequiredService<IConfigurationDraftCheck>(),
            services.GetRequiredService<ICatalogSelectionResolver>(),
            services.GetRequiredService<ICatalogReleaseCarryOver>(),
            services.GetRequiredService<PriorityWeightRange>(),
            services.GetRequiredService<EntityTags>(),
            Clock);
        return await service.DecideAsync(
            _id, revision, new ConfigurationDecisionBody(false, null, null, null), new ListEdit(TestUsers.EditorA.Id, FirstTag), Cancellation);
    }

    private string CriterionPath(string key) => $"{ListPath}/criteria/{RevisionOf(key)}";

    private int RevisionOf(string key) =>
        _criteria.Single(c => c!["key"]!.GetValue<string>() == key)!["revisionId"]!.GetValue<int>();

    private async Task<string> ReadRawAsync(HttpClient client)
    {
        using var response = await client.GetAsync(ListPath, Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return $"{response.Headers.ETag} {await response.Content.ReadAsStringAsync(Cancellation)}";
    }

    /// <summary>409 <c>conflict</c> with <c>editorDisplayName</c>.</summary>
    private static async Task ShouldBeConflictAsync(HttpResponseMessage response, string? editor)
    {
        var text = await response.Content.ReadAsStringAsync(Cancellation);
        response.StatusCode.ShouldBe(HttpStatusCode.Conflict, text);
        using var body = JsonDocument.Parse(text);
        body.RootElement.GetProperty("type").GetString().ShouldNotBeNull().ShouldEndWith("/problems/" + ProblemTypes.Conflict);
        body.RootElement.GetProperty("editorDisplayName").GetString().ShouldBe(editor);
        response.Headers.ETag.ShouldBeNull();
    }

    /// <summary>Runs an action once, before the first save of its context reaches the database.</summary>
    private sealed class BeforeFirstSave(Func<Task> action) : SaveChangesInterceptor
    {
        private bool _done;

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!_done)
            {
                _done = true;
                await action();
            }

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }
    }
}
