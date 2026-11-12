// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;

namespace ProcureAccess.Modules.Export.Formatting;

/// <summary>
/// 🔧 The address of the tool that the sheet „Info“ of an XLSX names (SPEC §8.3), from
/// <c>Export:ToolUrl</c> of <c>appsettings.json</c> (owner decision O-S4-64 (a):
/// <c>https://procureaccess.de</c>).
/// </summary>
internal sealed record ExportToolUrl(Uri Value)
{
    public const string SettingName = "Export:ToolUrl";

    /// <summary>Reads the address; a missing one or one that is not an absolute http(s) URL fails the start.</summary>
    public static ExportToolUrl From(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        if (!Uri.TryCreate(configuration[SettingName], UriKind.Absolute, out var url)
            || (url.Scheme != Uri.UriSchemeHttps && url.Scheme != Uri.UriSchemeHttp))
        {
            throw new InvalidOperationException($"{SettingName} must be an absolute http(s) URL (SPEC §8.3).");
        }

        return new ExportToolUrl(url);
    }
}
