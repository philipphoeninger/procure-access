// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Workspace.Candidates;

/// <summary>The field checks of the candidate and evidence bodies (SPEC §7.1, §7.2; API-01).</summary>
internal static class InputCheck
{
    /// <summary>
    /// <paramref name="value"/> trimmed, or <c>null</c> when it is <c>null</c>; a value longer than
    /// <paramref name="maxLength"/> after trimming adds an error at <paramref name="field"/>.
    /// </summary>
    public static string? Text(string? value, string field, int maxLength, Dictionary<string, string[]> errors)
    {
        var trimmed = value?.Trim();
        if (trimmed?.Length > maxLength)
        {
            errors[field] = [$"At most {maxLength} characters."];
        }

        return trimmed;
    }

    /// <summary>
    /// As <see cref="Text"/>; a value that is not empty must also be an absolute <c>https</c>
    /// address (SPEC §7.2 „https:// only“, <c>errors.field.url</c>).
    /// </summary>
    public static string? Url(string? value, string field, int maxLength, Dictionary<string, string[]> errors)
    {
        var trimmed = Text(value, field, maxLength, errors);
        if (!string.IsNullOrEmpty(trimmed) && !errors.ContainsKey(field)
            && (trimmed.Any(char.IsWhiteSpace)
                || !Uri.TryCreate(trimmed, UriKind.Absolute, out var uri)
                || uri.Scheme != Uri.UriSchemeHttps
                || string.IsNullOrEmpty(uri.Host)))
        {
            errors[field] = ["An address that starts with https:// is required."];
        }

        return trimmed;
    }
}
