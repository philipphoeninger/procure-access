// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.Platform.Contracts.Email;

/// <summary>
/// An e-mail could not be sent, also not on the retry (SPEC §7.6). It carries neither the recipient
/// nor the SMTP error: a server's answer can quote the address, and a caller that logs this
/// exception must not log it above <c>Debug</c> (PRV-11). The sender logs the cause at <c>Debug</c>.
/// </summary>
public sealed class EmailDeliveryException : Exception
{
    public EmailDeliveryException()
        : base("The e-mail could not be sent.")
    {
    }

    public EmailDeliveryException(string messageType)
        : base($"The e-mail {messageType} could not be sent.")
    {
    }
}
