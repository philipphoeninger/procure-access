// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;

namespace ProcureAccess.Modules.Platform.Contracts.Configuration;

/// <summary>
/// The one reader of the duration settings of the API (#456, TECH_DEBT TD-94): every 🔧
/// <see cref="TimeSpan"/> setting is read through it when the settings are checked at the start.
/// </summary>
/// <remarks>
/// <see cref="TimeSpan.Parse(string, IFormatProvider)"/> and the configuration binder read a bare
/// number as <b>days</b>, so <c>"15"</c> for a timeout of 15 seconds would mean 15 days. This reader
/// accepts only <c>hh:mm:ss</c> or <c>d.hh:mm:ss</c>, each with an optional fraction of a second
/// (<c>00:00:15</c>, <c>1.00:00:00</c>, <c>00:00:00.5</c>), and fails the start with the setting's
/// name otherwise. It takes the raw text, so the contracts need no configuration package.
/// </remarks>
public static class DurationSetting
{
    /// <summary>The forms a duration setting may have; hours 0 – 23, minutes and seconds 0 – 59.</summary>
    private static readonly string[] Forms =
    [
        @"hh\:mm\:ss",
        @"hh\:mm\:ss\.FFFFFFF",
        @"d\.hh\:mm\:ss",
        @"d\.hh\:mm\:ss\.FFFFFFF",
    ];

    /// <summary>
    /// Reads <paramref name="value"/>, the text of <paramref name="setting"/>; a missing, malformed,
    /// negative or zero duration fails with an <see cref="InvalidOperationException"/> that names the
    /// setting.
    /// </summary>
    /// <param name="value">The configured text, e.g. <c>configuration["Email:Smtp:Timeout"]</c>.</param>
    /// <param name="setting">The setting's full key, e.g. <c>Email:Smtp:Timeout</c>.</param>
    /// <param name="zeroAllowed">Whether <c>00:00:00</c> is a valid value of the setting.</param>
    public static TimeSpan Read(string? value, string setting, bool zeroAllowed = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(setting);
        if (!TimeSpan.TryParseExact(value?.Trim(), Forms, CultureInfo.InvariantCulture, out var duration)
            || (duration == TimeSpan.Zero && !zeroAllowed))
        {
            var expected = zeroAllowed ? "a duration of zero or more" : "a positive duration";
            throw new InvalidOperationException(
                $"{setting} must be {expected} in the form hh:mm:ss or d.hh:mm:ss, such as 00:15:00 or 1.00:00:00; it is '{value}'.");
        }

        return duration;
    }
}
