// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT
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
    /// Registers the policies of <see cref="AuthorizationPolicies"/>. The e-mail confirmation gate
    /// (SEC-10) is registered by the IdentityAccess module since S6-10.
    /// </summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
                .AddPolicy(AuthorizationPolicies.ModerationReview, policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(CustomClaimTypes.Permission, Permissions.ModerationReview));
        return services;
    }
}
