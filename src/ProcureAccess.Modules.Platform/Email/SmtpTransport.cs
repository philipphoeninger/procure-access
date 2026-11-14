// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ProcureAccess.Modules.Platform.Email;

/// <summary>One delivery attempt of a MIME message; the seam of the sender's retry tests.</summary>
internal interface ISmtpTransport
{
    Task SendAsync(MimeMessage message, CancellationToken cancellationToken);
}

/// <summary>
/// Sends one message over SMTP with MailKit (MIT): connect, sign in if credentials are set, send,
/// disconnect — one connection per e-mail, so no connection outlives a request. STARTTLS is
/// required, not optional, unless the settings say <see cref="SmtpSecurity.None"/> (Mailpit,
/// Development and E2E only). No protocol logger: it would write the recipient.
/// </summary>
internal sealed class MailKitSmtpTransport(EmailSettings settings) : ISmtpTransport
{
    public async Task SendAsync(MimeMessage message, CancellationToken cancellationToken)
    {
        using var client = new SmtpClient();
        var options = settings.Security switch
        {
            SmtpSecurity.StartTls => SecureSocketOptions.StartTls,
            SmtpSecurity.SslOnConnect => SecureSocketOptions.SslOnConnect,
            _ => SecureSocketOptions.None,
        };
        await client.ConnectAsync(settings.Host, settings.Port, options, cancellationToken);
        if (settings.Username is not null && settings.Password is not null)
        {
            await client.AuthenticateAsync(settings.Username, settings.Password, cancellationToken);
        }

        // MailKit 5 sends a request; envelope sender and recipients come from the message headers.
        await client.SendAsync(new SendRequest(message), cancellationToken);
        await client.DisconnectAsync(quit: true, cancellationToken);
    }
}
