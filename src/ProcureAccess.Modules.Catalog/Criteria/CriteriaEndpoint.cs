// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using ProcureAccess.Modules.Catalog.Http;
using ProcureAccess.Modules.Catalog.Reading;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using HttpJsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace ProcureAccess.Modules.Catalog.Criteria;

/// <summary>
/// <c>GET /api/v1/catalog/criteria</c> (SPEC §8.2, ARCHITECTURE §7.2): the criteria of a complete
/// selection with resolved priorities, allowed ratings and procedure texts, plus the selected app
/// types without criteria, under the active release or the release named by <c>release</c>.
/// Public, read-only.
/// </summary>
/// <remarks>
/// <para><b>Errors.</b> 404 <c>not-found</c> for an unknown release, no active release, or a
/// release <c>catalog activate</c> would refuse (owner, S3-20 decision 7); 400 <c>validation</c>
/// for an incomplete selection and 400 <c>selection-excluded</c> for a value excluded by the rules
/// of the release (decision 5), each with one error per field as sent.</para>
/// <para><b>HTTP caching</b> as for every catalog endpoint: <see cref="CatalogJsonAnswer"/>.</para>
/// </remarks>
internal static class CriteriaEndpoint
{
    public const string Path = "/criteria";

    public static RouteHandlerBuilder Map(IEndpointRouteBuilder routes) =>
        routes.MapGet(Path, HandleAsync)
            .WithName("GetCatalogCriteria")
            .WithSummary("Criteria of a selection with resolved priorities, allowed ratings and procedure texts")
            .WithDescription(
                "Without release the active release applies. A selection names at most one useCase, at least one app type " +
                "and every always-selected product part. 400 validation for an incomplete selection, 400 selection-excluded " +
                "for a value excluded by the rules of the release.")
            .Produces<CriteriaResponse>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status304NotModified)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> HandleAsync(
        [FromQuery(Name = "release")] string? release,
        [FromQuery(Name = CriteriaSelectionReader.UseCaseParameter)] string?[]? useCase,
        [FromQuery(Name = CriteriaSelectionReader.AppTypesParameter)] string?[]? appTypeIds,
        [FromQuery(Name = CriteriaSelectionReader.ProductPartsParameter)] string?[]? productPartIds,
        [FromQuery(Name = CriteriaSelectionReader.TestTypesParameter)] string?[]? testTypeIds,
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

        if (!snapshot.IsComplete)
        {
            return TypedResults.Problem(
                statusCode: StatusCodes.Status404NotFound,
                detail: $"Catalog release '{snapshot.Version}' is incomplete and is not served: {string.Join(" ", snapshot.Gaps)}");
        }

        var values = await reader.GetFilterValuesAsync(cancellationToken);
        var result = CriteriaSelectionReader.Read(values, snapshot.Exclusions, useCase, appTypeIds, productPartIds, testTypeIds);
        if (result.Selection is null)
        {
            var errors = result.Errors.ToDictionary(e => e.Key, e => e.Value, StringComparer.Ordinal);
            return result.Problem == SelectionProblem.Excluded
                ? TypedResults.ValidationProblem(errors, type: ProblemTypes.SelectionExcluded)
                : TypedResults.ValidationProblem(errors);
        }

        var response = CriteriaResolver.Resolve(values, snapshot, result.Selection);
        return CatalogJsonAnswer.Create(response, namedRelease, jsonOptions.Value.SerializerOptions, context);
    }
}
