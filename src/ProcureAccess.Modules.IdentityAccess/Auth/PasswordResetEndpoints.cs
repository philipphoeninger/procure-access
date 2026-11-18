// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.RateLimiting;

namespace ProcureAccess.Modules.IdentityAccess.Auth;

/// <summary>
/// Password reset (SPEC R-AUTH-04, §8.8 v2.23; ARCHITECTURE §9.1, C72, C141; S5-13a). In the
/// module since S6-10.
/// </summary>
/// <remarks>
/// <para><c>POST /api/v1/auth/password-reset</c> <c>{ email }</c>: 200 <c>{ validHours }</c> for every
/// well-formed address, known or not. The endpoint only hands the address to
/// <see cref="PasswordResetMailQueue"/>; <see cref="PasswordResetMailer"/> looks the account up,
/// issues the link and sends it in the background, so a known address answers as fast as an unknown
/// one (owner decision S5-13a (a)). Rate limits <see cref="RateLimitPolicies.PasswordReset"/> per
/// client IP (middleware) and <see cref="RateLimitPolicies.PasswordResetAddress"/> per address
/// (<see cref="IKeyedRateLimiter"/>, after binding).</para>
/// <para><c>POST /api/v1/auth/password-reset/complete</c> <c>{ token, newPassword }</c>: checks the new
/// password before it redeems the single-use token, so a refused password keeps the link. 204 sets
/// the password through Identity — which changes the security stamp, so the user's other sessions
/// end at their next stamp check (owner decision S5-13 3 (a)) —, confirms the address and ends a
/// lockout (decision 2 (a)), and revokes the user's other reset links. 400 <c>validation</c> at
/// <c>token</c> for an unknown, expired or used link, at <c>newPassword</c> for a refused password.</para>
/// <para>Both are state-changing and validated against antiforgery (SEC-02).</para>
/// </remarks>
internal static class PasswordResetEndpoints
{
    public const string RequestPath = "/password-reset";
    public const string CompletePath = "/password-reset/complete";

    /// <summary>Maps the endpoints below <c>/auth</c> of <paramref name="routes"/>, the versioned group.</summary>
    public static IEndpointRouteBuilder MapPasswordResetEndpoints(this IEndpointRouteBuilder routes)
    {
        var auth = routes.MapGroup("/auth").WithTags("Auth");

        auth.MapPost(RequestPath, Request)
            .AllowAnonymous()
            .RequireRateLimiting(RateLimitPolicies.PasswordReset)
            .WithName("PostPasswordReset")
            .WithSummary("Sends a password-reset link if an account exists for the address")
            .WithDescription(
                "200 { validHours } for a known and an unknown address alike; the e-mail is sent in the background. " +
                "400 validation at 'email'; 400 antiforgery; 429 rate-limit (per client IP and per address).")
            .Accepts<PasswordResetRequestBody>("application/json")
            .Produces<PasswordResetRequested>()
            .ProducesValidationProblem();

        auth.MapPost(CompletePath, CompleteAsync)
            .AllowAnonymous()
            .WithName("PostPasswordResetComplete")
            .WithSummary("Sets a new password with the token of the reset link")
            .WithDescription(
                "204 when the password is set. 400 validation at 'newPassword' for a refused password (the link stays valid), " +
                "at 'token' for an unknown, expired or used link; 400 antiforgery.")
            .Accepts<PasswordResetCompleteBody>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem();

        return routes;
    }

