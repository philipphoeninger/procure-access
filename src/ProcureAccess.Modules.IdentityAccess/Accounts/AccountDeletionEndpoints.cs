// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.IdentityAccess.Session;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.Sharing;

namespace ProcureAccess.Modules.IdentityAccess.Accounts;

/// <summary>
/// The account deletion <c>DELETE /api/v1/account</c> (SPEC R-AUTH-10, §8.13; ARCHITECTURE §7.2,
/// C79; TEN-11; S5-28a). In the module with the users since S6-10; the other modules delete their
/// data of an organisation that ends through <see cref="IOrganizationDataDeletion"/> of this
/// module's contracts, because IdentityAccess depends on no module (ARCHITECTURE §4.3).
/// </summary>
/// <remarks>
/// <para><b>Rule (TEN-11).</b> A user who is the only Owner of an organisation with other members
/// gets 409 <c>sole-owner</c> with the names of these organisations, and nothing changes. An
/// organisation whose only member the user is ends with the account: its criteria lists, share
/// links and invitations are deleted.</para>
/// <para><b>Order.</b> The modules share no transaction (ADR-014), so the steps run in an order in
/// which each can be repeated: the check (<see cref="IAccountDeletion.BeginAsync"/>), the lists and
/// links of the organisations that end, the memberships and organisations
/// (<see cref="IAccountDeletion.CompleteAsync"/>), the account last. A failure in between answers
/// 500, the account still signs in, and the next attempt finishes the deletion.</para>
/// <para><b>The account</b> (owner decision S5-28 2a): the row stays, soft-deleted (ARCHITECTURE
/// §9.3 never hard-deletes it), with a random address under the reserved domain <c>.invalid</c>,
/// without password hash and with a new security stamp, so the user's other sessions end at its
/// next check (the immediate end is SEC-08, S5-29). Its platform roles, claims, external logins
/// and Identity tokens are deleted and its lockout is reset in the same save (S6-10b).</para>
/// <para><b>Not yet</b> (owner decision S5-28 3): contributor profiles, published entries and
/// experience reports (steps 5 – 6) and procurement projects (step 5) join with their steps.</para>
/// <para><b>Answer.</b> 204 without the session cookie and with a new antiforgery token, as the
/// sign-out. Needs a sign-in (401) and the antiforgery token, but no confirmed address; no rate
/// limit (SPEC §7.3 sets none). The request has no body: the address the user types on S61 guards
/// against a slip, the antiforgery token against a forged request (owner decision S5-28 P2 (a)).</para>
/// </remarks>
internal static class AccountDeletionEndpoints
{
    public const string Path = "/account";

    /// <summary>The extension of a <c>sole-owner</c> problem: the names of the organisations that refuse.</summary>
    public const string OrganizationsExtension = "organizations";

    /// <summary>Reserved for addresses that never exist (RFC 2606).</summary>
    private const string DeletedDomain = "deleted.invalid";

    /// <summary>Maps the deletion on <paramref name="routes"/>, the versioned group.</summary>
    public static IEndpointRouteBuilder MapAccountDeletionEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapDelete(Path, DeleteAsync)
              .RequireAuthorization()
              .WithTags("Account")
              .WithName("DeleteAccount")
              .WithSummary("Deletes the signed-in user's account and ends the session")
              .WithDescription(
                  "204 without the session cookie and with a new XSRF-TOKEN. 409 sole-owner with organizations (names) while the " +
                  "user is the only Owner of an organisation with other members; 401; 400 antiforgery; 404 when the account no longer exists.")
              .Produces(StatusCodes.Status204NoContent)
              .ProducesProblem(StatusCodes.Status404NotFound)
              .ProducesProblem(StatusCodes.Status409Conflict);
        return routes;
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> DeleteAsync(
        ClaimsPrincipal principal,
        UserManager<User> users,
        SignInManager<User> signIn,
        IAccountDeletion accounts,
        IEnumerable<IOrganizationDataDeletion> moduleData,
        IShareLinks shareLinks,
        IdentityAccessDbContext db,
        TimeProvider clock,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("A signed-in principal without a user id reached the account deletion.");
        var user = await users.FindByIdAsync(userId);
        if (user is null)
        {
            // Soft-deleted accounts are hidden by the context's query filter.
            return TypedResults.Problem(statusCode: StatusCodes.Status404NotFound);
        }

        var start = await accounts.BeginAsync(userId, cancellationToken);
        if (start.IsRefused)
        {
            return SoleOwner(start.SoleOwnerOf);
        }

        foreach (var data in moduleData)
        {
            await data.DeleteOfOrganizationsAsync(start.OrganizationsToDelete, cancellationToken);
        }

        await shareLinks.DeleteOfOrganizationsAsync(start.OrganizationsToDelete, cancellationToken);

        // Checked again under the lock: the other Owner of an organisation may have left meanwhile.
        var soleOwnerOf = await accounts.CompleteAsync(userId, cancellationToken);
        if (soleOwnerOf.Count > 0)
        {
            return SoleOwner(soleOwnerOf);
        }

        var address = $"deleted-{Guid.NewGuid():N}@{DeletedDomain}";
        user.Email = address;
        user.UserName = address;
        user.EmailConfirmed = false;
        user.PasswordHash = null;
        user.IsDeleted = true;
        user.DeletedAt = clock.GetUtcNow().UtcDateTime;

        // Nothing of the account stays but the row (S6-10b, owner decision S6-10 4; S5-28a R5): its
        // platform roles, claims, external logins and Identity tokens go and the lockout is reset.
        // Marked on the context UserManager saves, so they go in the one save below.
        user.LockoutEnd = null;
        user.AccessFailedCount = 0;
        db.UserRoles.RemoveRange(await db.UserRoles.Where(r => r.UserId == userId).ToListAsync(cancellationToken));
        db.UserClaims.RemoveRange(await db.UserClaims.Where(c => c.UserId == userId).ToListAsync(cancellationToken));
        db.UserLogins.RemoveRange(await db.UserLogins.Where(l => l.UserId == userId).ToListAsync(cancellationToken));
        db.UserTokens.RemoveRange(await db.UserTokens.Where(t => t.UserId == userId).ToListAsync(cancellationToken));

        // Stores the changes above with the new stamp (and the normalised address and name).
        var result = await users.UpdateSecurityStampAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"The account could not be deleted: {string.Join(", ", result.Errors.Select(e => e.Code))}.");
        }

        await signIn.SignOutAsync();
        context.User = new ClaimsPrincipal(new ClaimsIdentity());
        AntiforgeryTokenCookie.Issue(context);
        return TypedResults.NoContent();
    }

    private static ProblemHttpResult SoleOwner(IReadOnlyList<string> organizations) => TypedResults.Problem(
        statusCode: StatusCodes.Status409Conflict,
        type: ProblemTypes.SoleOwner,
        title: "An organisation would be left without an Owner",
        detail: "The account is the only Owner of an organisation with other members. Give another member the Owner role first.",
        extensions: new Dictionary<string, object?> { [OrganizationsExtension] = organizations });
}
