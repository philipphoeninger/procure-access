// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using ProcureAccess.Modules.Configurator.Contracts.Drafts;
using ProcureAccess.Modules.Configurator.Domain;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.Platform.Contracts.Errors;
using ProcureAccess.Modules.Platform.Contracts.Sharing;

namespace ProcureAccess.Modules.Configurator.Configurations;

/// <summary>
/// The organisation's saved criteria lists (SPEC R-SAV-01 – R-SAV-03, R-SAV-06, R-LST-14, §8.10;
/// ARCHITECTURE §7.2, C153, C155): save a draft in one call, read a list, list the organisation's
/// lists for S14 (S5-20a); change a decision or the selection, delete a list, copy it to the active
/// release (S5-21a). The saved export is the Export module's (<c>POST …/{id}/exports</c>).
/// </summary>
/// <remarks>
/// <para><b>Sign-in and organisation.</b> Every endpoint needs a sign-in (401). Saving and listing
/// name their organisation in the body or the query (owner decision S5-20 B (a)), never in a header;
/// it is checked with <see cref="OrganizationRoleRequirement"/> before anything else — Editor to
/// save, Reader to list —, so a non-member gets 404 and a Reader who saves 403 (TEN-05). Reading
/// one list takes the organisation from the stored list: the organisation filter answers 404 for a
/// list of another organisation (TEN-01).</para>
/// <para><b>Saving.</b> Needs a confirmed address (<see cref="IdentityAccessPolicies.ConfirmedEmail"/>,
/// SEC-10) and antiforgery like every state-changing endpoint (SEC-02). The draft passes the check
/// of the anonymous export (<see cref="IConfigurationDraftCheck"/>, C153): the server resolves the
/// recommendations (EXP-07), only the active release is accepted (400 <c>release-mismatch</c>), and
/// an inactive use case is refused (CAT-18). The body is at most <see cref="MaxRequestBodySize"/>
/// bytes, like the export's (SPEC §7.2). SPEC §7.3 sets no rate limit.</para>
/// <para><b>Changes (S5-21a).</b> Changing, deleting and copying a list take the organisation from
/// the stored list and need Editor on it (404 non-member, 403 Reader), a confirmed address (SEC-10)
/// and antiforgery. A project's copy (C53, S6-12) is read and its criteria are changed here as on
/// any list (C51); its selection change, its deletion and its copy answer 409 <c>project-copy</c>.</para>
/// <para><b>Concurrency (API-03, API-04; S6-20b).</b> <c>GET /configurations/{id}</c> answers with
/// the list's <c>ETag</c>; the lists of S14 name theirs as <c>etag</c>. Changing a decision,
/// changing the selection and deleting need it in <c>If-Match</c>: 428 <c>precondition-required</c>
/// without the header (after the role check, before the validation), 409 <c>conflict</c> with
/// <c>editorDisplayName</c> for a stale one — also when another change lands while this one is
/// saved, and before <c>project-copy</c>. The answers of the two changes carry the new tag.
/// Saving a new list and copying one need none.</para>
/// <para><b>Ids.</b> No route constraint on <c>{id}</c>, so a malformed id answers 400
/// <c>validation</c> like every other bad input (API-01), as the organisation endpoints do.</para>
/// </remarks>
internal static class ConfigurationEndpoints
{
    public const string GroupPath = "/configurations";

    /// <summary>As the anonymous export (SPEC §7.2 „body ≤ 1 MB“): the same draft plus a name.</summary>
    public const long MaxRequestBodySize = 1024 * 1024;

    private const string IfMatchHeader = "If-Match";
    private const string OrganizationField = "organizationId";
    private const string NameField = "name";

