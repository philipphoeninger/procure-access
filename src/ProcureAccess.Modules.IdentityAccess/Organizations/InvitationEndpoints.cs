// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ProcureAccess.Modules.IdentityAccess.Auth;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.Platform.Contracts.Email;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.RateLimiting;
using IEmailSender = ProcureAccess.Modules.Platform.Contracts.Email.IEmailSender;

namespace ProcureAccess.Modules.IdentityAccess.Organizations;

/// <summary>
/// Invitations into an organisation by e-mail (SPEC R-ORG-02, R-ORG-03, §8.9 v2.29; ARCHITECTURE
/// §7.1, C72, C80, C147; S5-17a). The endpoints add the members' and the signed-in user's
/// addresses from the <see cref="User"/>; the rules live behind
/// <see cref="IOrganizationInvitations"/>. In the module since S6-10.
/// </summary>
/// <remarks>
/// <para>Inviting, the pending list and revoking are the Owner's (owner decision S5-17 D4 (a)): the
/// route's organisation is checked first (404 non-member, 403 lesser role; TEN-05). Inviting sends
/// the e-mail synchronously; a failed send answers 503 <c>email-delivery</c> and keeps the invitation
/// (SPEC §7.6), so the Owner invites again, which replaces it (D3 (a)). Rate limit
/// <see cref="RateLimitPolicies.Invitation"/> per user.</para>
/// <para>The page of the link reads the invitation without a sign-in (D5 (a)): organisation and role
/// only; the invited address goes only to a signed-in user whose address differs, for
/// <c>org.invite.mismatch</c>. Accepting needs a sign-in and a confirmed address (SEC-10, D6 (a)) and
/// the invited address (403 <c>invitation-mismatch</c>, C80). The token travels in the body, never in
/// the URL, so it never reaches a request log.</para>
/// <para>Every endpoint here is state-changing or reads by a secret and is validated against
/// antiforgery like every other POST/DELETE (SEC-02); the client has the token of the session probe,
/// also when signed out.</para>
/// </remarks>
internal static class InvitationEndpoints
{
    public const string InvitationsGroupPath = "/invitations";

    /// <summary>Maps the endpoints below <c>/organizations</c> and <c>/invitations</c> of <paramref name="routes"/>, the versioned group.</summary>
    public static IEndpointRouteBuilder MapInvitationEndpoints(this IEndpointRouteBuilder routes)
    {
        var organizations = routes.MapGroup(OrganizationEndpoints.GroupPath).WithTags("Organizations").RequireAuthorization();

        organizations.MapPost("/{orgId}/invitations", InviteAsync)
                     .RequireRateLimiting(RateLimitPolicies.Invitation)
                     .WithName("PostOrganizationInvitation")
                     .WithSummary("Invites an e-mail address into the organisation with a role (Owner only)")
                     .WithDescription(
                         "201 { id, validDays }; an open invitation of the same address is replaced. 400 validation at 'email' " +
                         "(required, a valid address, not a member yet) or 'role' (owner, editor, reader); 400 antiforgery; 401; " +
                         "403 below Owner; 404 for a non-member; 429 rate-limit; 503 email-delivery (the invitation is kept).")
                     .Accepts<InviteBody>("application/json")
                     .Produces<CreatedInvitation>(StatusCodes.Status201Created)
                     .ProducesValidationProblem()
                     .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        organizations.MapGet("/{orgId}/invitations", GetPendingAsync)
                     .WithName("GetOrganizationInvitations")
                     .WithSummary("The open invitations of the organisation (Owner only)")
                     .WithDescription("200 [ { id, email, role, expiresAt } ] by address; 401; 403 below Owner; 404 for a non-member.")
                     .Produces<IReadOnlyList<PendingInvitationResponse>>()
                     .ProducesProblem(StatusCodes.Status404NotFound);

        organizations.MapDelete("/{orgId}/invitations/{invitationId}", RevokeAsync)
                     .WithName("DeleteOrganizationInvitation")
                     .WithSummary("Revokes an open invitation (Owner only)")
                     .WithDescription("204. 400 antiforgery; 401; 403 below Owner; 404 for a non-member or an invitation that is not open.")
                     .Produces(StatusCodes.Status204NoContent)
                     .ProducesProblem(StatusCodes.Status404NotFound);

        var invitations = routes.MapGroup(InvitationsGroupPath).WithTags("Organizations");

        invitations.MapPost("/view", ViewAsync)
                   .AllowAnonymous()
                   .WithName("PostInvitationView")
                   .WithSummary("What the token of an invitation link invites to")
                   .WithDescription(
                       "200 { organizationName, role, addressMatches, invitedEmail, alreadyMember, organizationId }: " +
                       "addressMatches is null when signed out; invitedEmail only for a signed-in user whose address differs; " +
                       "organizationId only for a member. 400 validation at 'token' for an unknown, expired, revoked or used link; 400 antiforgery.")
                   .Accepts<InvitationTokenBody>("application/json")
                   .Produces<InvitationViewResponse>()
                   .ProducesValidationProblem();

        invitations.MapPost("/accept", AcceptAsync)
                   .RequireAuthorization()
                   .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
                   .WithName("PostInvitationAccept")
                   .WithSummary("Accepts an invitation for the signed-in user")
                   .WithDescription(
                       "200 { organizationId }, also for a member who accepted the link before. 400 validation at 'token' for an " +
                       "unknown, expired, revoked or used link; 400 antiforgery; 401; 403 email-unconfirmed; " +
                       "403 invitation-mismatch with invitedEmail when the invitation is for another address.")
                   .Accepts<InvitationTokenBody>("application/json")
                   .Produces<AcceptedInvitation>()
                   .ProducesValidationProblem()
                   .ProducesProblem(StatusCodes.Status403Forbidden);

        return routes;
    }

