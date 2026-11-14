// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using MimeKit;
using ProcureAccess.Modules.Platform.Contracts.Email;
using ProcureAccess.Modules.Platform.Email;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Platform;

/// <summary>
/// PRV-11 on the real SMTP path (S5-11, ARCHITECTURE C71, TESTING §4.3): every message type goes
/// through <see cref="SmtpEmailSender"/> and MailKit to a Mailpit container and is read back over
/// Mailpit's API. The mail holds exactly the rendered texts with the message's own link — no
/// tracking pixel, no other link, no header beyond the message itself — and no log event at
/// <c>Information</c> or above names the recipient, also when the server refuses it.
/// </summary>
public sealed partial class EmailDeliveryTests(MailpitContainer mailpit) : IClassFixture<MailpitContainer>, IAsyncLifetime
{
    private const string Sender = "noreply@procureaccess.example";

    /// <summary>A recipient Mailpit refuses (<c>MP_SMTP_ALLOWED_RECIPIENTS</c>), so the sender sees a real SMTP error.</summary>
    private const string RefusedRecipient = "eve.refused@refused.example";

    private static readonly Uri Link = new("https://procureaccess.example/confirm-email?userId=u1&token=a%2Bb");

    private static readonly HashSet<string> MessageHeaders =
        new(["From", "Date", "Subject", "Message-Id", "To", "MIME-Version", "Content-Type"], StringComparer.OrdinalIgnoreCase);

    /// <summary>Headers Mailpit adds on receipt, as any receiving server does.</summary>
    private static readonly HashSet<string> ReceiverHeaders =
        new(["Received", "Return-Path"], StringComparer.OrdinalIgnoreCase);

    private readonly CapturingLoggerProvider _logs = new();

    public static TheoryData<string> MessageTypes() =>
        [nameof(AddressConfirmationEmail), nameof(PasswordResetEmail), nameof(AlreadyRegisteredEmail), nameof(OrganisationInvitationEmail)];

    /// <summary>Each test starts with an empty mailbox.</summary>
    public async ValueTask InitializeAsync() =>
        (await mailpit.Api.DeleteAsync("/api/v1/messages", TestContext.Current.CancellationToken)).EnsureSuccessStatusCode();

    public ValueTask DisposeAsync()
    {
        _logs.Dispose();
        return ValueTask.CompletedTask;
    }

    [Theory(DisplayName = "INV:PRV-11 " + nameof(Send_EveryMessageType_ArrivesAsTheRenderedTextsWithOnlyItsOwnLink))]
    [Trait("Invariant", "PRV-11")]
    [MemberData(nameof(MessageTypes))]
    public async Task Send_EveryMessageType_ArrivesAsTheRenderedTextsWithOnlyItsOwnLink(string messageType)
    {
        var recipient = $"ada.{messageType.ToLowerInvariant()}@example.test";
        var message = Message(messageType, recipient);
        var expected = new EmailComposer(EmailTexts.LoadGerman()).Compose(message);

        await CreateSender().SendAsync(message, TestContext.Current.CancellationToken);

        var mail = await SingleMailAsync();
        mail.From.Mailboxes.Single().Address.ShouldBe(Sender);
        mail.To.Mailboxes.Select(m => m.Address).ShouldBe([recipient]);
        mail.Cc.ShouldBeEmpty();
        mail.Subject.ShouldBe(expected.Subject);

        // Exactly the two alternatives, each exactly the rendered text: nothing added on the way.
        var alternative = mail.Body.ShouldBeOfType<MultipartAlternative>();
        alternative.Select(part => part.ContentType.MimeType).ShouldBe(["text/plain", "text/html"]);
        Normalise(mail.TextBody).ShouldBe(expected.Text);
        Normalise(mail.HtmlBody).ShouldBe(Normalise(expected.Html));

        // No tracking pixel, no external resource, no link but the message's own.
        var html = mail.HtmlBody.ShouldNotBeNull();
        html.ShouldNotContain("<img", Case.Insensitive);
        html.ShouldNotContain("src=", Case.Insensitive);
        Href().Matches(html).Select(m => System.Net.WebUtility.HtmlDecode(m.Groups["url"].Value)).ShouldBe([Link.AbsoluteUri]);
        mail.TextBody.ShouldNotBeNull().Split('\n').Select(line => line.TrimEnd('\r')).ShouldContain(Link.AbsoluteUri);

        // No header beyond the message itself (no tracking or list headers).
        mail.Headers.Select(h => h.Field).Where(name => !ReceiverHeaders.Contains(name)).ShouldAllBe(name => MessageHeaders.Contains(name));

        AboveDebug().ShouldNotContain(recipient);
        _logs.Text.ShouldContain(recipient, customMessage: "the capture sees Debug, so the check above is not empty");
    }

