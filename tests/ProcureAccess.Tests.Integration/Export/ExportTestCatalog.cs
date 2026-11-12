// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using ProcureAccess.Modules.Catalog;
using ProcureAccess.Modules.Catalog.Persistence;
using ProcureAccess.Modules.Catalog.Releases;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;

namespace ProcureAccess.Tests.Integration.Export;

/// <summary>
/// The test releases <c>T.1</c> and <c>T.2</c> on an API host, and export request bodies built from
/// the criteria the host answers, as the client builds them from S11 (ARCHITECTURE §7.2).
/// </summary>
internal static class ExportTestCatalog
{
    public const string ExportPath = "/api/v1/exports";

    /// <summary>A selection of <c>T.1</c> with a use case, a procedure and a „Geprüft nach …“ criterion.</summary>
    public static readonly JsonObject WebSelection = new()
    {
        ["useCaseTemplateId"] = "hr",
        ["appTypeIds"] = new JsonArray("web"),
        ["productPartIds"] = new JsonArray("base"),
        ["testTypeIds"] = new JsonArray("softwareTest"),
    };

    private static readonly string TestReleasesPath =
        Path.GetDirectoryName(TestReleaseFolders.PathOf(MiniCatalog.T1Version))!;

    /// <summary>Imports <c>T.1</c> and <c>T.2</c> and activates <paramref name="activeVersion"/> through the command.</summary>
    public static async Task ImportBothAndActivateAsync(
        IServiceProvider services, FakeTimeProvider clock, string activeVersion, CancellationToken cancellationToken)
    {
        foreach (var version in new[] { MiniCatalog.T1Version, MiniCatalog.T2Version })
        {
            await using var scope = services.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var result = await new CatalogReleaseImporter(db, clock).ImportAsync(TestReleasesPath, version, cancellationToken);
            result.Errors.ShouldBeEmpty();
            clock.Advance(TimeSpan.FromMinutes(1));
        }

        var error = new StringWriter();
        var exitCode = await CatalogModule.RunCommandAsync(
            services, ["catalog", "activate", activeVersion], TextWriter.Null, error, cancellationToken);
        exitCode.ShouldBe(0, error.ToString());
    }

    /// <summary>The criteria answer of <paramref name="selection"/> under <paramref name="release"/>.</summary>
    public static async Task<JsonArray> CriteriaAsync(
        HttpClient client, string release, JsonObject selection, CancellationToken cancellationToken)
    {
        var query = new List<string> { $"release={release}" };
        if (selection["useCaseTemplateId"]?.GetValue<string>() is { } useCase)
        {
            query.Add($"useCase={useCase}");
        }

        foreach (var (member, parameter) in new[] { ("appTypeIds", "appTypeIds[]"), ("productPartIds", "productPartIds[]"), ("testTypeIds", "testTypeIds[]") })
        {
            query.AddRange((selection[member]?.AsArray() ?? []).Select(v => $"{parameter}={v!.GetValue<string>()}"));
        }

        using var response = await client.GetAsync($"/api/v1/catalog/criteria?{string.Join('&', query)}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonObject>(cancellationToken);
        return json!["criteria"]!.AsArray();
    }

    /// <summary>
    /// A request body that includes every criterion of <paramref name="criteria"/> without a change,
    /// then lets <paramref name="adjust"/> change single entries (by criterion key).
    /// </summary>
    public static JsonObject Body(
        string format,
        string release,
        JsonObject selection,
        JsonArray criteria,
        Action<string, JsonObject>? adjust = null)
    {
        var entries = new JsonArray();
        foreach (var criterion in criteria)
        {
            var entry = new JsonObject
            {
                ["criterionRevisionId"] = criterion!["revisionId"]!.GetValue<int>(),
                ["isIncluded"] = true,
            };
            adjust?.Invoke(criterion["key"]!.GetValue<string>(), entry);
            entries.Add(entry);
        }

        var withRelease = (JsonObject)selection.DeepClone();
        withRelease["releaseVersion"] = release;
        return new JsonObject { ["format"] = format, ["selection"] = withRelease, ["criteria"] = entries };
    }
}
