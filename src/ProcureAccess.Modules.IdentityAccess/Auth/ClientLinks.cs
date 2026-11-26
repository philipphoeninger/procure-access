// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using ProcureAccess.Modules.IdentityAccess.DataProtection;

namespace ProcureAccess.Modules.IdentityAccess.Auth;

/// <summary>Links into the client for the account e-mails: <c>App:BaseUrl</c> (absolute) plus a path.</summary>
internal static class ClientLinks
{
    public const string BaseUrlSetting = "App:BaseUrl";

    /// <summary>
    /// The value of <c>appsettings.json</c>: a placeholder that no deployment may keep (S7-09, #447;
    /// owner decision O-S7-5). <c>PublicAddressDefaultsTests</c> keeps it equal to the file.
    /// </summary>
    public const string DefaultBaseUrl = "http://procureaccess.tech:4200";

    /// <summary>The address-confirmation page of S02 (UX copy §3, U118); the token follows as <c>?token=</c>.</summary>
    public const string ConfirmationPagePath = "/registrieren/bestaetigen";

    /// <summary>The new-password step of S03 (S5-13a); the token follows as <c>?token=</c>.</summary>
    public const string PasswordResetPagePath = "/passwort/neu";

    /// <summary>The page that accepts an invitation into an organisation (S5-17); the token follows as <c>?token=</c>.</summary>
    public const string InvitationPagePath = "/organisation/einladung";

    /// <summary>Sign-in S01 in the <c>login</c> outlet (UX copy §3); its „Passwort vergessen?“ leads to the reset.</summary>
    public const string LoginPagePath = "/(login:anmelden)";

    /// <summary>The absolute link to <paramref name="pathAndQuery"/>; throws when <c>App:BaseUrl</c> is not absolute.</summary>
    public static Uri To(IConfiguration configuration, string pathAndQuery)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return new Uri(Root(configuration[BaseUrlSetting]) + pathAndQuery);
    }

    /// <summary>
    /// Stops the start (S7-09, #447; owner decision O-S7-5) when <c>App:BaseUrl</c> is not an absolute
    /// http(s) address, and outside <c>Development</c> and <c>E2E</c> when it still is
    /// <see cref="DefaultBaseUrl"/>: every e-mail of a deployment would link to another site. Like
    /// the key-ring certificate (<see cref="KeyRingCertificates"/>), read while the services are
    /// registered.
    /// </summary>
    public static void EnsureDeploymentAddress(IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var root = Root(configuration[BaseUrlSetting]);
        if (!KeyRingCertificates.MayStoreKeysUnencrypted(environment)
            && string.Equals(root, Root(DefaultBaseUrl), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"{BaseUrlSetting} still holds the default of appsettings.json. Outside Development and E2E it must be " +
                "the public address of this deployment, because the links of every e-mail start with it: set " +
                "APP_BASE_URL (see .env.example).");
        }
    }

    /// <summary>Scheme, host, port and path of the address without a closing slash; throws when it is not an absolute http(s) address.</summary>
    private static string Root(string? baseUrl)
    {
        // The scheme too: on Linux a path such as "/app" parses as an absolute file address.
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var root)
            || (root.Scheme != Uri.UriSchemeHttps && root.Scheme != Uri.UriSchemeHttp))
        {
            throw new InvalidOperationException($"{BaseUrlSetting} must be the absolute http(s) address of the client.");
        }

        return root.GetLeftPart(UriPartial.Authority) + root.AbsolutePath.TrimEnd('/');
    }
}
