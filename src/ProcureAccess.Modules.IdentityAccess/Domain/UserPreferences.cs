// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Domain;

/// <summary>
/// The settings of one user (SPEC R-SET-01, R-SET-02; ARCHITECTURE §5.2, C73; S5-25a): language,
/// appearance, motion and contrast, applied by the client on every device after the sign-in
/// (A11Y-06); since S6-19b also the state of the rating legend on the assessment tab S34b
/// (SPEC R-CAN-08, owner decision S6-19 Q1 (b)). One row per user, keyed by the user id (a foreign key onto the users since
/// S6-10b). A user without a row has <see cref="Defaults"/>.
/// </summary>
internal sealed class UserPreferences
{
    /// <summary>The only UI language of v1 (SPEC R-SET-01, SD12): German.</summary>
    public const string German = "de";

    /// <summary>Longest language code stored; v1 knows only <see cref="German"/>.</summary>
    public const int LanguageMaxLength = 8;

    private UserPreferences()
    {
    }

    public string UserId { get; private set; } = null!;

    public string Language { get; private set; } = German;

    public AppearancePreference Appearance { get; private set; }

    public MotionPreference Motion { get; private set; }

    public ContrastPreference Contrast { get; private set; }

    /// <summary>The rating legend on S34b: <see cref="AssessmentLegendPreference.Auto"/> until the user decides or assesses.</summary>
    public AssessmentLegendPreference AssessmentLegend { get; private set; }

    /// <summary>The settings of a user who never saved any: German, everything as in the system.</summary>
    public static UserPreferences Defaults(string userId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        return new UserPreferences
        {
            UserId = userId,
            Language = German,
            Appearance = AppearancePreference.System,
            Motion = MotionPreference.System,
            Contrast = ContrastPreference.System,
            AssessmentLegend = AssessmentLegendPreference.Auto,
        };
    }

    /// <summary>
    /// Replaces all four settings. Throws for a language other than <see cref="German"/> and for an
    /// undefined value; the endpoint refuses those before (and <see cref="ContrastPreference.High"/>,
    /// which waits for the reviewed high-contrast design H2, owner decision S5-25 3).
    /// </summary>
    public void Change(string language, AppearancePreference appearance, MotionPreference motion, ContrastPreference contrast)
    {
        if (language != German)
        {
            throw new ArgumentOutOfRangeException(nameof(language), language, "Only German is available.");
        }

        if (!Enum.IsDefined(appearance))
        {
            throw new ArgumentOutOfRangeException(nameof(appearance), appearance, "Unknown appearance.");
        }

        if (!Enum.IsDefined(motion))
        {
            throw new ArgumentOutOfRangeException(nameof(motion), motion, "Unknown motion setting.");
        }

        if (!Enum.IsDefined(contrast))
        {
            throw new ArgumentOutOfRangeException(nameof(contrast), contrast, "Unknown contrast setting.");
        }

        Language = language;
        Appearance = appearance;
        Motion = motion;
        Contrast = contrast;
    }

    /// <summary>Stores the state of the rating legend on S34b (S6-19b); throws for an undefined value.</summary>
    public void ChangeAssessmentLegend(AssessmentLegendPreference assessmentLegend)
    {
        if (!Enum.IsDefined(assessmentLegend))
        {
            throw new ArgumentOutOfRangeException(nameof(assessmentLegend), assessmentLegend, "Unknown legend state.");
        }

        AssessmentLegend = assessmentLegend;
    }
}

/// <summary>Light or dark appearance, or the system's (<c>prefers-color-scheme</c>; SPEC §7.5, C73).</summary>
internal enum AppearancePreference
{
    System,
    Light,
    Dark,
}

/// <summary>Motion as in the system (<c>prefers-reduced-motion</c>) or reduced (Handoff §1.7).</summary>
internal enum MotionPreference
{
    System,
    Reduced,
}

/// <summary>
/// Contrast as in the system (<c>prefers-contrast</c>) or high. <see cref="High"/> is stored as a
/// value but refused by the API until H2 (SPEC R-SET-01, R-SET-04; owner decision S5-25 3).
/// </summary>
internal enum ContrastPreference
{
    System,
    High,
}

/// <summary>
/// The rating legend <c>pa-rating-legend</c> on the assessment tab S34b (SPEC R-CAN-08, Handoff
/// §4.11; owner decisions S6-19 Q1 (b), S6-19b (a)): <see cref="Auto"/> — expanded from 600 px
/// until the user's first assessment, which the client stores as <see cref="Collapsed"/>; or what
/// the user chose by opening or closing it.
/// </summary>
internal enum AssessmentLegendPreference
{
    Auto,
    Expanded,
    Collapsed,
}
