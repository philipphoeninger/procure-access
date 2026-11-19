// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net.Http.Json;

namespace ProcureAccess.Tests.Integration.Infrastructure;

/// <summary>
/// Changes of shared resources with their <c>If-Match</c> (API-03; S6-20), for the tests that are
/// not about the precondition: they send the tag the resource has now, as a client does that has
/// just read it.
/// </summary>
internal static class IfMatchRequests
{
    /// <summary>
    /// The tag of a row no request has changed since it was stored: version 1 under the epoch of
    /// <c>appsettings.json</c>. For an evidence, which has no <c>GET</c> of its own.
    /// </summary>
    public const string FirstTag = "\"1-1\"";

    /// <summary>A well-formed tag no resource has; for requests that are refused before the tag counts.</summary>
    private const string NoTag = "\"1-0\"";

    /// <summary>
    /// The <c>ETag</c> of <c>GET <paramref name="resourcePath"/></c>, or a tag that matches nothing
    /// when the client may not read the resource (401, 404) — the change is refused for the same
    /// reason before its tag is compared.
    /// </summary>
    public static async Task<string> TagOfAsync(this HttpClient client, string resourcePath, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(resourcePath, cancellationToken);
        return response.Headers.ETag?.ToString() ?? NoTag;
    }

    /// <summary><c>PATCH</c> of a project or a candidate with its current tag.</summary>
    public static async Task<HttpResponseMessage> PatchCurrentAsync<T>(
        this HttpClient client, string path, T body, CancellationToken cancellationToken) =>
        await client.SendWithTagAsync(HttpMethod.Patch, path, body, await client.TagOfAsync(path, cancellationToken), cancellationToken);

    /// <summary>
    /// <c>PUT</c> below a project or a candidate (<c>…/criteria-configuration</c>,
    /// <c>…/test-status</c>) with the current tag of that project or candidate.
    /// </summary>
    public static async Task<HttpResponseMessage> PutBelowCurrentAsync<T>(
        this HttpClient client, string path, T body, CancellationToken cancellationToken)
    {
        var parent = path[..path.LastIndexOf('/')];
        return await client.SendWithTagAsync(HttpMethod.Put, path, body, await client.TagOfAsync(parent, cancellationToken), cancellationToken);
    }

    /// <summary><c>PATCH …/configurations/{id}/criteria/{revision}</c> with the current tag of the list (S6-20b).</summary>
    public static async Task<HttpResponseMessage> PatchListAsync<T>(
        this HttpClient client, string path, T body, CancellationToken cancellationToken) =>
        await client.SendWithTagAsync(HttpMethod.Patch, path, body, await client.TagOfListAsync(path, cancellationToken), cancellationToken);

    /// <summary><c>PUT …/configurations/{id}/selection</c> with the current tag of the list (S6-20b).</summary>
    public static async Task<HttpResponseMessage> PutListAsync<T>(
        this HttpClient client, string path, T body, CancellationToken cancellationToken) =>
        await client.SendWithTagAsync(HttpMethod.Put, path, body, await client.TagOfListAsync(path, cancellationToken), cancellationToken);

    /// <summary><c>DELETE …/configurations/{id}</c> with the current tag of the list (S6-20b).</summary>
    public static async Task<HttpResponseMessage> DeleteListAsync(
        this HttpClient client, string path, CancellationToken cancellationToken) =>
        await client.SendWithTagAsync(HttpMethod.Delete, path, await client.TagOfListAsync(path, cancellationToken), cancellationToken);

    /// <summary>The tag of the saved list a path below <c>/configurations/{id}</c> names.</summary>
    private static Task<string> TagOfListAsync(this HttpClient client, string path, CancellationToken cancellationToken)
    {
        const string Lists = "/configurations/";
        var start = path.IndexOf(Lists, StringComparison.Ordinal) + Lists.Length;
        var end = path.IndexOf('/', start);
        return client.TagOfAsync(end < 0 ? path : path[..end], cancellationToken);
    }

    /// <summary>A request with <paramref name="ifMatch"/>; <c>null</c> sends no <c>If-Match</c>.</summary>
    public static async Task<HttpResponseMessage> SendWithTagAsync<T>(
        this HttpClient client, HttpMethod method, string path, T? body, string? ifMatch, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, path);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        if (ifMatch is not null)
        {
            // Unvalidated: the tests also send what is no entity tag.
            request.Headers.TryAddWithoutValidation("If-Match", ifMatch);
        }

        return await client.SendAsync(request, cancellationToken);
    }

    /// <summary>A request without a body with <paramref name="ifMatch"/>.</summary>
    public static Task<HttpResponseMessage> SendWithTagAsync(
        this HttpClient client, HttpMethod method, string path, string? ifMatch, CancellationToken cancellationToken) =>
        client.SendWithTagAsync<object>(method, path, null, ifMatch, cancellationToken);
}
