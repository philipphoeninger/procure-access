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
using ProcureAccess.Modules.IdentityAccess.Session;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.RateLimiting;

namespace ProcureAccess.Modules.IdentityAccess.Auth;

/// <summary>
/// Sign-in and sign-out of the cookie session (ARCHITECTURE §7.1, §9.1, C60; SPEC R-AUTH-03,
/// R-AUTH-08; SEC-01, SEC-09). In the module since S6-10.
/// </summary>
/// <remarks>
/// <para><c>POST /api/v1/auth/login</c>: wrong e-mail address, wrong password, a locked-out or a
/// deleted account all answer 400 Problem Details of type <c>validation</c> with one error for the
/// body as a whole (<c>$</c>), so the answer names no field and the client's login overlay never
/// sees a 401 it would open itself on (owner decision S5-07 Q2 (a)). A failed attempt counts
/// towards the lockout (5 attempts, 15 minutes 🔧, R-AUTH-03). Success sets the session cookie
/// and answers 204.</para>
/// <para>Rate limits (SPEC §7.3, S6-26, owner decision O-S6-2): <see cref="RateLimitPolicies.SignIn"/>
/// per client IP (middleware) and <see cref="RateLimitPolicies.SignInAddress"/> per address
/// (<see cref="IKeyedRateLimiter"/>, after binding). Every attempt with both fields counts, also a
/// successful one; the address counts before it is looked up, so the 429 is the same for an
/// address with and without an account.</para>
/// <para><c>POST /api/v1/auth/logout</c>: ends the session (the cookie is deleted) and answers 204,
/// also for a request without a session.</para>
/// <para>Both issue a new antiforgery token, because it is bound to the user (SEC-09); both are
/// state-changing and therefore validated like every other endpoint, so the client needs the token
/// of the probe before signing in.</para>
/// </remarks>
internal static class AuthEndpoints
{
    public const string LoginPath = "/login";
    public const string LogoutPath = "/logout";

    private const string BodyKey = "$";
    private const string InvalidLoginMessage = "The e-mail address or the password is wrong, or the account is locked.";

    /// <summary>Maps the endpoints below <c>/auth</c> of <paramref name="routes"/>, the versioned group.</summary>
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var auth = routes.MapGroup("/auth").WithTags("Auth");

        auth.MapPost(LoginPath, LoginAsync)
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.SignIn)
            .WithName("PostLogin")
            .WithSummary("Signs in with e-mail address and password and sets the session cookie")
            .WithDescription(
                "204 with the session cookie and a new XSRF-TOKEN. 400 validation with an error for '$' for a wrong " +
                "address or password, a locked or a deleted account; 400 antiforgery without the token of GET /auth/session; " +
                "429 rate-limit per client IP or per address.")
            .Accepts<LoginRequestBody>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

        auth.MapPost(LogoutPath, LogoutAsync)
            .AllowAnonymous()
            .WithName("PostLogout")
            .WithSummary("Ends the session and deletes the cookie")
            .WithDescription("204 and a new XSRF-TOKEN, also without a session. 400 antiforgery without the token.")
            .Produces(StatusCodes.Status204NoContent);

        return routes;
    }

    private static async Task<Results<NoContent, ValidationProblem>> LoginAsync(
        LoginRequestBody body,
        UserManager<User> users,
        SignInManager<User> signIn,
        ILookupNormalizer normalizer,
        IKeyedRateLimiter limiter,
        HttpContext context)
    {
        var fieldErrors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(body.Email))
        {
            fieldErrors["email"] = ["The e-mail address is required."];
        }

        if (string.IsNullOrEmpty(body.Password))
        {
            fieldErrors["password"] = ["The password is required."];
        }

        if (fieldErrors.Count > 0)
        {
            return TypedResults.ValidationProblem(fieldErrors, type: ProblemTypes.Validation);
        }

        // Throws RateLimitExceededException (→ 429) for the 🔧 eleventh attempt per hour for this
        // address, before the lookup, so the answer never tells whether the address has an account.
        limiter.Acquire(RateLimitPolicies.SignInAddress, normalizer.NormalizeEmail(body.Email!.Trim()));

        var user = await users.FindByEmailAsync(body.Email!);
        if (user is null || user.IsDeleted)
        {
            return InvalidLogin();
        }

        var result = await signIn.CheckPasswordSignInAsync(user, body.Password!, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return InvalidLogin();
        }

        await signIn.SignInAsync(user, isPersistent: false);

        // The antiforgery token is bound to HttpContext.User, which still holds the anonymous
        // principal of this request: bind the new token to the user who just signed in.
        context.User = await signIn.CreateUserPrincipalAsync(user);
        AntiforgeryTokenCookie.Issue(context);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> LogoutAsync(SignInManager<User> signIn, HttpContext context)
    {
        await signIn.SignOutAsync();

        context.User = new ClaimsPrincipal(new ClaimsIdentity());
        AntiforgeryTokenCookie.Issue(context);
        return TypedResults.NoContent();
    }

    private static ValidationProblem InvalidLogin() =>
        TypedResults.ValidationProblem(
            new Dictionary<string, string[]>(StringComparer.Ordinal) { [BodyKey] = [InvalidLoginMessage] },
            type: ProblemTypes.Validation);
}

/// <summary>Body of <c>POST /api/v1/auth/login</c>. Both members are required.</summary>
internal sealed record LoginRequestBody(string? Email, string? Password);
