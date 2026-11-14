// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using MimeKit;
using MimeKit.Utils;
using ProcureAccess.Modules.Platform.Contracts.Email;

namespace ProcureAccess.Modules.Platform.Email;

/// <summary>The rendered texts of one e-mail: subject, plain text and the HTML built from it.</summary>
internal sealed record ComposedEmail(string Subject, string Text, string Html);

/// <summary>
/// Renders a message type of ARCHITECTURE C71 from its <c>email.*</c> texts (UX copy §5.17) and
/// builds the MIME message (SPEC §7.6): a plain-text part and a simple HTML part with the same
/// text, German, <c>email.footer</c> last. The HTML part has no image, no style, no script and no
/// link but the message's own link, written out in full as its own text (PRV-11; owner decision
/// S5-11 A). Every value is HTML-escaped; a value with a line break or another control character is
/// refused, so it cannot add a line, a paragraph or a header.
/// </summary>
internal sealed partial class EmailComposer(EmailTexts texts)
{
    /// <summary>The UX copy writes a line break inside a table cell as the two characters <c>\n</c>.</summary>
    private const string EscapedNewLine = "\\n";

    /// <summary>Renders the subject, the plain text and the HTML of <paramref name="message"/>.</summary>
    public ComposedEmail Compose(EmailMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        var (key, values, link) = Describe(message);

        var subject = Fill(texts[$"{key}.subject"], values, out var usedInSubject);
        var body = Fill(texts[$"{key}.body"], values, out var usedInBody);
        var unused = values.Keys.Except(usedInSubject).Except(usedInBody).ToList();
        if (unused.Count > 0)
        {
            throw new InvalidOperationException(
                $"The text of {message.MessageType} does not use {string.Join(", ", unused)}; check the UX copy (§5.17).");
        }

        var text = body.Replace(EscapedNewLine, "\n", StringComparison.Ordinal) + "\n\n" + texts["email.footer"];
        return new ComposedEmail(subject, text, Html(subject, text, link));
    }

    /// <summary>
    /// The MIME message of <paramref name="message"/> from <paramref name="sender"/>, dated
    /// <paramref name="date"/> (from <see cref="TimeProvider"/>): <c>multipart/alternative</c> with
    /// <c>text/plain</c> and <c>text/html</c>, UTF-8.
    /// </summary>
    public MimeMessage Build(EmailMessage message, MailboxAddress sender, DateTimeOffset date)
    {
        ArgumentNullException.ThrowIfNull(message);
        ArgumentNullException.ThrowIfNull(sender);

        // The parser's error would quote the address, so it is never shown (PRV-11).
        if (!TryParseRecipient(message.Recipient, out var parsed))
        {
            throw new ArgumentException("The recipient is not one e-mail address.", nameof(message));
        }

        var composed = Compose(message);
        var mime = new MimeMessage
        {
            Subject = composed.Subject,
            Date = date,
            MessageId = MimeUtils.GenerateMessageId(sender.Domain),
            Body = new BodyBuilder { TextBody = composed.Text, HtmlBody = composed.Html }.ToMessageBody(),
        };
        mime.From.Add(sender);
        mime.To.Add(parsed);
        return mime;
    }

    /// <summary>
    /// True when <paramref name="recipient"/> is exactly one address MimeKit can send to; the result
    /// carries the address without a display name.
    /// </summary>
    public static bool TryParseRecipient(string recipient, out MailboxAddress address)
    {
        address = new MailboxAddress(string.Empty, "invalid@invalid");
        if (!MailboxAddress.TryParse(recipient, out var parsed)
            || string.IsNullOrEmpty(parsed.Address)
            || !parsed.Address.Contains('@', StringComparison.Ordinal))
        {
            return false;
        }

        address = new MailboxAddress(string.Empty, parsed.Address);
        return true;
    }

