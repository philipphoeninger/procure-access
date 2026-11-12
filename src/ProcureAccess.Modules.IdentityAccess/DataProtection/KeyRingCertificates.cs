// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace ProcureAccess.Modules.IdentityAccess.DataProtection;

/// <summary>
/// The certificates that encrypt the Data Protection key ring at rest (S4-26, #235; owner decisions
/// O-S4-87 – O-S4-88 (a)). Both come from the environment as a base64-encoded PKCS #12 (PFX) file
/// with an RSA private key and without password, never from the repository:
/// <see cref="CertificateSettingName"/> encrypts every new key and reads the keys it encrypted;
/// <see cref="PreviousCertificateSettingName"/> (optional) only reads keys encrypted before a
/// rotation. Only <c>Development</c> and <c>E2E</c> may run without a certificate (keys stored in
/// plain text, as before); every other environment stops at startup without one, and a value that
/// is set but cannot be used stops the start everywhere.
/// </summary>
internal sealed record KeyRingCertificates(X509Certificate2? Current, X509Certificate2? Previous)
{
    public const string CertificateSettingName = "DataProtection:Certificate";
    public const string PreviousCertificateSettingName = "DataProtection:PreviousCertificate";

    /// <summary>Environment of the E2E stack once it has its own (TESTING §3.3); until then it runs as Development.</summary>
    public const string E2EEnvironmentName = "E2E";

    /// <summary>True only in <c>Development</c> and <c>E2E</c>: local stacks and CI need no secret.</summary>
    public static bool MayStoreKeysUnencrypted(IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(environment);
        return environment.IsDevelopment() || environment.IsEnvironment(E2EEnvironmentName);
    }

    /// <summary>Reads both certificates; throws <see cref="InvalidOperationException"/> to stop the start.</summary>
    public static KeyRingCertificates From(IConfiguration configuration, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var current = Load(configuration, CertificateSettingName);
        var previous = Load(configuration, PreviousCertificateSettingName);

        if (current is null && previous is not null)
        {
            previous.Dispose();
            throw new InvalidOperationException(
                $"{PreviousCertificateSettingName} is set without {CertificateSettingName}: the previous certificate " +
                "only reads old keys, new keys need the current one.");
        }

        if (current is null && !MayStoreKeysUnencrypted(environment))
        {
            throw new InvalidOperationException(
                $"{CertificateSettingName} is not set. Outside Development and E2E the Data Protection key ring " +
                "must be encrypted at rest: set it to a base64-encoded PFX file with an RSA private key and " +
                "without password (see .env.example, DATA_PROTECTION_CERTIFICATE).");
        }

        return new KeyRingCertificates(current, previous);
    }

    /// <summary>Loads one certificate; null when the setting is empty. The value never goes into a message.</summary>
    private static X509Certificate2? Load(IConfiguration configuration, string settingName)
    {
        var value = configuration[settingName];
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        byte[] pfx;
        try
        {
            pfx = Convert.FromBase64String(value.Trim());
        }
        catch (FormatException)
        {
            throw new InvalidOperationException($"{settingName} is not base64.");
        }

        X509Certificate2 certificate;
        try
        {
            certificate = X509CertificateLoader.LoadPkcs12(pfx, password: null);
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException($"{settingName} is not a PFX file without password.", ex);
        }

        // EncryptedXml encrypts the key with the certificate's RSA public key and needs the private
        // key to read it again.
        using var rsa = certificate.GetRSAPrivateKey();
        if (rsa is null)
        {
            certificate.Dispose();
            throw new InvalidOperationException($"{settingName} must hold a certificate with an RSA private key.");
        }

        return certificate;
    }
}
