// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using ProcureAccess.Modules.IdentityAccess.Domain;

namespace ProcureAccess.Modules.IdentityAccess.Accounts;

/// <summary>
/// The account as S61 shows it, <c>GET /api/v1/account</c> (SPEC §8.13; S5-28b, owner decision
/// S5-28b 1): the signed-in user's address and whether it is confirmed, so the page shows
/// <c>auth.confirm.required</c> for an unconfirmed one (R-AUTH-02; Handoff §5.3). The session probe
/// does not say it.
/// </summary>
/// <remarks>
/// Needs a sign-in (401), no confirmed address. <c>Cache-Control: no-store</c> is Platform's
/// default for <c>/api/v1</c> (TEN-04). 404 when the account no longer exists.
/// </remarks>
internal static class AccountEndpoints
{
    public const string Path = "/account";

    /// <summary>Maps the read on <paramref name="routes"/>, the versioned group.</summary>
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet(Path, GetAsync)
              .RequireAuthorization()
              .WithTags("Account")
              .WithName("GetAccount")
              .WithSummary("The signed-in user's address and whether it is confirmed")
              .WithDescription("200 { email, emailConfirmed }; 401; 404 when the account no longer exists.")
              .Produces<AccountResponse>()
              .ProducesProblem(StatusCodes.Status404NotFound);
        return routes;
    }

    private static async Task<Results<Ok<AccountResponse>, ProblemHttpResult>> GetAsync(
        ClaimsPrincipal principal, UserManager<User> users)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("A signed-in principal without a user id reached the account read.");
        var user = await users.FindByIdAsync(userId);

        // Soft-deleted accounts are hidden by the context's query filter.
        return user is null
            ? TypedResults.Problem(statusCode: StatusCodes.Status404NotFound)
            : TypedResults.Ok(new AccountResponse(user.Email ?? "", user.EmailConfirmed));
    }
}

/// <summary>The answer of <c>GET /api/v1/account</c>.</summary>
internal sealed record AccountResponse(string Email, bool EmailConfirmed);
