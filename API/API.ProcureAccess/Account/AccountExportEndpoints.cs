// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

namespace API.ProcureAccess.Account;

/// <summary>
/// The personal data export <c>GET /api/v1/account/export</c> (SPEC R-AUTH-09, §8.4, SD14;
/// ARCHITECTURE §7.2, C79; PRV-07; S5-27): the signed-in user's own data as a JSON file. In the
/// host, because the address, its confirmation and the creation time come from the legacy
/// <see cref="User"/>; the memberships and contributor profiles come from IdentityAccess through
/// <see cref="IAccountDataExport"/>. It moves into IdentityAccess with the users (M3.1).
/// </summary>
/// <remarks>
/// <para><b>Only the user's own data (PRV-07).</b> No id of any kind and nothing of another user:
/// memberships name the organisation, the role and since when; profiles their display name, kind
/// and creation time.</para>
/// <para><b>Sections not built yet</b> (owner decision S5-27 1a): <c>preferences</c> is
/// <c>null</c> until S5-25 stores <c>UserPreferences</c>; <c>contributions</c> (step 6),
/// <c>createdByMe.criteriaLists</c> (S5-19) and <c>createdByMe.projects</c> (step 5) are empty
/// arrays. Each of these tasks fills its section and its part of PRV-07; the format stays
/// <c>formatVersion</c> 1. <c>account.createdAt</c> is <c>null</c> for accounts created before
/// S5-27 (decision 2a).</para>
/// <para><b>Answer.</b> Generated synchronously; <c>Cache-Control: no-store</c> on every answer of
/// the handler; the file name carries the UTC day of <c>generatedAt</c>, as the criteria export
/// does. Needs a sign-in (401), but no confirmed address: an unconfirmed account may read its data
/// too. A GET, so no antiforgery; no rate limit (SPEC §7.3 sets none; decision 4a).</para>
/// </remarks>
public static class AccountExportEndpoints
{
    public const string Path = "/account/export";

    public const int FormatVersion = 1;

    private const string NoStore = "no-store";

    private const string ContentType = "application/json";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    /// <summary>Maps the export on <paramref name="routes"/>, the versioned group.</summary>
    public static IEndpointRouteBuilder MapAccountExportEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet(Path, ExportAsync)
              .RequireAuthorization()
              .WithTags("Account")
              .WithName("GetAccountExport")
              .WithSummary("The signed-in user's personal data as a JSON file")
              .WithDescription(
                  "200 application/json as an attachment ProcureAccess_Daten_<YYYY-MM-DD>.json (SPEC §8.4), Cache-Control: no-store; " +
                  "401; 404 when the account no longer exists.")
              .Produces<AccountExportDocument>(StatusCodes.Status200OK, ContentType)
              .ProducesProblem(StatusCodes.Status404NotFound);
        return routes;
    }

    /// <summary>The file name of an export generated at <paramref name="generatedAt"/> (SPEC §8.4).</summary>
    public static string FileNameOf(DateTimeOffset generatedAt) =>
        $"ProcureAccess_Daten_{generatedAt.UtcDateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}.json";

    private static async Task<Results<FileContentHttpResult, ProblemHttpResult>> ExportAsync(
        ClaimsPrincipal principal,
        UserManager<User> users,
        IAccountDataExport accountData,
        TimeProvider clock,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        context.Response.Headers.CacheControl = NoStore;

        var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("A signed-in principal without a user id reached the account export.");
        var user = await users.FindByIdAsync(userId);
        if (user is null)
        {
            // Soft-deleted accounts are hidden by the legacy query filter.
            return TypedResults.Problem(statusCode: StatusCodes.Status404NotFound);
        }

        var part = await accountData.GetAsync(userId, cancellationToken);
        var generatedAt = clock.GetUtcNow();
        var document = new AccountExportDocument(
            generatedAt,
            FormatVersion,
            new AccountExportAccount(user.Email, user.CreatedAt, user.EmailConfirmed, Preferences: null),
            [.. part.Memberships.Select(m => new AccountExportMembership(m.Organization, OrganizationEndpoints.NameOf(m.Role), m.Since))],
            [.. part.ContributorProfiles.Select(p => new AccountExportContributorProfile(p.DisplayName, p.Kind, p.CreatedAt))],
            Contributions: [],
            new AccountExportCreatedByMe(CriteriaLists: [], Projects: []));

        return TypedResults.File(JsonSerializer.SerializeToUtf8Bytes(document, Json), ContentType, FileNameOf(generatedAt));
    }
}

/// <summary>The export file of SPEC §8.4, its properties in the order of the SPEC.</summary>
public sealed record AccountExportDocument(
    DateTimeOffset GeneratedAt,
    int FormatVersion,
    AccountExportAccount Account,
    IReadOnlyList<AccountExportMembership> Memberships,
    IReadOnlyList<AccountExportContributorProfile> ContributorProfiles,
    IReadOnlyList<object> Contributions,
    AccountExportCreatedByMe CreatedByMe);

/// <summary>The account: address, creation time (<c>null</c> before S5-27), confirmation, preferences (<c>null</c> until S5-25).</summary>
public sealed record AccountExportAccount(string? Email, DateTimeOffset? CreatedAt, bool EmailConfirmed, object? Preferences);

/// <summary>A membership: organisation name, role (<c>owner</c>, <c>editor</c>, <c>reader</c>) and since when.</summary>
public sealed record AccountExportMembership(string Organization, string Role, DateTimeOffset Since);

/// <summary>A contributor profile of the user.</summary>
public sealed record AccountExportContributorProfile(string DisplayName, string Kind, DateTimeOffset CreatedAt);

/// <summary>What the user created: criteria lists (S5-19) and procurement projects (step 5), empty until then.</summary>
public sealed record AccountExportCreatedByMe(IReadOnlyList<object> CriteriaLists, IReadOnlyList<object> Projects);
