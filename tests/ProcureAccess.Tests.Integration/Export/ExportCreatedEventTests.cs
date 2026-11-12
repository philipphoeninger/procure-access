// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using ProcureAccess.Modules.Platform.Analytics;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.Auth;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using Xunit;
using static ProcureAccess.Tests.Integration.Export.ExportTestCatalog;

namespace ProcureAccess.Tests.Integration.Export;

/// <summary>
/// The usage event <c>ExportCreated</c> of <c>POST /api/v1/exports</c> (SPEC §9.1, R-MET-01;
/// S4-21a): one event per created file, with its format and whether the request is signed in, and
/// none for a refused request.
/// </summary>
public sealed class ExportCreatedEventTests(ApiFixture api) : IntegrationTest(api), IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await Api.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public static TheoryData<string, bool, string, string> Exports => new()
    {
        { "xlsx", false, UsageEventWriter.Dimensions.Xlsx, UsageEventWriter.Dimensions.Anonymous },
        { "csv", true, UsageEventWriter.Dimensions.Csv, UsageEventWriter.Dimensions.SignedIn },
    };

    [Theory]
    [MemberData(nameof(Exports))]
    public async Task Export_FileCreated_StoresOneExportCreatedWithFormatAndAudience(
        string format, bool signedIn, string expectedFormat, string expectedAudience)
    {
        await ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
        using var client = CreateClient(signedIn ? TestUsers.Member : null);
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        await UsageEventRows.DeleteAllAsync(Api);

        using var response = await client.PostAsJsonAsync(
            ExportPath, Body(format, MiniCatalog.T1Version, WebSelection, criteria), Cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var stored = (await UsageEventRows.WaitForAsync(Api, 1)).ShouldHaveSingleItem();
        stored.EventType.ShouldBe(UsageEventType.ExportCreated);
        stored.OccurredOn.ShouldBe(DateOnly.FromDateTime(Clock.GetUtcNow().UtcDateTime));
        stored.Dimension1.ShouldBe(expectedFormat);
        stored.Dimension2.ShouldBe(expectedAudience);
    }

    [Fact]
    public async Task Export_Refused_StoresNoEvent()
    {
        await ImportBothAndActivateAsync(Api.Factory.Services, Clock, MiniCatalog.T1Version, Cancellation);
        using var client = CreateClient();
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, WebSelection, Cancellation);
        await UsageEventRows.DeleteAllAsync(Api);

        using var refused = await client.PostAsJsonAsync(
            ExportPath,
            Body("xlsx", MiniCatalog.T1Version, WebSelection, criteria, (_, entry) => entry["note"] = new string('x', 1001)),
            Cancellation);
        // A created file afterwards: the queue is in order, so once its event is stored, an event
        // of the refused request would be stored too.
        using var created = await client.PostAsJsonAsync(
            ExportPath, Body("csv", MiniCatalog.T1Version, WebSelection, criteria), Cancellation);

        refused.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        created.StatusCode.ShouldBe(HttpStatusCode.OK);
        var stored = (await UsageEventRows.WaitForAsync(Api, 1)).ShouldHaveSingleItem();
        stored.Dimension1.ShouldBe(UsageEventWriter.Dimensions.Csv);
    }

    private static CancellationToken Cancellation => TestContext.Current.CancellationToken;
}