    /// <summary>The one release a copy can go to (ARCHITECTURE C58).</summary>
    private const string ActiveRelease = "active";

    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder routes)
    {
        var configurations = routes.MapGroup(GroupPath).WithTags("Configurations").RequireAuthorization();

        configurations.MapPost("", SaveAsync)
                      .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
                      .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBodySize))
                      .WithName("PostConfiguration")
                      .WithSummary("Saves a criteria draft as a list of the organisation (Editor or Owner)")
                      .WithDescription(
                          "201 { id }. The body names the organisation, the name (required, at most 120 characters) and the draft " +
                          "in the shape of the anonymous export: the selection and every criterion of it once by its revision. " +
                          "400 validation (also an inactive use case), selection-excluded or release-mismatch (the release is no " +
                          "longer active); 400 antiforgery; 401; 403 email-unconfirmed or below Editor; 404 for a non-member or an " +
                          "unknown release; 413 above 1 MB.")
                      .Accepts<SaveConfigurationBody>("application/json")
                      .Produces<CreatedConfiguration>(StatusCodes.Status201Created)
                      .ProducesValidationProblem()
                      .ProducesProblem(StatusCodes.Status403Forbidden)
                      .ProducesProblem(StatusCodes.Status404NotFound)
                      .ProducesProblem(StatusCodes.Status413PayloadTooLarge);

        configurations.MapGet("/{id}", GetAsync)
                      .WithName("GetConfiguration")
                      .WithSummary("A saved list with its selection and the decision on each criterion")
                      .WithDescription(
                          "200 for every member of the list's organisation, also for a project's copy (procurementProjectId set); " +
                          "the texts come from GET /catalog/criteria with release=<releaseVersion>. 401; 404 for an unknown list or " +
                          "one of another organisation.")
                      .Produces<ConfigurationResponse>()
                      .ProducesProblem(StatusCodes.Status404NotFound);

        configurations.MapGet("", ListAsync)
                      .WithName("GetConfigurations")
                      .WithSummary("The saved lists of the organisation (Reader and up), for S14")
                      .WithDescription(
                          "200 [ { id, name, releaseVersion, createdAt, modifiedAt } ], the most recently changed first; the " +
                          "copies the projects own are not listed. " +
                          "400 validation at 'organizationId' without it; 401; 404 for a non-member.")
                      .Produces<IReadOnlyList<ConfigurationSummaryResponse>>()
                      .ProducesValidationProblem()
                      .ProducesProblem(StatusCodes.Status404NotFound);

        configurations.MapPatch("/{id}/criteria/{criterionRevisionId}", DecideAsync)
                      .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
                      .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBodySize))
                      .WithName("PatchConfigurationCriterion")
                      .WithSummary("Changes the decision on one criterion of a saved list (Editor or Owner)")
                      .WithDescription(
                          "204 with the list's new ETag. Needs If-Match with the ETag of the list. The body is the whole decision " +
                          "{ isIncluded, overrideLevel, overrideWeight, note }, keyed as a " +
                          "row of the save request; also on a list of an older release. 400 validation; 400 antiforgery; 401; " +
                          "403 email-unconfirmed or below Editor; 404 for an unknown list, one of another organisation or a " +
                          "criterion that is not on the list; 409 conflict with editorDisplayName for a stale If-Match; 428 " +
                          "precondition-required without If-Match.")
                      .Accepts<ConfigurationDecisionBody>("application/json")
                      .Produces(StatusCodes.Status204NoContent)
                      .ProducesValidationProblem()
                      .ProducesProblem(StatusCodes.Status403Forbidden)
                      .ProducesProblem(StatusCodes.Status404NotFound)
                      .ProducesProblem(StatusCodes.Status409Conflict)
                      .ProducesProblem(StatusCodes.Status428PreconditionRequired);

        configurations.MapPut("/{id}/selection", ChangeSelectionAsync)
                      .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
                      .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBodySize))
                      .WithName("PutConfigurationSelection")
                      .WithSummary("Replaces the selection of a saved list under its release (Editor or Owner)")
                      .WithDescription(
                          "200 { droppedDecisions: [ { criterionRevisionId, number, title } ] }: the criteria whose decisions " +
                          "left with the old selection; decisions of criteria that stay are kept. Only while the list's release " +
                          "is active. 400 validation (also an inactive use case), selection-excluded or release-mismatch (another " +
                          "release, or the list's release is no longer active); 400 antiforgery; 401; 403 email-unconfirmed or " +
                          "below Editor; 404 for an unknown list or one of another organisation; 409 conflict with editorDisplayName " +
                          "for a stale If-Match; 409 project-copy for a project's list, whose selection is fixed; 428 " +
                          "precondition-required without If-Match (the ETag of the list; the answer carries the new one).")
                      .Accepts<ChangeSelectionBody>("application/json")
                      .Produces<ChangedSelection>()
                      .ProducesValidationProblem()
                      .ProducesProblem(StatusCodes.Status403Forbidden)
                      .ProducesProblem(StatusCodes.Status404NotFound)
                      .ProducesProblem(StatusCodes.Status409Conflict);

        configurations.MapDelete("/{id}", DeleteAsync)
                      .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
                      .WithName("DeleteConfiguration")
                      .WithSummary("Deletes a saved list (Editor or Owner)")
                      .WithDescription(
                          "204. Needs If-Match with the ETag of the list. Project copies are not affected. 400 antiforgery; 401; " +
                          "403 email-unconfirmed or below Editor; 404 for an unknown list or one of another organisation; 409 " +
                          "conflict with editorDisplayName for a stale If-Match; 409 project-copy for a project's list, which goes " +
                          "only when the project's list is replaced; 428 precondition-required without If-Match.")
                      .Produces(StatusCodes.Status204NoContent)
                      .ProducesProblem(StatusCodes.Status403Forbidden)
                      .ProducesProblem(StatusCodes.Status404NotFound)
                      .ProducesProblem(StatusCodes.Status409Conflict);

        configurations.MapPost("/{id}/copies", CopyAsync)
                      .RequireAuthorization(IdentityAccessPolicies.ConfirmedEmail)
                      .WithMetadata(new RequestSizeLimitAttribute(MaxRequestBodySize))
                      .WithName("PostConfigurationCopy")
                      .WithSummary("Copies a saved list to the active catalog release (Editor or Owner)")
                      .WithDescription(
                          "201 { id, releaseVersion, droppedValues, notCarried }, Location of the new list. Body { release: " +
                          "\"active\" }. The new list has the same organisation and name; values the active release no longer " +
                          "allows are dropped and named, decisions move by criterion key or successor key, the others are named. " +
                          "The source list is unchanged. 400 validation (another release, or the list is already on the active " +
                          "release) or selection-excluded (nothing complete is left); 400 antiforgery; 401; 403 email-unconfirmed " +
                          "or below Editor; 404 for an unknown list, one of another organisation, or without an active release; " +
                          "409 project-copy for a project's list, which is never a source.")
                      .Accepts<CopyConfigurationBody>("application/json")
                      .Produces<CopiedConfiguration>(StatusCodes.Status201Created)
                      .ProducesValidationProblem()
                      .ProducesProblem(StatusCodes.Status403Forbidden)
                      .ProducesProblem(StatusCodes.Status404NotFound)
                      .ProducesProblem(StatusCodes.Status409Conflict);

        return routes;
    }

    private static async Task<IResult> SaveAsync(
        SaveConfigurationBody body,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        ConfigurationService configurations,
        CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        var name = body.Name?.Trim();
        if (string.IsNullOrEmpty(name))
        {
            errors[NameField] = ["The name is required."];
        }
        else if (name.Length > CriteriaConfiguration.NameMaxLength)
        {
            errors[NameField] = [$"The name has at most {CriteriaConfiguration.NameMaxLength} characters."];
        }

        if (body.OrganizationId is not { } organizationId)
        {
            errors[OrganizationField] = ["The organisation is required."];
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        if (await RefusedAsync(authorization, user, organizationId, OrganizationRoleRequirement.Editor) is { } refused)
        {
            return refused;
        }

        var result = await configurations.CheckAsync(new CriteriaDraft(body.Selection, body.Criteria), cancellationToken);
        switch (result.Outcome)
        {
            case DraftCheckOutcome.Checked when errors.Count == 0:
                var id = await configurations.CreateAsync(organizationId, name!, UserIdOf(user), result.Draft!, cancellationToken);
                return TypedResults.Created($"/api/v1{GroupPath}/{id}", new CreatedConfiguration(id));
            case DraftCheckOutcome.Checked:
                return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
            case DraftCheckOutcome.Invalid:
                // The name's error and the draft's field errors in one answer, as the export adds its format.
                return TypedResults.ValidationProblem(
                    new Dictionary<string, string[]>(result.Errors.Concat(errors), StringComparer.Ordinal), type: ProblemTypes.Validation);
            case DraftCheckOutcome.SelectionExcluded:
                return TypedResults.ValidationProblem(result.Errors, detail: result.Detail, type: ProblemTypes.SelectionExcluded);
            case DraftCheckOutcome.ReleaseNotActive:
                return TypedResults.ValidationProblem(result.Errors, detail: result.Detail, type: ProblemTypes.ReleaseMismatch);
            case DraftCheckOutcome.ReleaseNotFound:
                return TypedResults.Problem(detail: result.Detail, statusCode: StatusCodes.Status404NotFound, type: ProblemTypes.NotFound);
            default:
                throw new InvalidOperationException($"Unknown outcome {result.Outcome} of the draft check.");
        }
    }

    private static async Task<Results<Ok<ConfigurationResponse>, ProblemHttpResult>> GetAsync(
        Guid id,
        ConfigurationService configurations,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        if (await configurations.FindAsync(id, cancellationToken) is not { } list)
        {
            return TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, type: ProblemTypes.NotFound);
        }

        response.Headers.ETag = list.Etag;
        return TypedResults.Ok(list);
    }

    private static async Task<IResult> ListAsync(
        [FromQuery] Guid? organizationId,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        ConfigurationService configurations,
        CancellationToken cancellationToken)
    {
        if (organizationId is not { } id)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>(StringComparer.Ordinal) { [OrganizationField] = ["The organisation is required."] },
                type: ProblemTypes.Validation);
        }

        if (await RefusedAsync(authorization, user, id, OrganizationRoleRequirement.Reader) is { } refused)
        {
            return refused;
        }

        return TypedResults.Ok(await configurations.ListAsync(id, cancellationToken));
    }

    private static async Task<IResult> DecideAsync(
        Guid id,
        int criterionRevisionId,
        ConfigurationDecisionBody body,
        [FromHeader(Name = IfMatchHeader)] string? ifMatch,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        ConfigurationService configurations,
        IUserDirectory users,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        if (await RefusedForListAsync(authorization, user, configurations, id, cancellationToken) is { } refused)
        {
            return refused;
        }

        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            return PreconditionRequired();
        }

        var result = await configurations.DecideAsync(
            id, criterionRevisionId, body, new ListEdit(UserIdOf(user), ifMatch), cancellationToken);
        return await AnswerAsync(result, TypedResults.NoContent(), users, response, cancellationToken);
    }

    private static async Task<IResult> ChangeSelectionAsync(
        Guid id,
        ChangeSelectionBody body,
        [FromHeader(Name = IfMatchHeader)] string? ifMatch,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        ConfigurationService configurations,
        IUserDirectory users,
        HttpResponse response,
        CancellationToken cancellationToken)
    {
        if (await RefusedForListAsync(authorization, user, configurations, id, cancellationToken) is { } refused)
        {
            return refused;
        }

        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            return PreconditionRequired();
        }

        var result = await configurations.ChangeSelectionAsync(
            id, body.Selection, new ListEdit(UserIdOf(user), ifMatch), cancellationToken);
        return await AnswerAsync(result, TypedResults.Ok(result.Value), users, response, cancellationToken);
    }

    private static async Task<IResult> DeleteAsync(
        Guid id,
        [FromHeader(Name = IfMatchHeader)] string? ifMatch,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        ConfigurationService configurations,
        IShareLinks links,
        IUserDirectory users,
        CancellationToken cancellationToken)
    {
        if (await RefusedForListAsync(authorization, user, configurations, id, cancellationToken) is { } refused)
        {
            return refused;
        }

        if (string.IsNullOrWhiteSpace(ifMatch))
        {
            return PreconditionRequired();
        }

        var result = await configurations.DeleteAsync(id, new ListEdit(UserIdOf(user), ifMatch), cancellationToken);
        if (result.Outcome == EditOutcome.Conflict)
        {
            return await ConflictAsync(users, result.EditorUserId, cancellationToken);
        }

        if (result.Outcome != EditOutcome.Done)
        {
            return ProblemOf(result);
        }

        // After the list, in Platform's own context: links that outlive a failure here open nothing.
        await links.DeleteOfResourceAsync(ShareableResourceType.Configuration, id, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<IResult> CopyAsync(
        Guid id,
        CopyConfigurationBody body,
        ClaimsPrincipal user,
        IAuthorizationService authorization,
        ConfigurationService configurations,
        CancellationToken cancellationToken)
    {
        if (await RefusedForListAsync(authorization, user, configurations, id, cancellationToken) is { } refused)
        {
            return refused;
        }

        if (body.Release != ActiveRelease)
        {
            return TypedResults.ValidationProblem(
                new Dictionary<string, string[]>(StringComparer.Ordinal) { ["release"] = [$"The release must be '{ActiveRelease}'."] },
                type: ProblemTypes.Validation);
        }

        var result = await configurations.CopyToActiveReleaseAsync(id, UserIdOf(user), cancellationToken);
        return result.Outcome == EditOutcome.Done
            ? TypedResults.Created($"/api/v1{GroupPath}/{result.Value!.Id}", result.Value)
            : ProblemOf(result);
    }

    /// <summary>
    /// The role check of a change to a stored list (TEN-05): the organisation is the list's, read
    /// through the filter — 404 for an unknown list or one of another organisation (TEN-01) —, then
    /// Editor on it — 403 for a Reader. <c>null</c> when it passed.
    /// </summary>
    internal static async Task<IResult?> RefusedForListAsync(
        IAuthorizationService authorization,
        ClaimsPrincipal user,
        ConfigurationService configurations,
        Guid id,
        CancellationToken cancellationToken) =>
        await configurations.OrganizationOfAsync(id, cancellationToken) is { } organizationId
            ? await RefusedAsync(authorization, user, organizationId, OrganizationRoleRequirement.Editor)
            : TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, type: ProblemTypes.NotFound);

    /// <summary>
    /// The answer of a change that needed <c>If-Match</c>: <paramref name="done"/> with the list's
    /// new <c>ETag</c>, 409 <c>conflict</c> with the editor's name, or <see cref="ProblemOf{T}"/>.
    /// </summary>
    private static async Task<IResult> AnswerAsync<T>(
        EditResult<T> result, IResult done, IUserDirectory users, HttpResponse response, CancellationToken cancellationToken)
    {
        switch (result.Outcome)
        {
            case EditOutcome.Done:
                response.Headers.ETag = result.Etag;
                return done;
            case EditOutcome.Conflict:
                return await ConflictAsync(users, result.EditorUserId, cancellationToken);
            default:
                return ProblemOf(result);
        }
    }

    /// <summary>
    /// 428 <c>precondition-required</c>: a change of a saved list without <c>If-Match</c> (API-03;
    /// owner decision S6-20 D3 (a)).
    /// </summary>
    private static ProblemHttpResult PreconditionRequired() =>
        TypedResults.Problem(
            detail: "Send the ETag of the version you read in If-Match.",
            statusCode: StatusCodes.Status428PreconditionRequired,
            type: ProblemTypes.PreconditionRequired);

    /// <summary>
    /// 409 <c>conflict</c> for a stale <c>If-Match</c> (API-03), with the display name of who made
    /// the stored version as <c>editorDisplayName</c> for S37 — <c>null</c> when the row names
    /// nobody or the account is gone. The caller has checked that the request's user may edit the
    /// list (<see cref="IUserDirectory"/>).
    /// </summary>
    private static async Task<ProblemHttpResult> ConflictAsync(
        IUserDirectory users, string? editorUserId, CancellationToken cancellationToken)
    {
        string? name = null;
        if (editorUserId is not null)
        {
            (await users.GetDisplayNamesAsync([editorUserId], cancellationToken)).TryGetValue(editorUserId, out name);
        }

        return TypedResults.Problem(
            detail: "The content has changed since you read it.",
            statusCode: StatusCodes.Status409Conflict,
            type: ProblemTypes.Conflict,
            extensions: new Dictionary<string, object?>(StringComparer.Ordinal) { ["editorDisplayName"] = name });
    }

    private static IResult ProblemOf<T>(EditResult<T> result) => result.Outcome switch
    {
        EditOutcome.NotFound => TypedResults.Problem(
            detail: result.Detail, statusCode: StatusCodes.Status404NotFound, type: ProblemTypes.NotFound),
        EditOutcome.Invalid => TypedResults.ValidationProblem(result.Errors, detail: result.Detail, type: ProblemTypes.Validation),
        EditOutcome.SelectionExcluded => TypedResults.ValidationProblem(
            result.Errors, detail: result.Detail, type: ProblemTypes.SelectionExcluded),
        EditOutcome.ReleaseMismatch => TypedResults.ValidationProblem(
            result.Errors, detail: result.Detail, type: ProblemTypes.ReleaseMismatch),
        EditOutcome.ProjectCopy => ProjectCopyConflict(result.Detail),
        _ => throw new InvalidOperationException($"No problem for outcome {result.Outcome}."),
    };

    /// <summary>
    /// 409 <c>project-copy</c>: the action is for organisation lists, and the list is a project's
    /// copy (C53; owner decision S6-12 D2 (a)).
    /// </summary>
    internal static ProblemHttpResult ProjectCopyConflict(string? detail) =>
        TypedResults.Problem(detail: detail, statusCode: StatusCodes.Status409Conflict, type: ProblemTypes.ProjectCopy);

    /// <summary>The 404 or 403 of a failed role check (TEN-05), or <c>null</c> when it passed.</summary>
    internal static async Task<ProblemHttpResult?> RefusedAsync(
        IAuthorizationService authorization, ClaimsPrincipal user, Guid organizationId, OrganizationRoleRequirement requirement)
    {
        var result = await authorization.AuthorizeAsync(user, new OrganizationReference(organizationId), requirement);
        return OrganizationAccess.StatusOf(result) is { } status ? TypedResults.Problem(statusCode: status) : null;
    }

    /// <summary>The signed-in user's id; the group requires a sign-in, so it is always there.</summary>
    private static string UserIdOf(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("A signed-in principal without a user id reached a configuration endpoint.");
}
