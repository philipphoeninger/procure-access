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

namespace ProcureAccess.Modules.Configurator.Configurations;

/// <summary>
/// The organisation's saved criteria lists (SPEC R-SAV-01, R-SAV-02, §8.10; ARCHITECTURE §7.2,
/// C153; S5-20a): save a draft in one call, read a list, list the organisation's lists for S14.
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
/// <para><b>Ids.</b> No route constraint on <c>{id}</c>, so a malformed id answers 400
/// <c>validation</c> like every other bad input (API-01), as the organisation endpoints do.</para>
/// </remarks>
internal static class ConfigurationEndpoints
{
    public const string GroupPath = "/configurations";

    /// <summary>As the anonymous export (SPEC §7.2 „body ≤ 1 MB“): the same draft plus a name.</summary>
    public const long MaxRequestBodySize = 1024 * 1024;

    private const string OrganizationField = "organizationId";
    private const string NameField = "name";

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
                          "200 for every member of the list's organisation; the texts come from GET /catalog/criteria with " +
                          "release=<releaseVersion>. 401; 404 for an unknown list or one of another organisation.")
                      .Produces<ConfigurationResponse>()
                      .ProducesProblem(StatusCodes.Status404NotFound);

        configurations.MapGet("", ListAsync)
                      .WithName("GetConfigurations")
                      .WithSummary("The saved lists of the organisation (Reader and up), for S14")
                      .WithDescription(
                          "200 [ { id, name, releaseVersion, createdAt, modifiedAt } ], the most recently changed first. " +
                          "400 validation at 'organizationId' without it; 401; 404 for a non-member.")
                      .Produces<IReadOnlyList<ConfigurationSummaryResponse>>()
                      .ProducesValidationProblem()
                      .ProducesProblem(StatusCodes.Status404NotFound);

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
        CancellationToken cancellationToken) =>
        await configurations.FindAsync(id, cancellationToken) is { } list
            ? TypedResults.Ok(list)
            : TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, type: ProblemTypes.NotFound);

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

    /// <summary>The 404 or 403 of a failed role check (TEN-05), or <c>null</c> when it passed.</summary>
    private static async Task<ProblemHttpResult?> RefusedAsync(
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
