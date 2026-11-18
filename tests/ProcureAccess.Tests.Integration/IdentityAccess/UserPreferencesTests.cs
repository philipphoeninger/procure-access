// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.IdentityAccess;

/// <summary>
/// The signed-in user's settings <c>GET</c> / <c>PUT /api/v1/account/preferences</c> (S5-25a;
/// SPEC R-SET-01, R-SET-02; ARCHITECTURE §5.2, C73). The client applies them in S5-25b, which
/// activates A11Y-06; here: stored per user, the defaults without a row, the checks of the PUT
/// (owner decision S5-25 3: no high contrast before H2), sign-in and antiforgery.
/// </summary>
public sealed class UserPreferencesTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    private const string Preferences = "/api/v1/account/preferences";

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await Api.ResetDatabaseAsync();

        // The settings belong to an account (foreign key, S6-10b).
        await StoreAsync([.. TestUsers.ByName.Values.Select(u => TestAccounts.Of(u, confirmed: true))]);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task Get_UserWhoNeverSaved_ReturnsTheDefaultsNotStored()
    {
        using var client = CreateClient(TestUsers.Member);

        using var response = await client.GetAsync(Preferences, Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Headers.CacheControl.ShouldNotBeNull().NoStore.ShouldBeTrue();
        (await ValuesOf(response)).ShouldBe(new UserPreferencesValues("de", "system", "system", "system"));
    }

    [Fact]
    public async Task Put_ValidSettings_AreStoredAndReturnedByTheNextGet()
    {
        using var client = CreateClient(TestUsers.Member);

        using var put = await client.PutAsync(Preferences, Body("de", "dark", "reduced", "system"), Cancellation);

        put.StatusCode.ShouldBe(HttpStatusCode.OK);
        var expected = new UserPreferencesValues("de", "dark", "reduced", "system");
        (await ValuesOf(put)).ShouldBe(expected);
        using var get = await CreateClient(TestUsers.Member).GetAsync(Preferences, Cancellation);
        (await ValuesOf(get)).ShouldBe(expected);
    }

    [Fact]
    public async Task Put_SecondTime_ReplacesEverySetting()
    {
        using var client = CreateClient(TestUsers.Member);
        using var first = await client.PutAsync(Preferences, Body("de", "dark", "reduced", "system"), Cancellation);
        first.StatusCode.ShouldBe(HttpStatusCode.OK);

        using var second = await client.PutAsync(Preferences, Body("de", "light", "system", "system"), Cancellation);

        second.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var get = await client.GetAsync(Preferences, Cancellation);
        (await ValuesOf(get)).ShouldBe(new UserPreferencesValues("de", "light", "system", "system"));
    }

    [Fact]
    public async Task Put_OneUser_LeavesTheSettingsOfOthersUnchanged()
    {
        var other = UserPreferences.Defaults(TestUsers.OwnerA.Id);
        other.Change(UserPreferences.German, AppearancePreference.Light, MotionPreference.System, ContrastPreference.System);
        await StoreAsync(other);
        using var client = CreateClient(TestUsers.Member);

        using var put = await client.PutAsync(Preferences, Body("de", "dark", "reduced", "system"), Cancellation);

        put.StatusCode.ShouldBe(HttpStatusCode.OK);
        using var ownerA = await CreateClient(TestUsers.OwnerA).GetAsync(Preferences, Cancellation);
        (await ValuesOf(ownerA)).ShouldBe(new UserPreferencesValues("de", "light", "system", "system"));
        using var editorA = await CreateClient(TestUsers.EditorA).GetAsync(Preferences, Cancellation);
        (await ValuesOf(editorA)).ShouldBe(new UserPreferencesValues("de", "system", "system", "system"));
    }

    [Fact]
    public async Task Put_ConcurrentFirstSaves_StoreOneRowWithOneOfTheValues()
    {
        var light = new UserPreferencesValues("de", "light", "system", "system");
        var dark = new UserPreferencesValues("de", "dark", "reduced", "system");
        using var first = CreateClient(TestUsers.Member);
        using var second = CreateClient(TestUsers.Member);

        var responses = await Task.WhenAll(
            first.PutAsync(Preferences, Body(light.Language, light.Appearance, light.Motion, light.Contrast), Cancellation),
            second.PutAsync(Preferences, Body(dark.Language, dark.Appearance, dark.Motion, dark.Contrast), Cancellation));

        responses.ShouldAllBe(r => r.StatusCode == HttpStatusCode.OK);
        using var get = await first.GetAsync(Preferences, Cancellation);
        (await ValuesOf(get)).ShouldBeOneOf(light, dark);
        foreach (var response in responses)
        {
            response.Dispose();
        }
    }

    [Theory]
    [InlineData("""{"language":"en","appearance":"system","motion":"system","contrast":"system"}""", "language")]
    [InlineData("""{"language":"de","appearance":"Dark","motion":"system","contrast":"system"}""", "appearance")]
    [InlineData("""{"language":"de","appearance":"system","motion":"none","contrast":"system"}""", "motion")]
    [InlineData("""{"language":"de","appearance":"system","motion":"system","contrast":"more"}""", "contrast")]
    [InlineData("""{"language":"de","appearance":"system","motion":"system","contrast":"high"}""", "contrast")]
    [InlineData("""{"language":"de","appearance":"system","motion":"system"}""", "contrast")]
    [InlineData("""{"language":"de","appearance":5,"motion":"system","contrast":"system"}""", "appearance")]
    public async Task Put_InvalidSetting_Returns400ValidationAtTheFieldAndStoresNothing(string json, string field)
    {
        using var client = CreateClient(TestUsers.Member);

        using var response = await client.PutAsync(Preferences, new StringContent(json, Encoding.UTF8, "application/json"), Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        body.RootElement.GetProperty("type").GetString().ShouldEndWith("/problems/" + ProblemTypes.Validation);
        body.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name).ShouldBe([field]);
        using var get = await client.GetAsync(Preferences, Cancellation);
        (await ValuesOf(get)).ShouldBe(new UserPreferencesValues("de", "system", "system", "system"));
    }

    [Fact]
    public async Task Put_EmptyObject_Returns400ValidationAtEveryField()
    {
        using var client = CreateClient(TestUsers.Member);

        using var response = await client.PutAsync(Preferences, new StringContent("{}", Encoding.UTF8, "application/json"), Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        body.RootElement.GetProperty("errors").EnumerateObject().Select(p => p.Name).Order(StringComparer.Ordinal)
            .ShouldBe(["appearance", "contrast", "language", "motion"]);
    }

    [Fact]
    public async Task GetAndPut_Anonymous_Return401()
    {
        using var client = CreateClient();

        using var get = await client.GetAsync(Preferences, Cancellation);
        using var put = await client.PutAsync(Preferences, Body("de", "dark", "system", "system"), Cancellation);

        get.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        put.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Put_WithoutAntiforgeryToken_Returns400AntiforgeryAndStoresNothing()
    {
        using var client = Api.Factory.CreateDefaultClient(BaseAddress);
        client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, TestUsers.Member.Name);

        using var response = await client.PutAsync(Preferences, Body("de", "dark", "system", "system"), Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        body.RootElement.GetProperty("type").GetString().ShouldEndWith("/problems/" + ProblemTypes.Antiforgery);
        using var get = await CreateClient(TestUsers.Member).GetAsync(Preferences, Cancellation);
        (await ValuesOf(get)).ShouldBe(new UserPreferencesValues("de", "system", "system", "system"));
    }

    private static JsonContent Body(string language, string appearance, string motion, string contrast) =>
        JsonContent.Create(new { language, appearance, motion, contrast });

    private static async Task<UserPreferencesValues> ValuesOf(HttpResponseMessage response)
    {
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(Cancellation));
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Cancellation));
        var root = body.RootElement;
        root.EnumerateObject().Select(p => p.Name).ShouldBe(["language", "appearance", "motion", "contrast"]);
        return new UserPreferencesValues(
            root.GetProperty("language").GetString()!,
            root.GetProperty("appearance").GetString()!,
            root.GetProperty("motion").GetString()!,
            root.GetProperty("contrast").GetString()!);
    }
}
