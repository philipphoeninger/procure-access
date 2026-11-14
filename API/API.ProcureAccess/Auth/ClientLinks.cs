// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace API.ProcureAccess.Auth;

/// <summary>Links into the client for the account e-mails: <c>App:BaseUrl</c> (absolute) plus a path.</summary>
public static class ClientLinks
{
    public const string BaseUrlSetting = "App:BaseUrl";

    /// <summary>The address-confirmation page of S02 (UX copy §3, U118); the token follows as <c>?token=</c>.</summary>
    public const string ConfirmationPagePath = "/registrieren/bestaetigen";

    /// <summary>Sign-in S01 in the <c>login</c> outlet (UX copy §3); its „Passwort vergessen?“ leads to the reset.</summary>
    public const string LoginPagePath = "/(login:anmelden)";

    /// <summary>The absolute link to <paramref name="pathAndQuery"/>; throws when <c>App:BaseUrl</c> is not absolute.</summary>
    public static Uri To(IConfiguration configuration, string pathAndQuery)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var baseUrl = configuration[BaseUrlSetting];
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var root))
        {
            throw new InvalidOperationException($"{BaseUrlSetting} must be the absolute address of the client.");
        }

        return new Uri(root.GetLeftPart(UriPartial.Authority) + root.AbsolutePath.TrimEnd('/') + pathAndQuery);
    }
}