    private static async Task<Results<Created<CreatedInvitation>, ValidationProblem, ProblemHttpResult>> InviteAsync(
        Guid orgId,
        InviteBody body,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        IOrganizationMembers members,
        IOrganizationInvitations invitations,
        UserManager<User> users,
        IEmailSender emails,
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        if (await OrganizationEndpoints.RefusedAsync(authorization, user, orgId, OrganizationRoleRequirement.Owner) is { } refused)
        {
            return refused;
        }

        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var email = body.Email?.Trim();
        if (string.IsNullOrEmpty(email))
        {
            errors["email"] = ["The e-mail address is required."];
        }
        else if (email.Length > RegistrationEndpoints.EmailMaxLength || !RegistrationEndpoints.IsEmailAddress(email))
        {
            errors["email"] = ["The e-mail address is not valid."];
        }

        if (body.Role is null || !OrganizationEndpoints.Roles.TryGetValue(body.Role, out var role))
        {
            errors["role"] = ["The role is required: owner, editor or reader."];
            role = default;
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var organization = await members.GetAsync(orgId, cancellationToken);
        if (organization is null)
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status404NotFound);
        }

        // Members see each other's addresses (owner decision S5-16 4a), so this tells an Owner nothing new.
        var memberIds = organization.Members.Select(m => m.UserId).ToList();
        var normalized = users.NormalizeEmail(email);
        if (await users.Users.AsNoTracking().AnyAsync(u => memberIds.Contains(u.Id) && u.NormalizedEmail == normalized, cancellationToken))
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>(StringComparer.Ordinal) { ["email"] = ["This address belongs to a member already."] },
                type: ProblemTypes.Validation);
        }

        var issued = await invitations.InviteAsync(orgId, email!, role, cancellationToken);
        if (issued is null)
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status404NotFound);
        }

        var link = ClientLinks.To(configuration, ClientLinks.InvitationPagePath + "?token=" + Uri.EscapeDataString(issued.Token));
        try
        {
            await emails.SendAsync(
                new OrganisationInvitationEmail(email!, issued.OrganizationName, InvitationRoleOf(role), link, issued.ValidDays),
                cancellationToken);
        }
        catch (EmailDeliveryException)
        {
            // Logged without the address (PRV-11); the invitation stays and is listed (SPEC §7.6).
            loggerFactory.CreateLogger(typeof(InvitationEndpoints))
                         .LogWarning("The invitation e-mail for organisation {OrganizationId} could not be sent.", orgId);
            return TypedResults.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                type: ProblemTypes.EmailDelivery,
                title: "E-mail not sent",
                detail: "The invitation is saved, but its e-mail could not be sent. Invite the address again in a few minutes.");
        }

        return TypedResults.Created(
            $"/api/v1{OrganizationEndpoints.GroupPath}/{orgId}/invitations",
            new CreatedInvitation(issued.Id, issued.ValidDays));
    }

    private static async Task<Results<Ok<List<PendingInvitationResponse>>, ProblemHttpResult>> GetPendingAsync(
        Guid orgId,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        IOrganizationInvitations invitations,
        CancellationToken cancellationToken)
    {
        if (await OrganizationEndpoints.RefusedAsync(authorization, user, orgId, OrganizationRoleRequirement.Owner) is { } refused)
        {
            return refused;
        }

        var pending = await invitations.GetPendingAsync(orgId, cancellationToken);
        if (pending is null)
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status404NotFound);
        }

        return TypedResults.Ok(pending
            .Select(i => new PendingInvitationResponse(i.Id, i.Email, OrganizationEndpoints.NameOf(i.Role), i.ExpiresAt))
            .ToList());
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> RevokeAsync(
        Guid orgId,
        Guid invitationId,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        IOrganizationInvitations invitations,
        CancellationToken cancellationToken)
    {
        if (await OrganizationEndpoints.RefusedAsync(authorization, user, orgId, OrganizationRoleRequirement.Owner) is { } refused)
        {
            return refused;
        }

        return await invitations.RevokeAsync(orgId, invitationId, cancellationToken)
            ? TypedResults.NoContent()
            : TypedResults.Problem(statusCode: StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<InvitationViewResponse>, ValidationProblem>> ViewAsync(
        InvitationTokenBody body,
        ClaimsPrincipal user,
        UserManager<User> users,
        IOrganizationInvitations invitations,
        CancellationToken cancellationToken)
    {
        var account = user.Identity?.IsAuthenticated == true ? await users.GetUserAsync(user) : null;
        var callerEmail = account is { IsDeleted: false } ? account.Email : null;

        var details = await invitations.FindAsync(body.Token, callerEmail, cancellationToken);
        if (details is null)
        {
            return TokenProblem();
        }

        return TypedResults.Ok(new InvitationViewResponse(
            details.OrganizationName,
            OrganizationEndpoints.NameOf(details.Role),
            details.AddressMatches,
            details.AddressMatches == false ? details.InvitedEmail : null,
            details.AlreadyMember,
            details.AlreadyMember ? details.OrganizationId : null));
    }

    private static async Task<Results<Ok<AcceptedInvitation>, ValidationProblem, ProblemHttpResult>> AcceptAsync(
        InvitationTokenBody body,
        ClaimsPrincipal user,
        UserManager<User> users,
        IOrganizationInvitations invitations,
        CancellationToken cancellationToken)
    {
        var account = await users.GetUserAsync(user);
        if (account is null || account.IsDeleted || string.IsNullOrEmpty(account.Email))
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status401Unauthorized);
        }

        var result = await invitations.AcceptAsync(body.Token, account.Id, account.Email, cancellationToken);
        return result.Outcome switch
        {
            InvitationAcceptanceOutcome.Accepted or InvitationAcceptanceOutcome.AlreadyMember =>
                TypedResults.Ok(new AcceptedInvitation(result.OrganizationId!.Value)),
            InvitationAcceptanceOutcome.AddressMismatch => TypedResults.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                type: ProblemTypes.InvitationMismatch,
                title: "The invitation is for another address",
                detail: "Sign in with the invited address or ask for a new invitation.",
                extensions: new Dictionary<string, object?> { ["invitedEmail"] = result.InvitedEmail }),
            InvitationAcceptanceOutcome.Invalid => TokenProblem(),
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.Outcome, "Unknown outcome."),
        };
    }

    private static ValidationProblem TokenProblem() =>
        TypedResults.ValidationProblem(
            new Dictionary<string, string[]>(StringComparer.Ordinal)
            {
                ["token"] = ["The invitation is unknown, expired, revoked or already used."],
            },
            type: ProblemTypes.Validation);

    private static InvitationRole InvitationRoleOf(OrganizationRole role) => role switch
    {
        OrganizationRole.Owner => InvitationRole.Owner,
        OrganizationRole.Editor => InvitationRole.Editor,
        OrganizationRole.Reader => InvitationRole.Reader,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown organisation role."),
    };
}

