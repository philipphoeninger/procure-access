// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json;

namespace ProcureAccess.Modules.Platform.Email;

/// <summary>
/// The German texts of the e-mails by UX copy key (§5.17 <c>email.*</c> and the role labels the
/// invitation names): read from the embedded <c>Email/Texts/de.json</c>, which
/// <c>scripts/check-i18n.mjs --write</c> fills from the UX copy (target <c>EMAIL_AREAS</c>, never by
/// hand). The start fails when a key of <see cref="Required"/> is missing.
/// </summary>
internal sealed class EmailTexts
{
    /// <summary>The keys the composer reads; a new message type adds its keys here.</summary>
    public static readonly IReadOnlyList<string> Required =
    [
        "email.footer",
        "email.confirm.subject",
        "email.confirm.body",
        "email.alreadyRegistered.subject",
        "email.alreadyRegistered.body",
        "email.reset.subject",
        "email.reset.body",
        "email.invitation.subject",
        "email.invitation.body",
        "org.members.role.owner",
        "org.members.role.editor",
        "org.members.role.reader",
    ];

    private const string GermanResource = "ProcureAccess.Modules.Platform.Email.Texts.de.json";

    private readonly Dictionary<string, string> _texts;

    private EmailTexts(Dictionary<string, string> texts)
    {
        var missing = Required.Where(key => !texts.ContainsKey(key)).ToList();
        if (missing.Count > 0)
        {
            throw new InvalidOperationException(
                $"The e-mail texts lack {string.Join(", ", missing)}; run node scripts/check-i18n.mjs --write.");
        }

        _texts = texts;
    }

    /// <summary>The text of <paramref name="key"/>.</summary>
    public string this[string key] =>
        _texts.TryGetValue(key, out var text)
            ? text
            : throw new KeyNotFoundException($"The e-mail texts have no key {key}.");

    /// <summary>Reads the German texts embedded in the module.</summary>
    public static EmailTexts LoadGerman()
    {
        using var stream = typeof(EmailTexts).Assembly.GetManifestResourceStream(GermanResource)
            ?? throw new InvalidOperationException($"The resource {GermanResource} is not embedded.");
        using var document = JsonDocument.Parse(stream);
        return FromJson(document.RootElement);
    }

    /// <summary>
    /// Texts from a JSON object in the format of <c>de.json</c>: nested objects whose path is the key;
    /// a property name may itself hold dots (<c>"owner.hint"</c>, CLAUDE §9).
    /// </summary>
    public static EmailTexts FromJson(JsonElement root)
    {
        var texts = new Dictionary<string, string>(StringComparer.Ordinal);
        Flatten(root, prefix: null, texts);
        return new EmailTexts(texts);
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
                    throw new InvalidOperationException($"The e-mail text {key} is not a text.");
            }
        }
    }
}
