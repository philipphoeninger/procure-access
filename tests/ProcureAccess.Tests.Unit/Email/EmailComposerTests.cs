// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json;
using MimeKit;
using ProcureAccess.Modules.Platform.Contracts.Email;
using ProcureAccess.Modules.Platform.Email;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Email;

/// <summary>
/// The e-mails of ARCHITECTURE C71 as rendered from the UX copy texts (S5-11, SPEC §7.6, UX copy
/// §5.17): German text, the link in full on its own line, <c>email.footer</c> last, and an HTML part
/// with the same text and no other link, image or style (owner decision A).
/// </summary>
public class EmailComposerTests
{
    private static readonly Uri Link = new("https://procureaccess.example/confirm-email?userId=u1&token=a%2Bb");
    private static readonly EmailComposer Composer = new(EmailTexts.LoadGerman());

    [Fact]
    public void Compose_AddressConfirmation_TextOfTheUxCopyWithLinkAndFooter()
    {
        var email = Composer.Compose(new AddressConfirmationEmail("ada@example.test", Link, 24));

        email.Subject.ShouldBe("Bitte bestätigen Sie Ihre E-Mail-Adresse");
        email.Text.ShouldBe(
            "Guten Tag,\n\n" +
            "bitte bestätigen Sie Ihre E-Mail-Adresse für ProcureAccess mit diesem Link:\n" +
            Link.AbsoluteUri + "\n\n" +
            "Der Link ist 24 Stunden gültig. Wenn Sie kein Konto angelegt haben, können Sie diese E-Mail ignorieren.\n\n" +
            "Diese E-Mail wurde automatisch von ProcureAccess (Projekt WERTE.IT) versendet. Bitte antworten Sie nicht darauf.");
    }

    [Fact]
    public void Compose_AddressConfirmation_FillsTheConfiguredHours()
    {
        var email = Composer.Compose(new AddressConfirmationEmail("ada@example.test", Link, 48));

        email.Text.ShouldContain("Der Link ist 48 Stunden gültig.");
    }

    [Fact]
    public void Compose_PasswordReset_FillsTheHours()
    {
        var email = Composer.Compose(new PasswordResetEmail("ada@example.test", Link, validHours: 2));

        email.Subject.ShouldBe("Passwort zurücksetzen");
        email.Text.ShouldContain($"fest:\n{Link.AbsoluteUri}\n\nDer Link ist 2 Stunden gültig.");
    }

    [Fact]
    public void Compose_AlreadyRegistered_LinkOnItsOwnLine()
    {
        var email = Composer.Compose(new AlreadyRegisteredEmail("ada@example.test", Link));

        email.Subject.ShouldBe("Anmeldung bei ProcureAccess");
        email.Text.ShouldContain($"zurücksetzen:\n{Link.AbsoluteUri}\n\nWenn Sie das nicht waren");
    }

    [Fact]
    public void Compose_Invitation_OrganisationInSubjectAndRoleLabel()
    {
        var email = Composer.Compose(new OrganisationInvitationEmail("ada@example.test", "Stadt Musterhausen", InvitationRole.Editor, Link, validDays: 14));

        email.Subject.ShouldBe("Einladung in die Organisation Stadt Musterhausen");
        email.Text.ShouldContain("Sie wurden in die Organisation Stadt Musterhausen bei ProcureAccess eingeladen (Rolle: Bearbeiten).");
        email.Text.ShouldContain($"Einladung annehmen:\n{Link.AbsoluteUri}\n\nDie Einladung ist 14 Tage gültig.");
    }

    [Theory]
    [MemberData(nameof(EveryMessageType))]
    public void Compose_EveryMessageType_HtmlHasTheSameTextAndOnlyItsOwnLink(EmailMessage message)
    {
        var email = Composer.Compose(message);

        email.Html.ShouldStartWith("<!DOCTYPE html>\n<html lang=\"de\">");
        email.Html.ShouldContain($"<title>{System.Net.WebUtility.HtmlEncode(email.Subject)}</title>");
        var link = System.Net.WebUtility.HtmlEncode(Link.AbsoluteUri);
        email.Html.ShouldContain($"<a href=\"{link}\">{link}</a>");
        email.Html.Split("<a ").Length.ShouldBe(2, "exactly one link");
        email.Html.ShouldNotContain("<img", Case.Insensitive);
        email.Html.ShouldNotContain("src=", Case.Insensitive);
        email.Html.ShouldNotContain("style", Case.Insensitive);
        email.Html.ShouldNotContain("<script", Case.Insensitive);
        email.Html.ShouldContain("<p>Diese E-Mail wurde automatisch von ProcureAccess (Projekt WERTE.IT) versendet. Bitte antworten Sie nicht darauf.</p>");
    }

