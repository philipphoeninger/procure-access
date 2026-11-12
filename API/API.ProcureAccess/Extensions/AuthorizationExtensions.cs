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
    /// <summary>Registers the policies of <see cref="AuthorizationPolicies"/>.</summary>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
                .AddPolicy(AuthorizationPolicies.ModerationReview, policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(CustomClaimTypes.Permission, Permissions.ModerationReview));
        return services;
    }

    /// <summary>
    /// Creates the legacy <see cref="Roles.Member"/> role if it is missing (Development rebuild only,
    /// SEC-06). The platform roles and their permission come from the legacy migration
    /// <c>RenamePlatformRoles</c> (S4-25), so every environment has them.
    /// </summary>
    public static async Task AddMemberRoleAsync(this WebApplication app, RoleManager<IdentityRole> roleManager)
    {
        if (await roleManager.FindByNameAsync(Roles.Member) is null)
        {
            await roleManager.CreateAsync(new IdentityRole(Roles.Member));
        }
    }
}
