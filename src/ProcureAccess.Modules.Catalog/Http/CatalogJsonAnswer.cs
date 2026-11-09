// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Cryptography;
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
/// </summary>
internal static class CatalogJsonAnswer
{
    /// <summary><c>Cache-Control</c> of an answer for a named release.</summary>
    public const string NamedReleaseCacheControl = "public, max-age=3600, must-revalidate";

    /// <summary><c>Cache-Control</c> of an answer for the active release.</summary>
    public const string ActiveReleaseCacheControl = "public, no-cache";

    private const string JsonContentType = "application/json; charset=utf-8";

    public static IResult Create<T>(T response, bool namedRelease, JsonSerializerOptions options, HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(context);
        var body = JsonSerializer.SerializeToUtf8Bytes(response, options);
        var etag = new EntityTagHeaderValue($"\"{Convert.ToHexStringLower(SHA256.HashData(body).AsSpan(0, 16))}\"");

        context.Response.GetTypedHeaders().ETag = etag;
        context.Response.Headers.CacheControl = namedRelease ? NamedReleaseCacheControl : ActiveReleaseCacheControl;

        var ifNoneMatch = context.Request.GetTypedHeaders().IfNoneMatch;
        if (ifNoneMatch.Any(tag => tag.Equals(EntityTagHeaderValue.Any) || tag.Compare(etag, useStrongComparison: false)))
        {
            return TypedResults.StatusCode(StatusCodes.Status304NotModified);
        }

        return TypedResults.Bytes(body, JsonContentType);
    }
}
