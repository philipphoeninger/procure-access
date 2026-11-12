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
using ProcureAccess.Modules.Platform.Contracts.Errors;

namespace ProcureAccess.Modules.Export.Http;

/// <summary>
/// <c>POST /api/v1/exports</c> (ARCHITECTURE §7.2 „Export without account“, Flow A1; R-LST-12):
/// the export of a draft without an account. The body names format, selection and a decision per
/// criterion (<see cref="ExportRequest"/>); the answer is the file as an attachment.
/// </summary>
/// <remarks>
/// <para><b>Nothing is stored (EXP-05).</b> The file is rendered in memory and streamed; no row,
/// no file, and neither the body nor a note is logged. The usage event <c>ExportCreated</c> follows
/// with S4-21, the rate limit of SPEC §7.3 too (O-S4-56).</para>
/// <para><b>Limits (SPEC §7.2).</b> The body is at most <see cref="MaxRequestBodySize"/> bytes
/// (413); <see cref="AdHocSelectionSource"/> checks the rest.</para>
/// <para><b>Caching (TEN-04, ARCHITECTURE §9.6).</b> Every answer of the handler carries
/// <c>Cache-Control: no-store</c>; answers of the exception handler (a body that cannot be read)
/// carry ASP.NET's <c>no-cache,no-store</c>.</para>
/// </remarks>
internal static class ExportEndpoint
{
    public const string Path = "/exports";

    /// <summary>SPEC §7.2 „Anonymous export request: body ≤ 1 MB“.</summary>
    public const long MaxRequestBodySize = 1024 * 1024;

    public const string NoStore = "no-store";

    private const string FormatField = "format";

    public static RouteHandlerBuilder Map(IEndpointRouteBuilder routes) =>
        routes.MapPost(Path, HandleAsync)
            .AllowAnonymous()
            .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBodySize))
            .WithName("PostExport")
            .WithSummary("Exports the criteria of a selection with the user's decisions as XLSX or CSV, without an account")
            .WithDescription(
                "The server resolves the recommended priorities itself and stores nothing. The request lists every criterion " +
                "of the selection once by its revision. 400 validation, selection-excluded or release-mismatch (the release is no " +
                "longer active), 404 for an unknown release, 413 above 1 MB.")
            .Accepts<ExportRequest>("application/json")
            .Produces(
                StatusCodes.Status200OK,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                additionalContentTypes: "text/csv")
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status413PayloadTooLarge);

    private static async Task<IResult> HandleAsync(
        ExportRequest request,
        AdHocSelectionSource source,
        ExportFileWriter writer,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        context.Response.Headers.CacheControl = NoStore;

        var format = ExportFormats.Parse(request.Format);
        var result = await source.LoadAsync(request, cancellationToken);
        if (format is null)
        {
            var formatError = new KeyValuePair<string, string[]>(FormatField, ["The format must be xlsx or csv."]);
            if (result.Problem is null)
            {
                return ProblemOf(ExportProblem.Validation(new Dictionary<string, string[]>([formatError], StringComparer.Ordinal)));
            }

            if (result.Problem.Type == ProblemTypes.Validation && result.Problem.Errors is { } errors)
            {
                return ProblemOf(ExportProblem.Validation(new Dictionary<string, string[]>(errors.Append(formatError), StringComparer.Ordinal)));
            }
        }

        if (result.Problem is { } problem)
        {
            return ProblemOf(problem);
        }

        var file = writer.Write(result.Content!, format!.Value);
        return TypedResults.File(file.Content, file.ContentType, file.FileName);
    }

    private static IResult ProblemOf(ExportProblem problem) =>
        problem.Errors is { } errors
            ? TypedResults.ValidationProblem(errors, detail: problem.Detail, type: problem.Type)
            : TypedResults.Problem(detail: problem.Detail, statusCode: problem.Status, type: problem.Type);
}