/// <summary>Body of <c>POST /api/v1/organizations/{orgId}/invitations</c>.</summary>
/// <param name="Email">Required, a valid address, at most 256 characters after trimming.</param>
/// <param name="Role"><c>owner</c>, <c>editor</c> or <c>reader</c>.</param>
internal sealed record InviteBody(string? Email, string? Role);

/// <summary>Answer of <c>POST /api/v1/organizations/{orgId}/invitations</c>.</summary>
/// <param name="Id">The invitation's id, for revoking it.</param>
/// <param name="ValidDays">The 🔧 lifetime in days, for <c>org.invite.sent</c>.</param>
internal sealed record CreatedInvitation(Guid Id, int ValidDays);

/// <summary>One open invitation on S21 (<c>org.invite.pending</c>).</summary>
internal sealed record PendingInvitationResponse(Guid Id, string Email, string Role, DateTimeOffset ExpiresAt);

/// <summary>Body of <c>POST /api/v1/invitations/view</c> and <c>/accept</c>: the token of the link.</summary>
internal sealed record InvitationTokenBody(string? Token);

/// <summary>Answer of <c>POST /api/v1/invitations/view</c>.</summary>
/// <param name="OrganizationName"><c>{{org}}</c> of <c>org.invite.accept.text</c>.</param>
/// <param name="Role"><c>owner</c>, <c>editor</c> or <c>reader</c>.</param>
/// <param name="AddressMatches"><c>null</c> when signed out, else whether the signed-in address is the invited one.</param>
/// <param name="InvitedEmail">Only when <paramref name="AddressMatches"/> is false: <c>{{email}}</c> of <c>org.invite.mismatch</c>.</param>
/// <param name="AlreadyMember">True for a member of the organisation, e.g. after accepting the link before.</param>
/// <param name="OrganizationId">Only for a member: the members page to open.</param>
internal sealed record InvitationViewResponse(
    string OrganizationName, string Role, bool? AddressMatches, string? InvitedEmail, bool AlreadyMember, Guid? OrganizationId);

/// <summary>Answer of <c>POST /api/v1/invitations/accept</c>: the organisation the user is a member of now.</summary>
internal sealed record AcceptedInvitation(Guid OrganizationId);