    [Fact(DisplayName = "INV:PRV-11 " + nameof(Send_ServerRefusesTheRecipient_FailsAfterTheRetryWithoutLoggingTheAddressAboveDebug))]
    [Trait("Invariant", "PRV-11")]
    public async Task Send_ServerRefusesTheRecipient_FailsAfterTheRetryWithoutLoggingTheAddressAboveDebug()
    {
        var exception = await Should.ThrowAsync<EmailDeliveryException>(
            () => CreateSender().SendAsync(Message(nameof(AddressConfirmationEmail), RefusedRecipient), TestContext.Current.CancellationToken));

        exception.ToString().ShouldNotContain(RefusedRecipient);
        (await MessagesAsync()).Total.ShouldBe(0);
        AboveDebug().ShouldContain("could not be sent after 2 attempts");
        AboveDebug().ShouldNotContain(RefusedRecipient);
        AboveDebug().ShouldNotContain("refused.example");
    }

    private SmtpEmailSender CreateSender()
    {
        var settings = EmailSettings.From(
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Email:Smtp:Host"] = mailpit.Host,
                ["Email:Smtp:Port"] = mailpit.SmtpPort.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["Email:Smtp:Security"] = "None",
                ["Email:From"] = Sender,
                ["Email:DisplayName"] = "ProcureAccess",
            }).Build(),
            new HostingEnvironment { EnvironmentName = "Development" });
        var loggers = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug).AddProvider(_logs));
        return new SmtpEmailSender(
            new EmailComposer(EmailTexts.LoadGerman()),
            settings,
            new MailKitSmtpTransport(settings),
            TimeProvider.System,
            loggers.CreateLogger<SmtpEmailSender>());
    }

    private static EmailMessage Message(string messageType, string recipient) => messageType switch
    {
        nameof(AddressConfirmationEmail) => new AddressConfirmationEmail(recipient, Link, 24),
        nameof(PasswordResetEmail) => new PasswordResetEmail(recipient, Link, 2),
        nameof(AlreadyRegisteredEmail) => new AlreadyRegisteredEmail(recipient, Link),
        nameof(OrganisationInvitationEmail) => new OrganisationInvitationEmail(recipient, "Stadt Musterhausen", InvitationRole.Editor, Link, 14),
        _ => throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null),
    };

    /// <summary>The captured log events at <c>Information</c> and above, as one text.</summary>
    private string AboveDebug() => string.Join(
        "\n",
        _logs.Events.Where(e => !e.StartsWith(nameof(LogLevel.Debug), StringComparison.Ordinal) && !e.StartsWith(nameof(LogLevel.Trace), StringComparison.Ordinal)));

    private async Task<MimeMessage> SingleMailAsync()
    {
        var list = await MessagesAsync();
        list.Total.ShouldBe(1);
        await using var raw = await mailpit.Api.GetStreamAsync($"/api/v1/message/{list.Messages[0].Id}/raw", TestContext.Current.CancellationToken);
        return await MimeMessage.LoadAsync(raw, TestContext.Current.CancellationToken);
    }

    private async Task<MailpitList> MessagesAsync() =>
        (await mailpit.Api.GetFromJsonAsync<MailpitList>("/api/v1/messages", TestContext.Current.CancellationToken)).ShouldNotBeNull();

    /// <summary>Line endings as the renderer writes them (SMTP sends CRLF), without the trailing line end MimeKit adds.</summary>
    private static string Normalise(string? text) => (text ?? "").Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n');

    [GeneratedRegex("href=\"(?<url>[^\"]*)\"", RegexOptions.IgnoreCase)]
    private static partial Regex Href();

    private sealed record MailpitList(
        [property: JsonPropertyName("total")] int Total,
        [property: JsonPropertyName("messages")] IReadOnlyList<MailpitSummary> Messages);

    private sealed record MailpitSummary([property: JsonPropertyName("ID")] string Id);
}

/// <summary>
/// One Mailpit container per test class (S5-11), with the image of the compose overrides. It
/// accepts only recipients under <c>example.test</c>, so a test can see a real SMTP refusal.
/// </summary>
public sealed class MailpitContainer : IAsyncLifetime
{
    /// <summary>The Mailpit image of <c>docker-compose.dev.yml</c> and <c>docker-compose.e2e.yml</c>; change them together.</summary>
    public const string Image = "axllent/mailpit:v1.33.1@sha256:aef2aacc11f075ec0fb6874275ee4a421e688e11a96ebb296134d1735c7761ce";

    private const ushort Smtp = 1025;
    private const ushort Http = 8025;

    private readonly IContainer _container = new ContainerBuilder(Image)
        .WithPortBinding(Smtp, assignRandomHostPort: true)
        .WithPortBinding(Http, assignRandomHostPort: true)
        .WithEnvironment("MP_SMTP_ALLOWED_RECIPIENTS", @"@example\.test$")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(request => request.ForPort(Http).ForPath("/livez")))
        .Build();

    private HttpClient? _api;

    public string Host => _container.Hostname;

    public int SmtpPort => _container.GetMappedPublicPort(Smtp);

    /// <summary>Mailpit's HTTP API (<c>/api/v1/…</c>).</summary>
    public HttpClient Api => _api ?? throw new InvalidOperationException("Mailpit is not started.");

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync();
        _api = new HttpClient { BaseAddress = new Uri($"http://{_container.Hostname}:{_container.GetMappedPublicPort(Http)}") };
    }

    public async ValueTask DisposeAsync()
    {
        _api?.Dispose();
        await _container.DisposeAsync();
    }
}
