// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Headers;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Catalog;

/// <summary>
/// The language part of the catalog contract (API-06, S4-23), the same for every catalog endpoint:
/// whatever the client asks for, a release with German texts only answers in German, says so in
/// <c>Content-Language</c> and varies by <c>Accept-Language</c> — also on a 304.
/// </summary>
internal static class CatalogLanguageContract
{
    /// <summary>The <c>Accept-Language</c> values every endpoint test runs with; <c>null</c> sends none.</summary>
    public static TheoryData<string?> AcceptLanguages => new("de", "en", "en-US,en;q=0.9", "fr", null);

    /// <summary>
    /// Asks <paramref name="uri"/> with <paramref name="acceptLanguage"/> and checks the answer against
    /// the one for <c>de</c>: the same body and ETag, <c>Content-Language: de</c>, and a 304 with the
    /// same headers when it revalidates with that ETag.
    /// </summary>
    public static async Task ShouldAnswerGermanAsync(HttpClient client, string uri, string? acceptLanguage, CancellationToken cancellation)
    {
        using var german = await GetAsync(client, uri, "de", ifNoneMatch: null, cancellation);
        using var response = await GetAsync(client, uri, acceptLanguage, ifNoneMatch: null, cancellation);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        ShouldNameGerman(response);
        (await response.Content.ReadAsStringAsync(cancellation)).ShouldBe(await german.Content.ReadAsStringAsync(cancellation));
        var etag = response.Headers.ETag.ShouldNotBeNull();
        etag.ShouldBe(german.Headers.ETag);

        using var revalidated = await GetAsync(client, uri, acceptLanguage, etag, cancellation);
        revalidated.StatusCode.ShouldBe(HttpStatusCode.NotModified);
        ShouldNameGerman(revalidated);
    }

    private static async Task<HttpResponseMessage> GetAsync(
        HttpClient client, string uri, string? acceptLanguage, EntityTagHeaderValue? ifNoneMatch, CancellationToken cancellation)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        if (acceptLanguage is not null)
        {
            request.Headers.TryAddWithoutValidation("Accept-Language", acceptLanguage).ShouldBeTrue();
        }

        if (ifNoneMatch is not null)
        {
            request.Headers.IfNoneMatch.Add(ifNoneMatch);
        }

        return await client.SendAsync(request, cancellation);
    }

    private static void ShouldNameGerman(HttpResponseMessage response)
    {
        response.Content.Headers.ContentLanguage.ShouldBe(["de"]);
        response.Headers.Vary.ShouldContain("Accept-Language");
    }
}
