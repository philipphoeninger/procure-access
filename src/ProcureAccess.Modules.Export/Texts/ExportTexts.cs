// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json;
using ProcureAccess.Modules.Catalog.Contracts;

namespace ProcureAccess.Modules.Export.Texts;

/// <summary>
/// The German texts of the export files by UX copy key (owner decision O-S4-63 (a)): read from the
/// embedded <c>Texts/de.json</c>, which has the format of the client's <c>de.json</c> and is
/// checked against the UX copy by <c>scripts/check-i18n.mjs</c> (written with <c>--write</c>, never
/// by hand). German only, like the user interface; other languages follow with <c>Accept-Language</c>
/// (API-06).
/// </summary>
internal sealed class ExportTexts
{
    private const string GermanResource = "ProcureAccess.Modules.Export.Texts.de.json";

    private readonly Dictionary<string, string> _texts;

    private ExportTexts(Dictionary<string, string> texts)
    {
        var missing = ExportTextKeys.Required.Where(key => !texts.ContainsKey(key)).ToList();
        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"The export texts lack {string.Join(", ", missing)}; run node scripts/check-i18n.mjs --write.");
        }

        _texts = texts;
    }

    /// <summary>The text of <paramref name="key"/>.</summary>
    public string this[string key] =>
        _texts.TryGetValue(key, out var text)
            ? text
            : throw new KeyNotFoundException($"The export texts have no key {key}.");

    /// <summary>The label of a priority level (<c>criteria.priority.*</c>).</summary>
    public string Level(PriorityLevel level) => this[ExportTextKeys.Level(level)];

    /// <summary>Reads the German texts embedded in the module.</summary>
    public static ExportTexts LoadGerman()
    {
        using var stream = typeof(ExportTexts).Assembly.GetManifestResourceStream(GermanResource)
            ?? throw new InvalidOperationException($"The resource {GermanResource} is not embedded.");
        using var document = JsonDocument.Parse(stream);
        return FromJson(document.RootElement);
    }

    /// <summary>
    /// Texts from a JSON object in the format of <c>de.json</c>: nested objects whose path is the key;
    /// a property name may itself hold dots (<c>"menu.close"</c>, CLAUDE §9).
    /// </summary>
    public static ExportTexts FromJson(JsonElement root)
    {
        var texts = new Dictionary<string, string>(StringComparer.Ordinal);
        Flatten(root, prefix: null, texts);
        return new ExportTexts(texts);
    }

    private static void Flatten(JsonElement element, string? prefix, Dictionary<string, string> texts)
    {
        foreach (var property in element.EnumerateObject())
        {
            var key = prefix is null ? property.Name : $"{prefix}.{property.Name}";
            switch (property.Value.ValueKind)
            {
                case JsonValueKind.Object:
                    Flatten(property.Value, key, texts);
                    break;
                case JsonValueKind.String:
                    texts[key] = property.Value.GetString()!;
                    break;
                default:
                    throw new InvalidOperationException($"The export text {key} is not a text.");
            }
        }
    }
}
