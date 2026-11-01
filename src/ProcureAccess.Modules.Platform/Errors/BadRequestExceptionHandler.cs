// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ProcureAccess.Modules.Platform.Errors;

/// <summary>
/// Answers a request that a minimal-API endpoint cannot bind (malformed JSON, a missing required
/// parameter) with a validation problem, as controllers do (API-01). Controllers never get here:
/// they report binding errors through the model state.
/// </summary>
/// <remarks>
/// The field path comes from the JSON reader ("$.email" → "email"; see
/// <see cref="ProblemDetailsRegistration.Complete"/>). Other binding errors carry no field and are
/// keyed "$", the request as a whole.
/// </remarks>
internal sealed class BadRequestExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    private const string RequestKey = "$";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is not BadHttpRequestException badRequest)
        {
            return false;
        }

        // Other statuses (e.g. 413 for a body that is too large) are not validation errors.
        ProblemDetails problem = badRequest.StatusCode == StatusCodes.Status400BadRequest
            ? new HttpValidationProblemDetails(Errors(badRequest))
            : new ProblemDetails();
        problem.Status = badRequest.StatusCode;
        httpContext.Response.StatusCode = badRequest.StatusCode;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception,
        });
    }

    private static Dictionary<string, string[]> Errors(BadHttpRequestException badRequest) =>
        badRequest.InnerException is JsonException json
            ? new() { [json.Path ?? RequestKey] = [json.Message] }
            : new() { [RequestKey] = [badRequest.Message] };
}
