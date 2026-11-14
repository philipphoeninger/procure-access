// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT
using Microsoft.AspNetCore.Authorization.Policy;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

namespace API.ProcureAccess.Extensions;

/// <summary>Names of the claim-based authorization policies (ARCHITECTURE §9.1).</summary>
public static class AuthorizationPolicies
{
    /// <summary>
    /// Requires the permission <see cref="Permissions.ModerationReview"/> (roles Moderator and
    /// PlatformAdmin). Replaces <c>AuthApproverAttribute</c>, which answered 500 (S4-25, O-S4-85):
    /// a signed-in user without it gets 403, an anonymous request 401.
    /// </summary>
    public const string ModerationReview = "ModerationReview";
}

public static class AuthorizationExtensions
{
    /// <summary>
    /// Registers the policies of <see cref="AuthorizationPolicies"/> and the e-mail confirmation gate
    /// <see cref="IdentityAccessPolicies.ConfirmedEmail"/> (S5-12a, SEC-10) with its 403 answer.
    /// </summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
                .AddPolicy(AuthorizationPolicies.ModerationReview, policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(CustomClaimTypes.Permission, Permissions.ModerationReview))
                .AddPolicy(IdentityAccessPolicies.ConfirmedEmail, policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new ConfirmedEmailRequirement()));
        services.AddScoped<IAuthorizationHandler, ConfirmedEmailHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, ConfirmedEmailResultHandler>();
        return services;
    }
}