    [Fact]
    public void Compose_MarkupInAValue_IsEscapedInTheHtml()
    {
        var email = Composer.Compose(new OrganisationInvitationEmail("ada@example.test", "<b>A & B</b>", InvitationRole.Owner, Link, 14));

        email.Html.ShouldContain("&lt;b&gt;A &amp; B&lt;/b&gt;");
        email.Html.ShouldNotContain("<b>");
    }

    [Theory]
    [InlineData("Stadt\nMusterhausen")]
    [InlineData("Stadt\rBcc: x@example.test")]
    [InlineData("Stadt\tMusterhausen")]
    public void Compose_LineBreakOrControlCharacterInAValue_IsRefused(string organisation) =>
        Should.Throw<ArgumentException>(() =>
            Composer.Compose(new OrganisationInvitationEmail("ada@example.test", organisation, InvitationRole.Owner, Link, 14)));

    [Theory]
    [InlineData("ftp://procureaccess.example/x")]
    [InlineData("javascript:alert(1)")]
    public void Compose_LinkThatIsNotHttp_IsRefused(string link) =>
        Should.Throw<ArgumentException>(() => Composer.Compose(new AddressConfirmationEmail("ada@example.test", new Uri(link), 24)));

    [Fact]
    public void Compose_TextWithAPlaceholderTheTypeDoesNotFill_IsRefused()
    {
        var composer = new EmailComposer(Texts(("email.confirm.body", "Hallo {{name}}:\\n{{link}}")));

        Should.Throw<InvalidOperationException>(() => composer.Compose(new AddressConfirmationEmail("ada@example.test", Link, 24)))
            .Message.ShouldContain("{{name}}");
    }

    [Fact]
    public void Compose_TextWithoutTheLink_IsRefused()
    {
        var composer = new EmailComposer(Texts(("email.confirm.body", "Hallo.")));

        Should.Throw<InvalidOperationException>(() => composer.Compose(new AddressConfirmationEmail("ada@example.test", Link, 24)))
            .Message.ShouldContain("link");
    }

    [Fact]
    public void Build_Message_PlainTextAndHtmlAlternativesFromTheSenderToTheRecipientOnly()
    {
        var date = new DateTimeOffset(2026, 11, 14, 10, 0, 0, TimeSpan.Zero);
        var sender = new MailboxAddress("ProcureAccess", "noreply@procureaccess.example");

        var mime = Composer.Build(new AddressConfirmationEmail("ada@example.test", Link, 24), sender, date);

        mime.From.Mailboxes.Single().ShouldBe(sender);
        mime.To.Mailboxes.Single().Address.ShouldBe("ada@example.test");
        mime.Cc.ShouldBeEmpty();
        mime.Bcc.ShouldBeEmpty();
        mime.Date.ShouldBe(date);
        mime.MessageId.ShouldEndWith("@procureaccess.example");
        var alternative = mime.Body.ShouldBeOfType<MultipartAlternative>();
        alternative.Select(part => part.ContentType.MimeType).ShouldBe(["text/plain", "text/html"]);
        alternative.OfType<TextPart>().ShouldAllBe(part => part.ContentType.Charset == "utf-8");
        mime.TextBody.ShouldNotBeNull().ShouldContain(Link.AbsoluteUri);
    }

    [Theory]
    [InlineData("not an address")]
    [InlineData("ada@example.test, eve@example.test")]
    public void Build_RecipientThatIsNotOneAddress_IsRefusedWithoutQuotingIt(string recipient)
    {
        var exception = Should.Throw<ArgumentException>(() =>
            Composer.Build(new AddressConfirmationEmail(recipient, Link, 24), new MailboxAddress("", "noreply@procureaccess.example"), DateTimeOffset.UnixEpoch));

        exception.Message.ShouldNotContain(recipient);
    }

    [Fact]
    public void ToString_Message_NamesOnlyTheMessageType() =>
        new PasswordResetEmail("ada@example.test", Link, 2).ToString().ShouldBe(nameof(PasswordResetEmail));

    public static TheoryData<EmailMessage> EveryMessageType() =>
    [
        new AddressConfirmationEmail("ada@example.test", Link, 24),
        new PasswordResetEmail("ada@example.test", Link, 2),
        new AlreadyRegisteredEmail("ada@example.test", Link),
        new OrganisationInvitationEmail("ada@example.test", "Stadt Musterhausen", InvitationRole.Reader, Link, 14),
    ];

    /// <summary>The German texts with some values replaced.</summary>
    private static EmailTexts Texts(params (string Key, string Text)[] replaced)
    {
        var texts = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var key in EmailTexts.Required)
        {
            texts[key] = EmailTexts.LoadGerman()[key];
        }

        foreach (var (key, text) in replaced)
        {
            texts[key] = text;
        }

        using var document = JsonDocument.Parse(JsonSerializer.Serialize(texts));
        return EmailTexts.FromJson(document.RootElement);
    }
}
