// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using ProcureAccess.Modules.Catalog.Reading;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace ProcureAccess.Modules.Catalog.FilterOptions;

/// <summary>
/// <c>GET /api/v1/catalog/filter-options</c> (SPEC §8.1, ARCHITECTURE §7.2): the filter values of
/// the four layers with <c>excludedBy[]</c> for the given selection, under the rules of the active
/// release or of the release named by <c>release</c> (CAT-12, CAT-18, CAT-19). Public, read-only.
/// </summary>
/// <remarks>
/// <para><b>Errors (owner, S3-19 decision 4).</b> An unknown release, or no active release when
/// none is named, answers 404 <c>not-found</c>; an unknown key answers 400 <c>validation</c> with
/// one error per key.</para>
/// <para><b>HTTP caching (ARCHITECTURE §9.6, owner, S3-19 decision 3).</b> The <c>ETag</c> is a
/// hash of the body, so it changes with the release and the selection; <c>If-None-Match</c> gets
/// 304. A named release never changes and may be kept for an hour; an answer for the active release
/// must be revalidated on every use, so the request after <c>catalog activate</c> returns the new
/// release (CAT-09).</para>
/// </remarks>
internal static class FilterOptionsEndpoint
{
    public const string Path = "/filter-options";

    /// <summary><c>Cache-Control</c> of an answer for a named release.</summary>
    public const string NamedReleaseCacheControl = "public, max-age=3600, must-revalidate";

    /// <summary><c>Cache-Control</c> of an answer for the active release.</summary>
    public const string ActiveReleaseCacheControl = "public, no-cache";

    private const string JsonContentType = "application/json; charset=utf-8";

    public static RouteHandlerBuilder Map(IEndpointRouteBuilder routes) =>
        routes.MapGet(Path, HandleAsync)
            .WithName("GetCatalogFilterOptions")
            .WithSummary("Filter values of the four layers with the selected values that exclude them")
            .WithDescription(
                "Without release the rules of the active release apply. Without a selection every excludedBy is empty. " +
                "Inactive use cases are not listed but are accepted in useCase.")
            .Produces<FilterOptionsResponse>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status304NotModified)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> HandleAsync(
        [FromQuery(Name = "release")] string? release,
        [FromQuery(Name = FilterOptionsResolver.UseCaseParameter)] string? useCase,
        [FromQuery(Name = FilterOptionsResolver.AppTypesParameter)] string?[]? appTypeIds,
        [FromQuery(Name = FilterOptionsResolver.ProductPartsParameter)] string?[]? productPartIds,
        CatalogReader reader,
        IOptions<HttpJsonOptions> jsonOptions,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var namedRelease = !string.IsNullOrEmpty(release);
        var snapshot = namedRelease
            ? await reader.GetReleaseByVersionAsync(release!, cancellationToken)
            : await reader.GetActiveReleaseAsync(cancellationToken);
        if (snapshot is null)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: namedRelease ? $"Catalog release '{release}' is not stored." : "No catalog release is active.");
        }

        var values = await reader.GetFilterValuesAsync(cancellationToken);
        var (selection, errors) = FilterOptionsResolver.ReadSelection(values, useCase, appTypeIds, productPartIds);
        if (selection is null)
        {
            return TypedResults.ValidationProblem(errors);
        }

        var response = FilterOptionsResolver.Resolve(values, snapshot, selection);
        var body = JsonSerializer.SerializeToUtf8Bytes(response, jsonOptions.Value.SerializerOptions);
        var etag = new EntityTagHeaderValue($"\"{Convert.ToHexStringLower(SHA256.HashData(body).AsSpan(0, 16))}\"");

        var headers = context.Response.GetTypedHeaders();
        headers.ETag = etag;
        context.Response.Headers.CacheControl = namedRelease ? NamedReleaseCacheControl : ActiveReleaseCacheControl;

        var ifNoneMatch = context.Request.GetTypedHeaders().IfNoneMatch;
        if (ifNoneMatch.Any(tag => tag.Equals(EntityTagHeaderValue.Any) || tag.Compare(etag, useStrongComparison: false)))
        {
            return TypedResults.StatusCode(StatusCodes.Status304NotModified);
        }

        return TypedResults.Bytes(body, JsonContentType);
    }
}
