// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using ProcureAccess.Modules.Platform.Contracts.Email;

namespace ProcureAccess.Modules.Platform.Email;

/// <summary>
/// <see cref="IEmailSender"/> over SMTP (ARCHITECTURE C71, SPEC §7.6): renders the message, sends it
/// synchronously and tries once more when the first attempt fails. Logs (PRV-11): the message type
/// at <c>Information</c> and above, the recipient and the SMTP error only at <c>Debug</c> — a
/// server's answer can quote the address, so a failure above <c>Debug</c> names only the type of the
/// error and the SMTP status code.
/// </summary>
internal sealed partial class SmtpEmailSender(
    EmailComposer composer,
    EmailSettings settings,
    ISmtpTransport transport,
    TimeProvider timeProvider,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    /// <summary>The first attempt and one retry.</summary>
    public const int Attempts = 2;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        // An address the account system accepted but SMTP cannot use (e.g. with a space) cannot be
        // delivered: the caller handles it like any failed send, without the address (PRV-11).
        if (!EmailComposer.TryParseRecipient(message.Recipient, out _))
        {
            LogUnusableRecipient(message.MessageType);
            throw new EmailDeliveryException(message.MessageType);
        }

        var mime = composer.Build(message, settings.Sender, timeProvider.GetUtcNow());

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await transport.SendAsync(mime, cancellationToken);
                LogSent(message.MessageType, attempt);
                LogSentTo(message.MessageType, message.Recipient);
                return;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                LogFailureDetail(exception, message.MessageType, message.Recipient, attempt);
                var status = exception is SmtpCommandException command ? (int)command.StatusCode : (int?)null;
                if (attempt < Attempts)
                {
                    LogRetry(message.MessageType, attempt, exception.GetType().Name, status);
                    continue;
                }

                LogFailed(message.MessageType, Attempts, exception.GetType().Name, status);
                throw new EmailDeliveryException(message.MessageType);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "E-mail {MessageType} not sent: the recipient is not one e-mail address.")]
    private partial void LogUnusableRecipient(string messageType);

    [LoggerMessage(Level = LogLevel.Information, Message = "E-mail {MessageType} sent (attempt {Attempt}).")]
    private partial void LogSent(string messageType, int attempt);

    [LoggerMessage(Level = LogLevel.Debug, Message = "E-mail {MessageType} sent to {Recipient}.")]
    private partial void LogSentTo(string messageType, string recipient);

    [LoggerMessage(Level = LogLevel.Debug, Message = "E-mail {MessageType} to {Recipient} failed on attempt {Attempt}.")]
    private partial void LogFailureDetail(Exception exception, string messageType, string recipient, int attempt);

    [LoggerMessage(Level = LogLevel.Warning, Message = "E-mail {MessageType} failed on attempt {Attempt} ({ErrorType}, SMTP status {SmtpStatus}); trying once more.")]
    private partial void LogRetry(string messageType, int attempt, string errorType, int? smtpStatus);

    [LoggerMessage(Level = LogLevel.Error, Message = "E-mail {MessageType} could not be sent after {Attempts} attempts ({ErrorType}, SMTP status {SmtpStatus}).")]
    private partial void LogFailed(string messageType, int attempts, string errorType, int? smtpStatus);
}
