// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;
using ProcureAccess.Modules.Catalog.Contracts;
using ProcureAccess.Modules.Configurator.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

namespace API.ProcureAccess.Account;

/// <summary>
/// The personal data export <c>GET /api/v1/account/export</c> (SPEC R-AUTH-09, §8.4, SD14;
/// ARCHITECTURE §7.2, C79; PRV-07; S5-27): the signed-in user's own data as a JSON file. In the
/// host, because the address, its confirmation and the creation time come from the legacy
/// <see cref="User"/>; the memberships and contributor profiles come from IdentityAccess through
/// <see cref="IAccountDataExport"/>, the settings through <see cref="IUserPreferences"/> (S5-25a),
/// the criteria lists the user created from Configurator through
/// <see cref="ICriteriaListDataExport"/> (S5-19). It moves into IdentityAccess with the users (M3.1).
/// </summary>
/// <remarks>
/// <para><b>Only the user's own data (PRV-07).</b> No id of any kind and nothing of another user:
/// memberships name the organisation, the role and since when; profiles their display name, kind
/// and creation time; criteria lists their name, organisation, release, selection and criteria
/// with the user's decisions (owner decision S5-19 4), for the organisations the user still
/// belongs to.</para>
/// <para><b>Settings</b> (S5-25a, owner decision S5-25 4): <c>preferences</c> holds the effective
/// values — the defaults for a user who never saved any —, spelled as the settings API does.</para>
/// <para><b>Sections not built yet</b> (owner decision S5-27 1a): <c>contributions</c> (step 6) and
/// <c>createdByMe.projects</c> (step 5) are empty arrays. Each of these tasks fills its section and its part of PRV-07; the format stays
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
        IUserPreferences preferences,
        ICriteriaListDataExport criteriaLists,
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
        var settings = await preferences.GetAsync(userId, cancellationToken);
        var lists = await criteriaLists.GetCreatedByAsync(userId, cancellationToken);
        var generatedAt = clock.GetUtcNow();
        var document = new AccountExportDocument(
            generatedAt,
            FormatVersion,
            new AccountExportAccount(user.Email, user.CreatedAt, user.EmailConfirmed, PreferencesOf(settings)),
            [.. part.Memberships.Select(m => new AccountExportMembership(m.Organization, OrganizationEndpoints.NameOf(m.Role), m.Since))],
            [.. part.ContributorProfiles.Select(p => new AccountExportContributorProfile(p.DisplayName, p.Kind, p.CreatedAt))],
            Contributions: [],
            new AccountExportCreatedByMe([.. lists.Select(ListOf)], Projects: []));

        return TypedResults.File(JsonSerializer.SerializeToUtf8Bytes(document, Json), ContentType, FileNameOf(generatedAt));
    }

    private static AccountExportCriteriaList ListOf(ExportedCriteriaList list) => new(
        list.Name,
        list.Organization,
        list.Release,
        list.CreatedAt,
        new AccountExportListSelection(list.Selection.UseCase, list.Selection.AppTypes, list.Selection.ProductParts, list.Selection.TestTypes),
        [.. list.Criteria.Select(c => new AccountExportListCriterion(
            c.Number, c.Title, c.IsIncluded, LevelName(c.RecommendedLevel), LevelName(c.EffectiveLevel), c.Weight, c.Note))]);

    private static AccountExportPreferences PreferencesOf(UserPreferencesValues settings) =>
        new(settings.Language, settings.Appearance, settings.Motion, settings.Contrast);

    /// <summary>The level names of SPEC §8.2: <c>A</c>, <c>B</c>, <c>Info</c>.</summary>
    private static string LevelName(PriorityLevel level) => level switch
    {
        PriorityLevel.A => "A",
        PriorityLevel.B => "B",
        PriorityLevel.Info => "Info",
        _ => throw new ArgumentOutOfRangeException(nameof(level), level, "Unknown priority level."),
    };
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

/// <summary>The account: address, creation time (<c>null</c> before S5-27), confirmation, settings.</summary>
public sealed record AccountExportAccount(string? Email, DateTimeOffset? CreatedAt, bool EmailConfirmed, AccountExportPreferences Preferences);

/// <summary>
/// The settings (S5-25a): language <c>de</c>; appearance <c>system</c> / <c>light</c> / <c>dark</c>;
/// motion <c>system</c> / <c>reduced</c>; contrast <c>system</c> / <c>high</c>.
/// </summary>
public sealed record AccountExportPreferences(string Language, string Appearance, string Motion, string Contrast);

/// <summary>A membership: organisation name, role (<c>owner</c>, <c>editor</c>, <c>reader</c>) and since when.</summary>
public sealed record AccountExportMembership(string Organization, string Role, DateTimeOffset Since);

/// <summary>A contributor profile of the user.</summary>
public sealed record AccountExportContributorProfile(string DisplayName, string Kind, DateTimeOffset CreatedAt);

/// <summary>What the user created: criteria lists (S5-19) and procurement projects (step 5, empty until then).</summary>
public sealed record AccountExportCreatedByMe(IReadOnlyList<AccountExportCriteriaList> CriteriaLists, IReadOnlyList<object> Projects);

/// <summary>A criteria list the user created, with its current content (owner decision S5-19 4).</summary>
public sealed record AccountExportCriteriaList(
    string Name,
    string Organization,
    string Release,
    DateTimeOffset CreatedAt,
    AccountExportListSelection Selection,
    IReadOnlyList<AccountExportListCriterion> Criteria);

/// <summary>The selection of a list by the names of its values.</summary>
public sealed record AccountExportListSelection(
    string? UseCase, IReadOnlyList<string> AppTypes, IReadOnlyList<string> ProductParts, IReadOnlyList<string> TestTypes);

/// <summary>
/// A criterion of a list with the user's decision: levels as <c>A</c>, <c>B</c>, <c>Info</c>; the
/// weight only while B applies; the number is <c>null</c> for a „Geprüft nach …“ criterion.
/// </summary>
public sealed record AccountExportListCriterion(
    string? Number, string Title, bool IsIncluded, string RecommendedLevel, string EffectiveLevel, int? Weight, string? Note);
