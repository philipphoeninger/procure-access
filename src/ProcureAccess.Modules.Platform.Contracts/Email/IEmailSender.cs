// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Contracts.Email;

/// <summary>
/// Sends the e-mails of the application (ARCHITECTURE C71, SPEC §7.6): Platform renders the German
/// texts of the message type (<c>email.*</c> of the UX copy) as a plain-text and a simple HTML part
/// and sends them over SMTP with TLS — Lettermint in staging and production, Mailpit in development
/// and E2E. No tracking pixel, no rewritten link, and the recipient address is never logged above
/// <c>Debug</c> (PRV-11).
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends <paramref name="message"/> synchronously within the request, with one retry. Throws
    /// <see cref="EmailDeliveryException"/> when both attempts failed; the caller keeps its own
    /// action (e.g. the account) and shows <c>errors.email.sendFailed</c> (SPEC §7.6).
    /// </summary>
    Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
