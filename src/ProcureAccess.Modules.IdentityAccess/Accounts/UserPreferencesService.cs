// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;

namespace ProcureAccess.Modules.IdentityAccess.Accounts;

/// <summary>
/// Reads and stores a user's settings (SPEC R-SET-01; S5-25a). Reading never writes: a user
/// without a row gets the defaults, the row is created by the first save.
/// </summary>
internal sealed class UserPreferencesService(IdentityAccessDbContext db) : IUserPreferences
{
    public async Task<UserPreferencesValues> GetAsync(string userId, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        var stored = await db.UserPreferences.AsNoTracking()
                             .SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        return UserPreferenceSpelling.ValuesOf(stored ?? UserPreferences.Defaults(userId));
    }

    /// <summary>
    /// Replaces the settings of <paramref name="userId"/> with <paramref name="values"/>, which the
    /// endpoint checked. Two first saves of the same user at once: the one that loses the insert
    /// writes its values over the winner's, as a later save would (the last save wins).
    /// </summary>
    public async Task<UserPreferencesValues> SaveAsync(string userId, ParsedPreferences values, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        try
        {
            return await ChangeAsync(userId, values, cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            db.ChangeTracker.Clear();
            return await ChangeAsync(userId, values, cancellationToken);
        }
    }

    private async Task<UserPreferencesValues> ChangeAsync(string userId, ParsedPreferences values, CancellationToken cancellationToken)
    {
        var preferences = await db.UserPreferences.SingleOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (preferences is null)
        {
            preferences = UserPreferences.Defaults(userId);
            db.UserPreferences.Add(preferences);
        }

        preferences.Change(values.Language, values.Appearance, values.Motion, values.Contrast);
        await db.SaveChangesAsync(cancellationToken);
        return UserPreferenceSpelling.ValuesOf(preferences);
    }
}

/// <summary>Settings the endpoint has checked, ready for <see cref="UserPreferences.Change"/>.</summary>
internal sealed record ParsedPreferences(
    string Language, AppearancePreference Appearance, MotionPreference Motion, ContrastPreference Contrast);

/// <summary>
/// The spelling of the settings in the API and the export (SPEC §8.4, owner decision S5-25 4):
/// lower case, as the roles and kinds of the export.
/// </summary>
internal static class UserPreferenceSpelling
{
    public static UserPreferencesValues ValuesOf(UserPreferences preferences) => new(
        preferences.Language,
        NameOf(preferences.Appearance),
        NameOf(preferences.Motion),
        NameOf(preferences.Contrast));

    public static string NameOf(AppearancePreference appearance) => appearance switch
    {
        AppearancePreference.System => "system",
        AppearancePreference.Light => "light",
        AppearancePreference.Dark => "dark",
        _ => throw new ArgumentOutOfRangeException(nameof(appearance), appearance, "Unknown appearance."),
    };

    public static string NameOf(MotionPreference motion) => motion switch
    {
        MotionPreference.System => "system",
        MotionPreference.Reduced => "reduced",
        _ => throw new ArgumentOutOfRangeException(nameof(motion), motion, "Unknown motion setting."),
    };

    public static string NameOf(ContrastPreference contrast) => contrast switch
    {
        ContrastPreference.System => "system",
        ContrastPreference.High => "high",
        _ => throw new ArgumentOutOfRangeException(nameof(contrast), contrast, "Unknown contrast setting."),
    };

    /// <summary>The value spelled <paramref name="name"/> (exactly, lower case), if there is one.</summary>
    public static bool TryParse<TEnum>(string? name, out TEnum value)
        where TEnum : struct, Enum
    {
        foreach (var candidate in Enum.GetValues<TEnum>())
        {
            if (string.Equals(NameOfAny(candidate), name, StringComparison.Ordinal))
            {
                value = candidate;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static string NameOfAny<TEnum>(TEnum value)
        where TEnum : struct, Enum => value switch
        {
            AppearancePreference appearance => NameOf(appearance),
            MotionPreference motion => NameOf(motion),
            ContrastPreference contrast => NameOf(contrast),
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Not a setting."),
        };
}