    private static Results<Ok<PasswordResetRequested>, ValidationProblem> Request(
        PasswordResetRequestBody body,
        ILookupNormalizer normalizer,
        IKeyedRateLimiter limiter,
        IAccountTokens tokens,
        PasswordResetMailQueue queue)
    {
        var address = body.Email?.Trim();
        if (string.IsNullOrEmpty(address))
        {
            return EmailProblem("The e-mail address is required.");
        }

        if (address.Length > RegistrationEndpoints.EmailMaxLength || !RegistrationEndpoints.IsEmailAddress(address))
        {
            return EmailProblem("The e-mail address is not valid.");
        }

        // Throws RateLimitExceededException (→ 429) for the 🔧 fourth request per hour for this address.
        limiter.Acquire(RateLimitPolicies.PasswordResetAddress, normalizer.NormalizeEmail(address));
        queue.Enqueue(address);

        return TypedResults.Ok(new PasswordResetRequested(
            IssuedAccountToken.HoursOf(tokens.LifetimeOf(AccountTokenPurpose.PasswordReset))));
    }

    private static async Task<Results<NoContent, ValidationProblem>> CompleteAsync(
        PasswordResetCompleteBody body,
        UserManager<User> users,
        IAccountTokens tokens,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(body.Token))
        {
            errors["token"] = ["The token is required."];
        }

        if (await PasswordErrorsAsync(users, body.NewPassword) is { Length: > 0 } passwordErrors)
        {
            errors["newPassword"] = passwordErrors;
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var redemption = await tokens.RedeemAsync(body.Token!, AccountTokenPurpose.PasswordReset, cancellationToken);
        var user = redemption is { Outcome: AccountTokenOutcome.Redeemed, UserId: { } userId }
            ? await users.FindByIdAsync(userId)
            : null;
        if (user is null || user.IsDeleted)
        {
            return TokenProblem();
        }

        // The link proves the mailbox: the address counts as confirmed, and a lockout ends
        // (decision 2 (a)). Saved with the new password in Identity's one update below.
        user.EmailConfirmed = true;
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;

        var identityToken = await users.GeneratePasswordResetTokenAsync(user);
        var reset = await users.ResetPasswordAsync(user, identityToken, body.NewPassword!);
        if (!reset.Succeeded)
        {
            // The password passed the same validators above; anything else is a server fault.
            throw new InvalidOperationException(
                "Setting the new password failed: " + string.Join(", ", reset.Errors.Select(e => e.Code)));
        }

        await tokens.RevokeAsync(user.Id, AccountTokenPurpose.PasswordReset, cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// The refusals of the password rule (🔧 minimum, at most 128 characters) and of Identity's
    /// password validators, which check the password alone, so no account is needed yet.
    /// </summary>
    private static async Task<string[]> PasswordErrorsAsync(UserManager<User> users, string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return ["The new password is required."];
        }

        var failures = new List<string>();
        foreach (var validator in users.PasswordValidators)
        {
            var result = await validator.ValidateAsync(users, new User(), password);
            failures.AddRange(result.Errors.Select(e => e.Description));
        }

        return [.. failures];
    }

    private static ValidationProblem EmailProblem(string message) =>
        TypedResults.ValidationProblem(
            new Dictionary<string, string[]>(StringComparer.Ordinal) { ["email"] = [message] },
            type: ProblemTypes.Validation);

    private static ValidationProblem TokenProblem() =>
        TypedResults.ValidationProblem(
            new Dictionary<string, string[]>(StringComparer.Ordinal) { ["token"] = ["The link is unknown, expired or already used."] },
            type: ProblemTypes.Validation);
}

/// <summary>Body of <c>POST /api/v1/auth/password-reset</c>.</summary>
/// <param name="Email">The address of the account; required.</param>
internal sealed record PasswordResetRequestBody(string? Email);

/// <summary>Answer of <c>POST /api/v1/auth/password-reset</c>, the same for every address.</summary>
/// <param name="ValidHours">The 🔧 lifetime of the link in whole hours (<c>{{hours}}</c> of <c>auth.reset.sent</c>).</param>
internal sealed record PasswordResetRequested(int ValidHours);

/// <summary>Body of <c>POST /api/v1/auth/password-reset/complete</c>.</summary>
/// <param name="Token">The token of the reset link.</param>
/// <param name="NewPassword">At least 🔧 12, at most 128 characters, no composition rules.</param>
internal sealed record PasswordResetCompleteBody(string? Token, string? NewPassword);
