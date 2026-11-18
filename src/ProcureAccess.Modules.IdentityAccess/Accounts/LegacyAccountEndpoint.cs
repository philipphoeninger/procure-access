// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using ProcureAccess.Modules.IdentityAccess.Domain;

namespace ProcureAccess.Modules.IdentityAccess.Accounts;

/// <summary>
/// The legacy account read <c>GET /api/me</c>, outside the versioned API: the client's identity
/// store still reads it while the legacy screens need it. In the module since S6-10 (formerly the
/// host's <c>IdentityUserEndpoints</c> with the legacy <c>UserService</c>); the route and the
/// answer are unchanged: the id and the address of the signed-in user, <c>null</c> without a
/// sign-in or for a deleted account. <c>GET /api/v1/account</c> is its replacement.
/// </summary>
internal static class LegacyAccountEndpoint
{
    public const string Path = "/me";

    /// <summary>Maps the read on <paramref name="routes"/>, the host's unversioned group <c>/api</c>.</summary>
    public static void Map(IEndpointRouteBuilder routes)
    {
        routes.MapGet(Path, async (ClaimsPrincipal principal, UserManager<User> users) =>
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                return null;
            }

            // A soft-deleted account is hidden by the context's filter.
            var user = await users.FindByIdAsync(userId);
            return user is null ? null : new LegacyAccountResponse(user.Id, user.Email!);
        });
    }
}

/// <summary>The answer of <c>GET /api/me</c> (formerly <c>UserDto</c> of the legacy MODELS project).</summary>
internal sealed record LegacyAccountResponse(string Id, string Email);
