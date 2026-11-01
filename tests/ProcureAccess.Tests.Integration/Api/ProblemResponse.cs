// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Text.Json;

namespace ProcureAccess.Tests.Integration.Api;

/// <summary>
/// An error response read as raw JSON (TESTING §5.2: contract tests never deserialise into DTOs),
/// with the checks of RFC 9457 and SPEC §7.1 / §8.6 that the Problem Details tests share.
/// </summary>
internal sealed class ProblemResponse
{
    public const string MediaType = "application/problem+json";

    /// <summary>
    /// Media types a Problem Details document may come with. RFC 9457 registers
    /// <see cref="MediaType"/>; the legacy controller actions carry <c>[Produces("application/json")]</c>,
    /// which overrides it (S2-10 protocol, open points). New endpoints use <see cref="MediaType"/>.
    /// </summary>
    public static readonly IReadOnlySet<string> AcceptedMediaTypes = new HashSet<string> { MediaType, "application/json" };

    private ProblemResponse(HttpStatusCode statusCode, string? mediaType, string body)
    {
        StatusCode = statusCode;
        MediaTypeOfResponse = mediaType;
        Body = body;
        try
        {
            Json = JsonDocument.Parse(body).RootElement.Clone();
        }
        catch (JsonException)
        {
            Json = null;
        }
    }

    public HttpStatusCode StatusCode { get; }

    public string? MediaTypeOfResponse { get; }

    public string Body { get; }

    /// <summary>The parsed body; null when the body is empty or not JSON.</summary>
    public JsonElement? Json { get; }

    public static async Task<ProblemResponse> ReadAsync(HttpResponseMessage response, CancellationToken cancellationToken) =>
        new(response.StatusCode,
            response.Content.Headers.ContentType?.MediaType,
            await response.Content.ReadAsStringAsync(cancellationToken));

    public string? Property(string name) =>
        Json is { ValueKind: JsonValueKind.Object } json
        && json.TryGetProperty(name, out var value)
        && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    /// <summary>The keys of the <c>errors</c> map; empty when there is none.</summary>
    public IReadOnlyList<string> ErrorKeys =>
        Json is { ValueKind: JsonValueKind.Object } json
        && json.TryGetProperty("errors", out var errors)
        && errors.ValueKind == JsonValueKind.Object
            ? errors.EnumerateObject().Select(e => e.Name).ToList()
            : [];

    /// <summary>
    /// Every way the response misses a Problem Details document with <paramref name="status"/> and
    /// <paramref name="type"/>; empty when it matches. With <paramref name="requireErrors"/> the
    /// document must also carry a non-empty <c>errors</c> map whose values are non-empty lists of
    /// messages.
    /// </summary>
    public IReadOnlyList<string> Deviations(HttpStatusCode status, string type, bool requireErrors)
    {
        var deviations = new List<string>();
        if (StatusCode != status)
        {
            deviations.Add($"status {(int)StatusCode} instead of {(int)status}");
        }

        if (MediaTypeOfResponse is null || !AcceptedMediaTypes.Contains(MediaTypeOfResponse))
        {
            deviations.Add($"content type '{MediaTypeOfResponse}' instead of '{MediaType}'");
        }

        if (Json is not { ValueKind: JsonValueKind.Object } json)
        {
            deviations.Add($"body is not a JSON object: '{Body}'");
            return deviations;
        }

        if (Property("type") != type)
        {
            deviations.Add($"type '{Property("type")}' instead of '{type}'");
        }

        if (string.IsNullOrWhiteSpace(Property("title")))
        {
            deviations.Add("no title");
        }

        if (!json.TryGetProperty("status", out var statusValue)
            || statusValue.ValueKind != JsonValueKind.Number
            || statusValue.GetInt32() != (int)status)
        {
            deviations.Add("no matching status member");
        }

        if (requireErrors)
        {
            if (!json.TryGetProperty("errors", out var errors)
                || errors.ValueKind != JsonValueKind.Object
                || !errors.EnumerateObject().Any())
            {
                deviations.Add("no errors map");
            }
            else if (!errors.EnumerateObject().All(IsListOfMessages))
            {
                deviations.Add("an errors entry is not a non-empty list of messages");
            }
        }

        return deviations;
    }

    private static bool IsListOfMessages(JsonProperty entry) =>
        entry.Value.ValueKind == JsonValueKind.Array
        && entry.Value.GetArrayLength() > 0
        && entry.Value.EnumerateArray().All(m => m.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(m.GetString()));
}
