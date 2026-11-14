// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Collections.Concurrent;
using ProcureAccess.Modules.Platform.Contracts.Email;

namespace ProcureAccess.Tests.Integration.Infrastructure;

/// <summary>
/// The <see cref="IEmailSender"/> of every integration host (TESTING §4.3 PRV-11): keeps each message
/// instead of sending it, so no test run reaches an SMTP server. The SMTP path itself is tested
/// against a Mailpit container (<c>Platform/EmailDeliveryTests</c>).
/// </summary>
public sealed class CapturingEmailSender : IEmailSender
{
    private readonly ConcurrentQueue<EmailMessage> _sent = new();

    /// <summary>When set, every send fails as the real sender does after its retry.</summary>
    public bool Fail { get; set; }

    /// <summary>Every message sent so far, in order.</summary>
    public IReadOnlyCollection<EmailMessage> Sent => _sent;

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (Fail)
        {
            throw new EmailDeliveryException(message.MessageType);
        }

        _sent.Enqueue(message);
        return Task.CompletedTask;
    }

    /// <summary>Forgets the messages sent so far and sends again.</summary>
    public void Clear()
    {
        _sent.Clear();
        Fail = false;
    }
}
