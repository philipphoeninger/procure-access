// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.Platform.Contracts.Errors;

namespace API.ProcureAccess.Organizations;

/// <summary>
/// Organisations and members (SPEC R-ORG-01, R-ORG-03, R-ORG-04, §8.9 v2.27; ARCHITECTURE §7.1,
/// C80, C145; S5-16a). In the host, because the members' e-mail addresses come from the legacy
/// <see cref="User"/> (owner decision S5-16 4a); the rules live in the IdentityAccess module behind
/// <see cref="IOrganizationMembers"/>, and the endpoints move into it with the users (M3.1).
/// </summary>
/// <remarks>
/// <para>Every endpoint needs a sign-in (401). The organisation comes from the route and is checked
/// with <see cref="OrganizationRoleRequirement"/> before anything else: a non-member gets 404, a
/// member below the role 403 (<see cref="OrganizationAccess.StatusOf"/>; TEN-05). The id has no
/// route constraint, so a malformed one answers 400 <c>validation</c> like every other bad input
/// (API-01) instead of an unmatched route's 404.</para>
/// <para><c>POST /organizations</c> also needs a confirmed address
/// (<see cref="IdentityAccessPolicies.ConfirmedEmail"/>, SEC-10). A change that would leave the
/// organisation without an Owner answers 409 <c>last-owner</c> (TEN-11). The state-changing
/// endpoints are validated against antiforgery like every other (SEC-02).</para>
/// </remarks>
public static class OrganizationEndpoints
{
    public const string GroupPath = "/organizations";

    /// <summary>The organisation types and roles as the API names them (SPEC §8.9).</summary>
    private static readonly IReadOnlyDictionary<string, OrganizationType> Types =
        new Dictionary<string, OrganizationType>(StringComparer.Ordinal)
        {
            ["publicBody"] = OrganizationType.PublicBody,
            ["company"] = OrganizationType.Company,
            ["association"] = OrganizationType.Association,
        };

    private static readonly IReadOnlyDictionary<string, OrganizationRole> Roles =
        new Dictionary<string, OrganizationRole>(StringComparer.Ordinal)
        {
            ["owner"] = OrganizationRole.Owner,
            ["editor"] = OrganizationRole.Editor,
            ["reader"] = OrganizationRole.Reader,
        };

    /// <summary>Maps the endpoints below <c>/organizations</c> of <paramref name="routes"/>, the versioned group.</summary>
    public static IEndpointRouteBuilder MapOrganizationEndpoints(this IEndpointRouteBuilder routes)
    {
        var organizations = routes.MapGroup(GroupPath).WithTags("Organizations").RequireAuthorization();

        organizations.MapPost("", CreateAsync)
                     .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
                     .WithName("PostOrganization")
                     .WithSummary("Creates an organisation; the signed-in user becomes its Owner")
                     .WithDescription(
                         "201 { id }. 400 validation at 'name' (required, at most 120 characters) or 'type' " +
                         "(publicBody, company, association); 400 antiforgery; 401; 403 email-unconfirmed.")
                     .Accepts<CreateOrganizationBody>("application/json")
                     .Produces<CreatedOrganization>(StatusCodes.Status201Created)
                     .ProducesValidationProblem();

        organizations.MapGet("/{orgId}/members", GetMembersAsync)
                     .WithName("GetOrganizationMembers")
                     .WithSummary("The organisation and its members with their roles")
                     .WithDescription("200 for every member (Reader and up); 401; 404 for a non-member or an unknown organisation.")
                     .Produces<OrganizationMembersResponse>()
                     .ProducesProblem(StatusCodes.Status404NotFound);

        organizations.MapPatch("/{orgId}/members/{userId}", ChangeRoleAsync)
                     .WithName("PatchOrganizationMember")
                     .WithSummary("Gives a member another role (Owner only)")
                     .WithDescription(
                         "204. 400 validation at 'role' (owner, editor, reader); 400 antiforgery; 401; 403 below Owner; " +
                         "404 for a non-member, an unknown organisation or member; 409 last-owner.")
                     .Accepts<ChangeMemberRoleBody>("application/json")
                     .Produces(StatusCodes.Status204NoContent)
                     .ProducesValidationProblem()
                     .ProducesProblem(StatusCodes.Status409Conflict);

        organizations.MapDelete("/{orgId}/members/{userId}", RemoveAsync)
                     .WithName("DeleteOrganizationMember")
                     .WithSummary("Removes a member (Owner) or leaves the organisation (any member, own user id)")
                     .WithDescription(
                         "204. 400 antiforgery; 401; 403 when a member below Owner removes someone else; " +
                         "404 for a non-member, an unknown organisation or member; 409 last-owner.")
                     .Produces(StatusCodes.Status204NoContent)
                     .ProducesProblem(StatusCodes.Status409Conflict);

        return routes;
    }

