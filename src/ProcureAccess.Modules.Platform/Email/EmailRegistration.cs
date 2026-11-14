// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProcureAccess.Modules.Platform.Contracts.Email;

namespace ProcureAccess.Modules.Platform.Email;

/// <summary>Registers <see cref="IEmailSender"/> with its settings, texts and SMTP transport (ARCHITECTURE C71).</summary>
internal static class EmailRegistration
{
    public static IServiceCollection AddEmailSender(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Read here, not on first use: missing settings or texts stop the start (owner decision S5-11 4a).
        services.AddSingleton(EmailSettings.From(configuration, environment));
        services.AddSingleton(EmailTexts.LoadGerman());
        services.AddSingleton<EmailComposer>();
        services.AddSingleton<ISmtpTransport, MailKitSmtpTransport>();
        services.AddSingleton<IEmailSender, SmtpEmailSender>();
        return services;
    }
}
