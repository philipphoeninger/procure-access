// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.Platform.Contracts.Errors;

namespace ProcureAccess.Modules.IdentityAccess.Accounts;

/// <summary>
/// The signed-in user's settings (SPEC R-SET-01, R-SET-02; ARCHITECTURE §5.2, C73; A11Y-06;
/// S5-25a): <c>GET</c> and <c>PUT /api/v1/account/preferences</c>. The client applies them after
/// the sign-in; the settings page S60 (S5-26) changes them.
/// </summary>
/// <remarks>
/// Both need a sign-in (401) but no confirmed address: SEC-10 does not gate settings. The PUT needs
/// antiforgery like every state-changing endpoint (SEC-02) and replaces all four settings — the
/// fifth, <c>assessmentLegend</c> (S6-19b), is optional and kept when it is missing —; it has no
/// rate limit (SPEC §7.3 sets none). The answers carry the default <c>Cache-Control: no-store</c>
/// of the versioned group (TEN-04). <c>contrast: "high"</c> is refused until the high-contrast design
/// H2 is reviewed (SPEC R-SET-01, R-SET-04; owner decision S5-25 3).
/// </remarks>
internal static class UserPreferencesEndpoints
{
    public const string Path = "/account/preferences";

    private const string LanguageField = "language";
    private const string AppearanceField = "appearance";
    private const string MotionField = "motion";
    private const string ContrastField = "contrast";
    private const string AssessmentLegendField = "assessmentLegend";

    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder routes)
    {
        routes.MapGet(Path, GetAsync)
              .RequireAuthorization()
              .WithTags("Account")
              .WithName("GetAccountPreferences")
              .WithSummary("The signed-in user's settings")
              .WithDescription(
                  "200 { language, appearance, motion, contrast, assessmentLegend }; the defaults ('de', 'system', 'system', " +
                  "'system', 'auto') when the user never saved any. 401.")
              .Produces<UserPreferencesValues>();

        routes.MapPut(Path, PutAsync)
              .RequireAuthorization()
              .WithTags("Account")
              .WithName("PutAccountPreferences")
              .WithSummary("Replaces the signed-in user's settings")
              .WithDescription(
                  "200 with the stored settings. Body { language: 'de', appearance: 'system' | 'light' | 'dark', motion: 'system' | " +
                  "'reduced', contrast: 'system', assessmentLegend?: 'auto' | 'expanded' | 'collapsed' }, the first four required, " +
                  "a missing assessmentLegend keeps the stored one; 'high' contrast is refused until its design is reviewed (H2). " +
                  "400 validation; 400 antiforgery; 401.")
              .Accepts<UserPreferencesBody>("application/json")
              .Produces<UserPreferencesValues>()
              .ProducesValidationProblem();

        return routes;
    }

    private static async Task<IResult> GetAsync(
        ClaimsPrincipal user, UserPreferencesService preferences, CancellationToken cancellationToken) =>
        TypedResults.Ok(await preferences.GetAsync(UserIdOf(user), cancellationToken));

    private static async Task<IResult> PutAsync(
        UserPreferencesBody body, ClaimsPrincipal user, UserPreferencesService preferences, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.Ordinal);
        if (body.Language != UserPreferences.German)
        {
            errors[LanguageField] = [$"The language must be '{UserPreferences.German}'."];
        }

        if (!UserPreferenceSpelling.TryParse(body.Appearance, out AppearancePreference appearance))
        {
            errors[AppearanceField] = ["The appearance must be 'system', 'light' or 'dark'."];
        }

        if (!UserPreferenceSpelling.TryParse(body.Motion, out MotionPreference motion))
        {
            errors[MotionField] = ["The motion setting must be 'system' or 'reduced'."];
        }

        if (!UserPreferenceSpelling.TryParse(body.Contrast, out ContrastPreference contrast))
        {
            errors[ContrastField] = ["The contrast setting must be 'system'."];
        }
        else if (contrast == ContrastPreference.High)
        {
            errors[ContrastField] = ["High contrast is not available yet."];
        }

        AssessmentLegendPreference? assessmentLegend = null;
        if (body.AssessmentLegend is not null)
        {
            if (UserPreferenceSpelling.TryParse(body.AssessmentLegend, out AssessmentLegendPreference parsedLegend))
            {
                assessmentLegend = parsedLegend;
            }
            else
            {
                errors[AssessmentLegendField] = ["The legend state must be 'auto', 'expanded' or 'collapsed'."];
            }
        }

        if (errors.Count > 0)
        {
            return TypedResults.ValidationProblem(errors, type: ProblemTypes.Validation);
        }

        var parsed = new ParsedPreferences(body.Language!, appearance, motion, contrast, assessmentLegend);
        return TypedResults.Ok(await preferences.SaveAsync(UserIdOf(user), parsed, cancellationToken));
    }

    private static string UserIdOf(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("A signed-in principal without a user id reached the preferences endpoint.");
}

/// <summary>
/// The body of the PUT: the settings spelled as in <see cref="UserPreferencesValues"/>; the first
/// four required, <paramref name="AssessmentLegend"/> optional (S6-19b).
/// </summary>
internal sealed record UserPreferencesBody(string? Language, string? Appearance, string? Motion, string? Contrast, string? AssessmentLegend = null);
