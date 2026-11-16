// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ProcureAccess.Modules.Export.Formatting;
using ProcureAccess.Modules.Export.Requests;
using ProcureAccess.Modules.Export.Sources;
using ProcureAccess.Modules.Platform.Contracts.Analytics;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.RateLimiting;

namespace ProcureAccess.Modules.Export.Http;

/// <summary>
/// <c>POST /api/v1/configurations/{id}/exports</c> (ARCHITECTURE §7.2, Flow A2; R-LST-12; S5-21a):
/// the export of a saved list as XLSX or CSV, under the release the list is pinned to (ADR-002,
/// CAT-06). The content comes from <see cref="PersistedConfigurationSource"/>, the rows from the
/// same <c>ExportRow.Of</c> as the anonymous export (EXP-06); the file name carries the slug of the
/// list name (SPEC §8.3).
/// </summary>
/// <remarks>
/// <para><b>Access.</b> A sign-in is required (401); every member of the list's organisation may
/// export — Readers too, the export only reads (owner, S5-21a). A list of another organisation does
/// not exist for the caller: 404 through the organisation filter (TEN-01). Antiforgery applies like
/// on every state-changing route (SEC-02); only the anonymous export is exempt.</para>
/// <para><b>Rate limit (SPEC §7.3, API-05).</b> <see cref="RateLimitPolicies.SavedExport"/>: per
/// signed-in user, 60 per 10 minutes 🔧.</para>
/// <para><b>Nothing new is stored.</b> The file is rendered in memory; a created file writes the
/// usage event <c>ExportCreated</c> (signed in). Every answer of the handler carries
/// <c>Cache-Control: no-store</c>, as the anonymous export's.</para>
/// </remarks>
internal static class SavedExportEndpoint
{
    public const string Path = "/configurations/{id}/exports";

    /// <summary>The body names only the format.</summary>
    public const long MaxRequestBodySize = 4 * 1024;

    public static RouteHandlerBuilder Map(IEndpointRouteBuilder routes) =>
        routes.MapPost(Path, HandleAsync)
            .RequireAuthorization()
            .RequireRateLimiting(RateLimitPolicies.SavedExport)
            .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBodySize))
            .WithName("PostConfigurationExport")
            .WithSummary("Exports a saved criteria list as XLSX or CSV under its catalog release (every member)")
            .WithDescription(
                "Body { format: \"xlsx\" | \"csv\" }. The file name carries a slug of the list name. 400 validation; " +
                "400 antiforgery; 401; 404 for an unknown list or one of another organisation; 429 rate-limit (per user).")
            .Accepts<SavedExportRequest>("application/json")
            .Produces(
                StatusCodes.Status200OK,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                additionalContentTypes: "text/csv")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

    private static async Task<IResult> HandleAsync(
        Guid id,
        SavedExportRequest request,
        PersistedConfigurationSource source,
        ExportFileWriter writer,
        IUsageEventWriter usageEvents,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        context.Response.Headers.CacheControl = ExportEndpoint.NoStore;

        if (ExportFormats.Parse(request.Format) is not { } format)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>(StringComparer.Ordinal) { ["format"] = ["The format must be xlsx or csv."] },
                type: ProblemTypes.Validation);
        }

        var result = await source.LoadAsync(id, cancellationToken);
        if (result.Problem is { } problem)
        {
            return TypedResults.Problem(detail: problem.Detail, statusCode: problem.Status, type: problem.Type);
        }

        var file = writer.Write(result.Content!, format, result.ListName);
        usageEvents.ExportCreated(ExportEndpoint.UsageFormatOf(format), signedIn: true);
        return TypedResults.File(file.Content, file.ContentType, file.FileName);
    }
}
