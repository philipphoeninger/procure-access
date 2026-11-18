// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using ProcureAccess.Modules.Configurator.Configurations;
using ProcureAccess.Modules.Workspace.Projects;
using ProcureAccess.Tests.Shared.Fixtures;
using Shouldly;
using static ProcureAccess.Tests.Integration.Export.ExportTestCatalog;

namespace ProcureAccess.Tests.Integration.Workspace;

/// <summary>
/// A project with its own copy of a list, built through the API as S12 and S32 do, for the tests
/// of what depends on that list (the assessments, S6-18). Needs the test releases with <c>T.1</c>
/// active.
/// </summary>
internal static class ProjectLists
{
    /// <summary>
    /// A selection of <c>T.1</c> for two app types whose procedures differ: Web is tested by the
    /// Software-Test, Mobile by the App-Test too, so a criterion offers the levels of both (C31).
    /// </summary>
    public static readonly JsonObject WebAndMobileSelection = new()
    {
        ["useCaseTemplateId"] = null,
        ["appTypeIds"] = new JsonArray("web", "mobileOpen"),
        ["productPartIds"] = new JsonArray("base"),
        ["testTypeIds"] = new JsonArray("softwareTest"),
    };

    /// <summary>
    /// Saves a list of <paramref name="selection"/> under <c>T.1</c> for the organisation; every
    /// criterion is included unless <paramref name="adjust"/> changes its entry (by criterion key).
    /// Returns its id and the criteria as the catalog answers them.
    /// </summary>
    public static async Task<(Guid Id, JsonArray Criteria)> SaveAsync(
        HttpClient client,
        Guid organizationId,
        JsonObject selection,
        CancellationToken cancellationToken,
        Action<string, JsonObject>? adjust = null)
    {
        var criteria = await CriteriaAsync(client, MiniCatalog.T1Version, selection, cancellationToken);
        var body = Body("xlsx", MiniCatalog.T1Version, selection, criteria, adjust);
        body.Remove("format");
        body["organizationId"] = organizationId;
        body["name"] = "Liste";
        using var response = await client.PostAsJsonAsync("/api/v1/configurations", body, cancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(cancellationToken));
        return ((await response.Content.ReadFromJsonAsync<CreatedConfiguration>(cancellationToken)).ShouldNotBeNull().Id, criteria);
    }

    /// <summary>Gives the project its copy of the list <paramref name="sourceId"/>.</summary>
    public static async Task<ProjectCriteriaListResponse> AssignAsync(
        HttpClient client, Guid projectId, Guid sourceId, CancellationToken cancellationToken)
    {
        using var response = await client.PutAsJsonAsync(
            $"/api/v1/projects/{projectId}/criteria-configuration", new { sourceConfigurationId = sourceId }, cancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(cancellationToken));
        return (await response.Content.ReadFromJsonAsync<ProjectCriteriaListResponse>(cancellationToken)).ShouldNotBeNull();
    }

    /// <summary>The revision of the criterion <paramref name="key"/> in a catalog answer.</summary>
    public static int RevisionOf(JsonArray criteria, string key) =>
        criteria.Single(c => c!["key"]!.GetValue<string>() == key)!["revisionId"]!.GetValue<int>();

    /// <summary>The keys of the requirement criteria of a catalog answer, in list order.</summary>
    public static List<string> RequirementKeysOf(JsonArray criteria) =>
        [.. criteria.Where(c => c!["kind"]!.GetValue<string>() == "requirement").Select(c => c!["key"]!.GetValue<string>())];

    public static string AssessmentsPath(Guid candidateId) => $"/api/v1/candidates/{candidateId}/assessments";

    /// <summary>Sets the assessment of a criterion as the autosave of S34b does.</summary>
    public static Task<HttpResponseMessage> SetAssessmentAsync(
        HttpClient client, Guid candidateId, int revision, string? value, CancellationToken cancellationToken, string? note = null) =>
        client.PutAsJsonAsync($"{AssessmentsPath(candidateId)}/{revision}", new { value, note }, cancellationToken);
}
