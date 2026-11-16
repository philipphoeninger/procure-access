// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ProcureAccess.Modules.Platform.Contracts.Errors;

namespace ProcureAccess.Modules.Platform.Sharing;

/// <summary>
/// <c>GET /api/v1/shared/{token}</c> (SPEC R-SAV-05, §8.5; ARCHITECTURE §7.2, C59; S5-23): a shared
/// list for someone without an account. No sign-in, no antiforgery (a GET), no rate limit (owner
/// decision S5-23 7: a token of 32 random bytes cannot be guessed). <c>no-store</c> comes from the
/// platform default, so a revocation takes effect at once.
/// </summary>
internal static class SharedEndpoints
{
    public const string GroupPath = "/shared";

    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder routes)
    {
        routes.MapGet($"{GroupPath}/{{token}}", ReadAsync)
              .AllowAnonymous()
              .WithTags("Sharing")
              .WithName("GetShared")
              .WithSummary("A shared criteria list, read-only and without an account")
              .WithDescription(
                  "200 { name, releaseVersion, expiresAt, selection, criteria }: the list in the shape of GET /configurations/{id} " +
                  "without its id, organisation and dates (TEN-08); the texts come from GET /catalog/criteria with " +
                  "release=<releaseVersion>. 404 for an unknown, revoked or expired link and for a deleted list.")
              .Produces<object>()
              .ProducesProblem(StatusCodes.Status404NotFound);
        return routes;
    }

    private static async Task<IResult> ReadAsync(string token, ShareLinkService links, CancellationToken cancellationToken) =>
        await links.ReadSharedAsync(token, cancellationToken) is { } shared
            ? TypedResults.Ok(shared)
            : TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, type: ProblemTypes.NotFound);
}