    private static async Task<Results<Created<CreatedOrganization>, ValidationProblem>> CreateAsync(
        CreateOrganizationBody body,
        ClaimsPrincipal user,
        IOrganizationMembers organizations,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var name = body.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            errors["name"] = ["The name is required."];
        }
        else if (name.Length > OrganizationLimits.NameMaxLength)
        {
            errors["name"] = [$"The name has at most {OrganizationLimits.NameMaxLength} characters."];
        }

        if (body.Type is null || !Types.TryGetValue(body.Type, out var type))
        {
            errors["type"] = ["The type is required: publicBody, company or association."];
            type = default;
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var id = await organizations.CreateAsync(UserIdOf(user), name!, type, cancellationToken);
        return TypedResults.Created($"/api/v1{GroupPath}/{id}/members", new CreatedOrganization(id));
    }

    private static async Task<Results<Ok<OrganizationMembersResponse>, ProblemHttpResult>> GetMembersAsync(
        Guid orgId,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        IOrganizationMembers organizations,
        UserManager<User> users,
        CancellationToken cancellationToken)
    {
        if (await RefusedAsync(authorization, user, orgId, OrganizationRoleRequirement.Reader) is { } refused)
        {
            return refused;
        }

        var organization = await organizations.GetAsync(orgId, cancellationToken);
        if (organization is null)
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status404NotFound);
        }

        // One query for every address; a deleted account (legacy soft delete) has none.
        var userIds = organization.Members.Select(m => m.UserId).ToList();
        var addresses = await users.Users.AsNoTracking()
                                   .Where(u => userIds.Contains(u.Id))
                                   .Select(u => new { u.Id, u.Email })
                                   .ToDictionaryAsync(u => u.Id, u => u.Email, cancellationToken);

        var members = organization.Members
                                  .Select(m => new OrganizationMemberResponse(m.UserId, addresses.GetValueOrDefault(m.UserId), NameOf(m.Role)))
                                  .OrderByDescending(m => Roles[m.Role])
                                  .ThenBy(m => m.Email, StringComparer.OrdinalIgnoreCase)
                                  .ThenBy(m => m.UserId, StringComparer.Ordinal)
                                  .ToList();
        return TypedResults.Ok(new OrganizationMembersResponse(
            new OrganizationResponse(organization.Id, organization.Name, NameOf(organization.Type)),
            members));
    }

    private static async Task<Results<NoContent, ValidationProblem, ProblemHttpResult>> ChangeRoleAsync(
        Guid orgId,
        string userId,
        ChangeMemberRoleBody body,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        IOrganizationMembers organizations,
        CancellationToken cancellationToken)
    {
        if (await RefusedAsync(authorization, user, orgId, OrganizationRoleRequirement.Owner) is { } refused)
        {
            return refused;
        }

        if (body.Role is null || !Roles.TryGetValue(body.Role, out var role))
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>(StringComparer.Ordinal) { ["role"] = ["The role is required: owner, editor or reader."] },
                type: ProblemTypes.Validation);
        }

        return Answer(await organizations.ChangeRoleAsync(orgId, userId, role, cancellationToken));
    }

    private static async Task<Results<NoContent, ValidationProblem, ProblemHttpResult>> RemoveAsync(
        Guid orgId,
        string userId,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        IOrganizationMembers organizations,
        CancellationToken cancellationToken)
    {
        // Leaving needs only the membership (R-ORG-04); removing someone else is the Owner's (R-ORG-03).
        var requirement = string.Equals(userId, UserIdOf(user), StringComparison.Ordinal)
            ? OrganizationRoleRequirement.Reader
            : OrganizationRoleRequirement.Owner;
        if (await RefusedAsync(authorization, user, orgId, requirement) is { } refused)
        {
            return refused;
        }

        return Answer(await organizations.RemoveAsync(orgId, userId, cancellationToken));
    }

    private static Results<NoContent, ValidationProblem, ProblemHttpResult> Answer(MembershipChangeResult result) => result switch
    {
        MembershipChangeResult.Done => TypedResults.NoContent(),
        MembershipChangeResult.NotFound => TypedResults.Problem(statusCode: StatusCodes.Status404NotFound),
        MembershipChangeResult.LastOwner => TypedResults.Problem(
            statusCode: StatusCodes.Status409Conflict,
            type: ProblemTypes.LastOwner,
            title: "The organisation needs an Owner",
            detail: "Every organisation needs at least one Owner. Give another member the Owner role first."),
        _ => throw new ArgumentOutOfRangeException(nameof(result), result, "Unknown result."),
    };

    /// <summary>The 404 or 403 of a failed role check (TEN-05), or <c>null</c> when it passed.</summary>
    private static async Task<ProblemHttpResult?> RefusedAsync(
        IAuthorizationService authorization, ClaimsPrincipal user, Guid orgId, OrganizationRoleRequirement requirement)
    {
        var result = await authorization.AuthorizeAsync(user, new OrganizationReference(orgId), requirement);
        return OrganizationAccess.StatusOf(result) is { } status ? TypedResults.Problem(statusCode: status) : null;
    }

    /// <summary>The signed-in user's id; the group requires a sign-in, so it is always there.</summary>
    private static string UserIdOf(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("A signed-in principal without a user id reached an organisation endpoint.");

    private static string NameOf(OrganizationRole role) => Roles.Single(r => r.Value == role).Key;

    private static string NameOf(OrganizationType type) => Types.Single(t => t.Value == type).Key;
}