    /// <summary>The text key of the message type, the values of its placeholders and its link.</summary>
    private (string Key, Dictionary<string, string> Values, string Link) Describe(EmailMessage message)
    {
        switch (message)
        {
            case AddressConfirmationEmail confirm:
                return ("email.confirm", Values(("link", LinkText(confirm.Link)), ("hours", Number(confirm.ValidHours))), LinkText(confirm.Link));
            case PasswordResetEmail reset:
                return ("email.reset", Values(("link", LinkText(reset.Link)), ("hours", Number(reset.ValidHours))), LinkText(reset.Link));
            case AlreadyRegisteredEmail registered:
                return ("email.alreadyRegistered", Values(("link", LinkText(registered.Link))), LinkText(registered.Link));
            case OrganisationInvitationEmail invitation:
                return (
                    "email.invitation",
                    Values(
                        ("org", invitation.OrganisationName.Trim()),
                        ("role", texts[RoleKey(invitation.Role)]),
                        ("link", LinkText(invitation.Link)),
                        ("days", Number(invitation.ValidDays))),
                    LinkText(invitation.Link));
            default:
                throw new ArgumentException($"Unknown message type {message.MessageType}.", nameof(message));
        }
    }

    private static string RoleKey(InvitationRole role) => role switch
    {
        InvitationRole.Owner => "org.members.role.owner",
        InvitationRole.Editor => "org.members.role.editor",
        InvitationRole.Reader => "org.members.role.reader",
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown invitation role."),
    };

    private static Dictionary<string, string> Values(params (string Name, string Value)[] values)
    {
        foreach (var (name, value) in values)
        {
            if (value.Any(char.IsControl))
            {
                throw new ArgumentException($"The value of {{{{{name}}}}} holds a line break or another control character.");
            }
        }

        return values.ToDictionary(v => v.Name, v => v.Value, StringComparer.Ordinal);
    }

    /// <summary>The link as written in the e-mail: an absolute http(s) address, never rewritten.</summary>
    private static string LinkText(Uri link)
    {
        if (!link.IsAbsoluteUri || (link.Scheme != Uri.UriSchemeHttps && link.Scheme != Uri.UriSchemeHttp))
        {
            throw new ArgumentException("An e-mail link must be an absolute http(s) address.", nameof(link));
        }

        return link.AbsoluteUri;
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>Replaces every <c>{{name}}</c>; a placeholder without a value is an error in the texts.</summary>
    private static string Fill(string template, Dictionary<string, string> values, out HashSet<string> used)
    {
        var found = new HashSet<string>(StringComparer.Ordinal);
        var result = Placeholder().Replace(template, match =>
        {
            var name = match.Groups["name"].Value;
            if (!values.TryGetValue(name, out var value))
            {
                throw new InvalidOperationException($"The e-mail text has the placeholder {{{{{name}}}}}, which its message type does not fill.");
            }

            found.Add(name);
            return value;
        });
        used = found;
        return result;
    }

    /// <summary>
    /// The HTML part: the same text, one <c>p</c> per paragraph and <c>br</c> between its lines; the
    /// line that is the link becomes that link.
    /// </summary>
    private static string Html(string subject, string text, string link)
    {
        var html = new StringBuilder();
        html.Append("<!DOCTYPE html>\n<html lang=\"de\">\n<head>\n<meta charset=\"utf-8\">\n<title>")
            .Append(WebUtility.HtmlEncode(subject))
            .Append("</title>\n</head>\n<body>\n");
        foreach (var paragraph in text.Split("\n\n", StringSplitOptions.RemoveEmptyEntries))
        {
            var lines = paragraph.Split('\n').Select(line => line == link
                ? $"<a href=\"{WebUtility.HtmlEncode(link)}\">{WebUtility.HtmlEncode(link)}</a>"
                : WebUtility.HtmlEncode(line));
            html.Append("<p>").Append(string.Join("<br>\n", lines)).Append("</p>\n");
        }

        return html.Append("</body>\n</html>\n").ToString();
    }

    [GeneratedRegex(@"\{\{(?<name>[A-Za-z]+)\}\}")]
    private static partial Regex Placeholder();
}
