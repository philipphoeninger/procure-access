// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Authorization.Policy;
using ProcureAccess.Modules.Platform.Contracts.Errors;

namespace API.ProcureAccess.Auth;

/// <summary>
/// The e-mail confirmation gate of the policy <c>IdentityAccessPolicies.ConfirmedEmail</c>
/// (ARCHITECTURE §9.1, C72; SEC-10; S5-12a): the signed-in user's address must be confirmed.
/// </summary>
public sealed class ConfirmedEmailRequirement : IAuthorizationRequirement;

/// <summary>
/// Reads the confirmation from the database on every gated request, not from a claim: a claim would
/// stay stale for up to the security-stamp interval after the user confirms in another browser. In
/// the host, because it needs <see cref="UserManager{TUser}"/> of the legacy <see cref="User"/>; it
/// moves into the IdentityAccess module with the users (M3.1).
/// </summary>
public sealed class ConfirmedEmailHandler(UserManager<User> users) : AuthorizationHandler<ConfirmedEmailRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, ConfirmedEmailRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var user = await users.GetUserAsync(context.User);
        if (user is { IsDeleted: false, EmailConfirmed: true })
        {
            context.Succeed(requirement);
        }
    }
}

/// <summary>
/// Answers a request refused only by <see cref="ConfirmedEmailRequirement"/> with 403 Problem
/// Details <c>email-unconfirmed</c> (SPEC §8.6), which the client shows as
/// <c>auth.confirm.required</c> with „erneut senden“. Every other result goes to the default handler
/// (401 without a sign-in, a plain 403 for another missing requirement).
/// </summary>
public sealed class ConfirmedEmailResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _default = new();

    public async Task HandleAsync(
        RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        var failed = authorizeResult.AuthorizationFailure?.FailedRequirements.ToList() ?? [];
        if (authorizeResult.Forbidden && failed.Count > 0 && failed.All(r => r is ConfirmedEmailRequirement))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            var problems = context.RequestServices.GetRequiredService<IProblemDetailsService>();
            await problems.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Type = ProblemTypes.EmailUnconfirmed,
                    Title = "E-mail address not confirmed",
                    Detail = "Confirm your e-mail address first; the confirmation e-mail can be sent again.",
                },
            });
            return;
        }

        await _default.HandleAsync(next, context, policy, authorizeResult);
    }
}
