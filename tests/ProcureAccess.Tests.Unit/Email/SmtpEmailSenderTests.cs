// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using MimeKit;
using ProcureAccess.Modules.Platform.Contracts.Email;
using ProcureAccess.Modules.Platform.Email;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.Email;

/// <summary>The sender's delivery rule (S5-11, SPEC §7.6): synchronous, one retry, then <see cref="EmailDeliveryException"/>.</summary>
public class SmtpEmailSenderTests
{
    private const string Recipient = "ada@example.test";
    private static readonly DateTimeOffset Now = new(2026, 11, 14, 9, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task SendAsync_FirstAttemptSucceeds_SendsOnce()
    {
        var transport = new ScriptedTransport(failures: 0);

        await Sender(transport).SendAsync(Message(), TestContext.Current.CancellationToken);

        transport.Attempts.ShouldBe(1);
        transport.Last!.Date.ShouldBe(Now);
        transport.Last.To.Mailboxes.Single().Address.ShouldBe(Recipient);
    }

    [Fact]
    public async Task SendAsync_FirstAttemptFails_TriesOnceMore()
    {
        var transport = new ScriptedTransport(failures: 1);

        await Sender(transport).SendAsync(Message(), TestContext.Current.CancellationToken);

        transport.Attempts.ShouldBe(2);
    }

    [Fact]
    public async Task SendAsync_BothAttemptsFail_ThrowsDeliveryExceptionWithoutTheCause()
    {
        var transport = new ScriptedTransport(failures: 2);

        var exception = await Should.ThrowAsync<EmailDeliveryException>(
            () => Sender(transport).SendAsync(Message(), TestContext.Current.CancellationToken));

        transport.Attempts.ShouldBe(SmtpEmailSender.Attempts);
        exception.InnerException.ShouldBeNull();
        exception.Message.ShouldNotContain(Recipient);
    }

    [Fact]
    public async Task SendAsync_Cancelled_DoesNotRetry()
    {
        var transport = new ScriptedTransport(failures: 0, cancel: true);

        await Should.ThrowAsync<OperationCanceledException>(
            () => Sender(transport).SendAsync(Message(), TestContext.Current.CancellationToken));

        transport.Attempts.ShouldBe(1);
    }

    [Theory]
    [InlineData("ada lovelace@example.test")]
    [InlineData("ada@example.test, eve@example.test")]
    public async Task SendAsync_RecipientSmtpCannotUse_FailsLikeADeliveryWithoutTrying(string recipient)
    {
        var transport = new ScriptedTransport(failures: 0);

        await Should.ThrowAsync<EmailDeliveryException>(() => Sender(transport).SendAsync(
            new AddressConfirmationEmail(recipient, new Uri("https://procureaccess.example/confirm-email?token=t"), 24),
            TestContext.Current.CancellationToken));

        transport.Attempts.ShouldBe(0);
    }

    private static AddressConfirmationEmail Message() => new(Recipient, new Uri("https://procureaccess.example/confirm-email?token=t"), 24);

    private static SmtpEmailSender Sender(ISmtpTransport transport)
    {
        var settings = EmailSettings.From(
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Email:Smtp:Host"] = "mailpit",
                ["Email:Smtp:Port"] = "1025",
                ["Email:Smtp:Security"] = "None",
                ["Email:From"] = "noreply@procureaccess.example",
            }).Build(),
            new HostingEnvironment { EnvironmentName = "Development" });
        return new SmtpEmailSender(
            new EmailComposer(EmailTexts.LoadGerman()), settings, transport, new FakeTimeProvider(Now), NullLogger<SmtpEmailSender>.Instance);
    }

    /// <summary>Fails the first <paramref name="failures"/> attempts with an SMTP error that quotes the recipient.</summary>
    private sealed class ScriptedTransport(int failures, bool cancel = false) : ISmtpTransport
    {
        public int Attempts { get; private set; }

        public MimeMessage? Last { get; private set; }

        public Task SendAsync(MimeMessage message, CancellationToken cancellationToken)
        {
            Attempts++;
            Last = message;
            if (cancel)
            {
                throw new OperationCanceledException();
            }

            if (Attempts <= failures)
            {
                throw new SmtpCommandException(SmtpErrorCode.RecipientNotAccepted, SmtpStatusCode.MailboxUnavailable, $"<{Recipient}>: mailbox unavailable");
            }

            return Task.CompletedTask;
        }
    }
}
