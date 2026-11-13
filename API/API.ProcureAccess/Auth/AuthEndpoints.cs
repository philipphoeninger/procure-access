// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using ProcureAccess.Modules.Platform.Contracts.Errors;

namespace API.ProcureAccess.Auth;

/// <summary>
/// Sign-in and sign-out of the cookie session (ARCHITECTURE §7.1, §9.1, C60; SPEC R-AUTH-03,
/// R-AUTH-08; SEC-01, SEC-09). In the host, because they need <see cref="SignInManager{TUser}"/> of
/// the legacy <see cref="User"/>; they move into the IdentityAccess module with the users (M3.1),
/// like the <c>roles</c> command.
/// </summary>
/// <remarks>
/// <para><c>POST /api/v1/auth/login</c>: wrong e-mail address, wrong password, a locked-out or a
/// deleted account all answer 400 Problem Details of type <c>validation</c> with one error for the
/// body as a whole (<c>$</c>), so the answer names no field and the client's login overlay never
/// sees a 401 it would open itself on (owner decision S5-07 Q2 (a)). A failed attempt counts
/// towards the lockout (5 attempts, 15 minutes 🔧, R-AUTH-03). Success sets the session cookie
/// and answers 204.</para>
/// <para><c>POST /api/v1/auth/logout</c>: ends the session (the cookie is deleted) and answers 204,
/// also for a request without a session.</para>
/// <para>Both issue a new antiforgery token, because it is bound to the user (SEC-09); both are
/// state-changing and therefore validated like every other endpoint, so the client needs the token
/// of the probe before signing in.</para>
/// </remarks>
public static class AuthEndpoints
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
            .WithName("PostLogin")
            .WithSummary("Signs in with e-mail address and password and sets the session cookie")
            .WithDescription(
                "204 with the session cookie and a new XSRF-TOKEN. 400 validation with an error for '$' for a wrong " +
                "address or password, a locked or a deleted account; 400 antiforgery without the token of GET /auth/session.")
            .Accepts<LoginRequestBody>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

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
        IdentityAccessModule.IssueAntiforgeryToken(context);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> LogoutAsync(SignInManager<User> signIn, HttpContext context)
    {
        await signIn.SignOutAsync();

        context.User = new ClaimsPrincipal(new ClaimsIdentity());
        IdentityAccessModule.IssueAntiforgeryToken(context);
        return TypedResults.NoContent();
    }

    private static ValidationProblem InvalidLogin() =>
        TypedResults.ValidationProblem(
            new Dictionary<string, string[]>(StringComparer.Ordinal) { [BodyKey] = [InvalidLoginMessage] },
            type: ProblemTypes.Validation);
}

/// <summary>Body of <c>POST /api/v1/auth/login</c>. Both members are required.</summary>
public sealed record LoginRequestBody(string? Email, string? Password);
