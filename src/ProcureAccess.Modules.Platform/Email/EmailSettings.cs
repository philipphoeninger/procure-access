// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MimeKit;

namespace ProcureAccess.Modules.Platform.Email;

/// <summary>How the SMTP connection is secured (<c>Email:Smtp:Security</c>).</summary>
internal enum SmtpSecurity
{
    /// <summary>Plain connection upgraded with STARTTLS, which must succeed (Lettermint on port 587).</summary>
    StartTls,

    /// <summary>TLS from the first byte (port 465).</summary>
    SslOnConnect,

    /// <summary>No TLS: only for Mailpit in Development and E2E.</summary>
    None,
}

/// <summary>
/// The SMTP settings of the e-mail sender (SPEC §7.6, ARCHITECTURE C71), from the environment only:
/// <c>Email:Smtp:Host</c>, <c>:Port</c>, <c>:Username</c>, <c>:Password</c>, <c>:Security</c>,
/// <c>Email:From</c> and <c>Email:DisplayName</c>. Only <c>Development</c> and <c>E2E</c> may send
/// without credentials or without TLS (Mailpit); every other environment stops at startup without
/// them (owner decision S5-11 4a), like the key-ring certificate (S4-26). Not a record: its
/// <c>ToString</c> would print the password.
/// </summary>
internal sealed class EmailSettings
{
    public const string SectionName = "Email";

    /// <summary>Environment of the E2E stack once it has its own (TESTING §3.3); until then it runs as Development.</summary>
    public const string E2EEnvironmentName = "E2E";

    private EmailSettings(string host, int port, string? username, string? password, SmtpSecurity security, MailboxAddress sender)
    {
        Host = host;
        Port = port;
        Username = username;
        Password = password;
        Security = security;
        Sender = sender;
    }

    public string Host { get; }

    public int Port { get; }

    /// <summary>Null when the server needs no sign-in (Mailpit).</summary>
    public string? Username { get; }

    public string? Password { get; }

    public SmtpSecurity Security { get; }

    /// <summary>The sender address with its display name.</summary>
    public MailboxAddress Sender { get; }

    /// <summary>True only in <c>Development</c> and <c>E2E</c>: local stacks and CI send to Mailpit.</summary>
    public static bool MaySendWithoutTlsOrCredentials(IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        return environment.IsDevelopment() || environment.IsEnvironment(E2EEnvironmentName);
    }

    /// <summary>Reads the settings; throws <see cref="InvalidOperationException"/> to stop the start. No message holds a value.</summary>
    public static EmailSettings From(IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var email = configuration.GetSection(SectionName);
        var smtp = email.GetSection("Smtp");
        var lenient = MaySendWithoutTlsOrCredentials(environment);

        var host = smtp["Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            throw Invalid("Email:Smtp:Host is not set.");
        }

        if (!int.TryParse(smtp["Port"], out var port) || port is < 1 or > 65535)
        {
            throw Invalid("Email:Smtp:Port must be a port number (587 for STARTTLS).");
        }

        var securityText = smtp["Security"];
        var security = SmtpSecurity.StartTls;
        if (!string.IsNullOrWhiteSpace(securityText)
            && (!Enum.TryParse(securityText, ignoreCase: true, out security) || !Enum.IsDefined(security)))
        {
            throw Invalid("Email:Smtp:Security must be StartTls, SslOnConnect or None.");
        }

        if (security == SmtpSecurity.None && !lenient)
        {
            throw Invalid("Email:Smtp:Security None is allowed only in Development and E2E; e-mails leave every other environment over TLS.");
        }

        var username = Blank(smtp["Username"]);
        var password = Blank(smtp["Password"]);
        if ((username is null) != (password is null))
        {
            throw Invalid("Email:Smtp:Username and Email:Smtp:Password are set only together.");
        }

        if (username is null && !lenient)
        {
            throw Invalid(
                "Email:Smtp:Username and Email:Smtp:Password are not set. Outside Development and E2E the API " +
                "sends e-mail through the provider's SMTP relay (Lettermint): set SMTP_USERNAME and SMTP_PASSWORD (see .env.example).");
        }

        if (!MailboxAddress.TryParse(email["From"], out var sender) || string.IsNullOrEmpty(sender.Address) || !sender.Address.Contains('@', StringComparison.Ordinal))
        {
            throw Invalid("Email:From must be one e-mail address.");
        }

        var displayName = email["DisplayName"];
        if (!string.IsNullOrWhiteSpace(displayName))
        {
            sender = new MailboxAddress(displayName, sender.Address);
        }

        return new EmailSettings(host.Trim(), port, username, password, security, sender);
    }

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private static InvalidOperationException Invalid(string message) => new($"E-mail settings (SPEC §7.6): {message}");
}
