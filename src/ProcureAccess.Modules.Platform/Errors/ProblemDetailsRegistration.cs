// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Platform.Contracts.Errors;

namespace ProcureAccess.Modules.Platform.Errors;

/// <summary>
/// Errors as RFC 9457 Problem Details (ARCHITECTURE §7.1, SPEC §7.1, §8.6, API-01): every error
/// response of the API, from controllers, minimal APIs, unhandled exceptions and empty error
/// responses, is a Problem Details document whose <c>type</c> is
/// <c>&lt;base&gt;/problems/&lt;name&gt;</c> for the names of <see cref="ProblemTypes"/>.
/// </summary>
internal static class ProblemDetailsRegistration
{
    /// <summary>Configuration key of the base URI of the problem types (🔧, SPEC §8.6).</summary>
    public const string BaseUriKey = "ProblemTypes:BaseUri";

    /// <summary>
    /// Types ASP.NET Core sets on its own (links into RFC 9110 per status code). Platform replaces
    /// them where a problem type of <see cref="ProblemTypes"/> applies.
    /// </summary>
    private const string FrameworkTypePrefix = "https://tools.ietf.org/html/rfc9110#";

    public static IServiceCollection AddPlatformProblemDetails(this IServiceCollection services, IConfiguration configuration)
    {
        var baseUri = ReadBaseUri(configuration);

        services.AddProblemDetails(options =>
            options.CustomizeProblemDetails = context => Complete(context.ProblemDetails, baseUri));

        // Minimal APIs: a request that cannot be bound throws instead of ending as an empty 400,
        // so BadRequestExceptionHandler can answer with a validation problem.
        services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);
        services.AddExceptionHandler<BadRequestExceptionHandler>();

        // Controllers: validation errors of a property are keyed by its JSON name ("newEmail"),
        // as the client sends it, not by the C# name ("NewEmail").
        services.Configure<MvcOptions>(options =>
            options.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider()));

        return services;
    }

    /// <summary>
    /// Unhandled exceptions become 500 Problem Details without the exception text, and error
    /// responses without a body (e.g. 404 for an unknown route) become Problem Details.
    /// Must run before every middleware whose errors it should cover.
    /// </summary>
    public static IApplicationBuilder UsePlatformProblemDetails(this IApplicationBuilder app) =>
        app.UseExceptionHandler()
           .UseStatusCodePages();

    /// <summary>
    /// Sets the problem type and the error keys of a document written by any part of the API.
    /// </summary>
    internal static void Complete(ProblemDetails problem, string baseUri)
    {
        string? name = null;
        if (problem.Type is { } type && ProblemTypes.All.Contains(type))
        {
            name = type;
        }
        else if (problem.Type is null or "about:blank" || IsFrameworkType(problem.Type))
        {
            name = DefaultName(problem);
        }

        if (name is not null)
        {
            problem.Type = $"{baseUri}/problems/{name}";
        }

        if (problem is HttpValidationProblemDetails validation)
        {
            KeyByFieldPath(validation.Errors);
        }
    }

    private static bool IsFrameworkType(string type) =>
        type.StartsWith(FrameworkTypePrefix, StringComparison.Ordinal);

    /// <summary>The problem type that applies without being set; null when none does.</summary>
    private static string? DefaultName(ProblemDetails problem) => problem switch
    {
        HttpValidationProblemDetails => ProblemTypes.Validation,
        { Status: StatusCodes.Status404NotFound } => ProblemTypes.NotFound,
        _ => null,
    };

    /// <summary>
    /// Errors of the JSON reader are keyed by a JSON path ("$.items[0].note"); the field path is
    /// the same without the root "$." ("items[0].note"). An error of the whole body keeps "$".
    /// </summary>
    private static void KeyByFieldPath(IDictionary<string, string[]> errors)
    {
        foreach (var key in errors.Keys.Where(k => k.StartsWith("$.", StringComparison.Ordinal)).ToList())
        {
            var fieldPath = key[2..];
            errors[fieldPath] = errors.TryGetValue(fieldPath, out var existing)
                ? [.. existing, .. errors[key]]
                : errors[key];
            errors.Remove(key);
        }
    }

    /// <summary>The configured base URI without a trailing slash; startup fails if it is not an absolute http(s) URI.</summary>
    private static string ReadBaseUri(IConfiguration configuration)
    {
        var value = configuration[BaseUriKey];
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            throw new InvalidOperationException(
                $"{BaseUriKey} must be an absolute http(s) URI, e.g. https://procureaccess.de (SPEC §8.6).");
        }

        return value!.TrimEnd('/');
    }
}
