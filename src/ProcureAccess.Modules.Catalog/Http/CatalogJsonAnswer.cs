// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace ProcureAccess.Modules.Catalog.Http;

/// <summary>
/// The 200 answer of a catalog endpoint with the HTTP caching of ARCHITECTURE §9.6 (owner, S3-19
/// decision 3), the same for every catalog endpoint: the <c>ETag</c> is a hash of the body, so it
/// changes with the release and the selection; <c>If-None-Match</c> gets 304. A named release never
/// changes and may be kept for an hour; an answer for the active release must be revalidated on
/// every use, so the request after <c>catalog activate</c> returns the new release (CAT-09).
/// The answer names its language in <c>Content-Language</c> and varies by <c>Accept-Language</c>
/// (API-06, <see cref="CatalogLanguage"/>); the language is part of the <c>ETag</c>, so an answer in
/// one language never revalidates an answer cached in another.
/// </summary>
internal static class CatalogJsonAnswer
{
    /// <summary><c>Cache-Control</c> of an answer for a named release.</summary>
    public const string NamedReleaseCacheControl = "public, max-age=3600, must-revalidate";

    /// <summary><c>Cache-Control</c> of an answer for the active release.</summary>
    public const string ActiveReleaseCacheControl = "public, no-cache";

    private const string JsonContentType = "application/json; charset=utf-8";

    /// <param name="language">The language the texts of <paramref name="response"/> are in.</param>
    public static IResult Create<T>(T response, bool namedRelease, string language, JsonSerializerOptions options, HttpContext context)
    {
        ArgumentException.ThrowIfNullOrEmpty(language);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(context);
        var body = JsonSerializer.SerializeToUtf8Bytes(response, options);
        var etag = new EntityTagHeaderValue($"\"{Convert.ToHexStringLower(Hash(language, body))}\"");

        context.Response.GetTypedHeaders().ETag = etag;
        context.Response.Headers.CacheControl = namedRelease ? NamedReleaseCacheControl : ActiveReleaseCacheControl;
        context.Response.Headers.ContentLanguage = language;
        context.Response.Headers.Append(HeaderNames.Vary, HeaderNames.AcceptLanguage);

        var ifNoneMatch = context.Request.GetTypedHeaders().IfNoneMatch;
        if (ifNoneMatch.Any(tag => tag.Equals(EntityTagHeaderValue.Any) || tag.Compare(etag, useStrongComparison: false)))
        {
            return TypedResults.StatusCode(StatusCodes.Status304NotModified);
        }

        return TypedResults.Bytes(body, JsonContentType);
    }

    /// <summary>The first 16 bytes of the SHA-256 of the language and the body.</summary>
    private static byte[] Hash(string language, byte[] body)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(Encoding.UTF8.GetBytes(language + "\n"));
        hash.AppendData(body);
        return hash.GetHashAndReset()[..16];
    }
}