/// <summary>Body of <c>POST /api/v1/organizations</c>.</summary>
/// <param name="Name">Required, at most 120 characters after trimming (SPEC §7.2).</param>
/// <param name="Type"><c>publicBody</c>, <c>company</c> or <c>association</c>.</param>
public sealed record CreateOrganizationBody(string? Name, string? Type);

/// <summary>Answer of <c>POST /api/v1/organizations</c>.</summary>
public sealed record CreatedOrganization(Guid Id);

/// <summary>Body of <c>PATCH /api/v1/organizations/{orgId}/members/{userId}</c>.</summary>
/// <param name="Role"><c>owner</c>, <c>editor</c> or <c>reader</c>.</param>
public sealed record ChangeMemberRoleBody(string? Role);

/// <summary>Answer of <c>GET /api/v1/organizations/{orgId}/members</c>.</summary>
public sealed record OrganizationMembersResponse(OrganizationResponse Organization, IReadOnlyList<OrganizationMemberResponse> Members);

/// <summary>The organisation of the members page; the name is the H1 „Mitglieder von {{org}}“.</summary>
public sealed record OrganizationResponse(Guid Id, string Name, string Type);

/// <summary>One member: Owners first, then Editors and Readers, each by address.</summary>
/// <param name="UserId">The account's id, for the role change and the removal.</param>
/// <param name="Role"><c>owner</c>, <c>editor</c> or <c>reader</c>.</param>
/// <param name="Email"><c>null</c> for an account that was deleted.</param>
public sealed record OrganizationMemberResponse(string UserId, string? Email, string Role);
