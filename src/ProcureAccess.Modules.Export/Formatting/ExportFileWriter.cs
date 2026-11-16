// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Text;
using ProcureAccess.Modules.Export.Documents;

namespace ProcureAccess.Modules.Export.Formatting;

/// <summary>A rendered export file, held in memory only and never stored (EXP-05).</summary>
internal sealed record ExportFile(byte[] Content, string ContentType, string FileName);

/// <summary>
/// Turns export content into a file: adds the generation time from <see cref="TimeProvider"/>,
/// renders it with the formatter of the format and names it as SPEC §8.3 says,
/// <c>ProcureAccess_Kriterien_&lt;release&gt;_&lt;YYYY-MM-DD&gt;.&lt;extension&gt;</c>; the file of a saved
/// list carries the slug of its name after „Kriterien“ (<see cref="SlugOf"/>, S5-21a). The date is
/// the UTC day, as for the usage events.
/// </summary>
internal sealed class ExportFileWriter(IEnumerable<IExportFormatter> formatters, TimeProvider time)
{
    /// <summary>Longest slug of a list name in a file name.</summary>
    public const int SlugMaxLength = 50;

    private readonly Dictionary<ExportFormat, IExportFormatter> _formatters = formatters.ToDictionary(f => f.Format);

    public ExportFile Write(ExportContent content, ExportFormat format, string? listName = null)
    {
        ArgumentNullException.ThrowIfNull(content);
        var formatter = _formatters.TryGetValue(format, out var found)
            ? found
            : throw new InvalidOperationException($"No formatter is registered for {format}.");

        var document = new ExportDocument(content, time.GetUtcNow());
        var day = document.GeneratedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        var slug = listName is null ? string.Empty : SlugOf(listName);
        var label = slug.Length == 0 ? string.Empty : slug + "_";
        return new ExportFile(
            formatter.Render(document),
            formatter.ContentType,
            $"ProcureAccess_Kriterien_{label}{content.ReleaseVersion}_{day}.{formatter.FileExtension}");
    }

    /// <summary>
    /// The slug of a list name for a file name (SPEC §8.3; rule confirmed by the owner, S5-21a):
    /// German umlauts and ß spelled out (ä → ae, ß → ss), other letters without their accents, every
    /// other character a hyphen, repeated hyphens merged, none at either end, at most
    /// <see cref="SlugMaxLength"/> characters; the case stays. Empty when nothing is left.
    /// </summary>
    internal static string SlugOf(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        var spelled = name
            .Replace("ä", "ae", StringComparison.Ordinal).Replace("ö", "oe", StringComparison.Ordinal)
            .Replace("ü", "ue", StringComparison.Ordinal).Replace("Ä", "Ae", StringComparison.Ordinal)
            .Replace("Ö", "Oe", StringComparison.Ordinal).Replace("Ü", "Ue", StringComparison.Ordinal)
            .Replace("ß", "ss", StringComparison.Ordinal).Replace("ẞ", "SS", StringComparison.Ordinal)
            .Normalize(NormalizationForm.FormD);

        var slug = new StringBuilder(spelled.Length);
        foreach (var c in spelled)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            var keep = char.IsAsciiLetterOrDigit(c);
            if (keep || (slug.Length > 0 && slug[^1] != '-'))
            {
                slug.Append(keep ? c : '-');
            }
        }

        var text = slug.ToString();
        if (text.Length > SlugMaxLength)
        {
            text = text[..SlugMaxLength];
        }

        return text.Trim('-');
    }
}
