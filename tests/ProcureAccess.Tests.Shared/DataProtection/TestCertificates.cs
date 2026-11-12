// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace ProcureAccess.Tests.Shared.DataProtection;

/// <summary>
/// Throwaway self-signed certificates for the key-ring encryption (S4-26), created in memory, in
/// the form the API reads from <c>DataProtection:Certificate</c>: a base64-encoded PFX without
/// password. Never a certificate of a real environment (CLAUDE.md §10).
/// </summary>
public static class TestCertificates
{
    /// <summary>A new RSA certificate with its private key, as the setting value.</summary>
    public static string NewRsaPfxBase64(string name = "ProcureAccess test key ring")
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest($"CN={name}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        return Convert.ToBase64String(certificate.Export(X509ContentType.Pfx));
    }

    /// <summary>An RSA certificate without its private key, as a PFX (it cannot read the keys it would encrypt).</summary>
    public static string PublicOnlyPfxBase64()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=public only", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        using var withKey = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        using var publicOnly = X509CertificateLoader.LoadCertificate(withKey.Export(X509ContentType.Cert));
        return Convert.ToBase64String(publicOnly.Export(X509ContentType.Pfx));
    }

    /// <summary>An ECDSA certificate with its private key (EncryptedXml needs RSA).</summary>
    public static string EcdsaPfxBase64()
    {
        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = new CertificateRequest("CN=ecdsa", ecdsa, HashAlgorithmName.SHA256);
        using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddYears(1));
        return Convert.ToBase64String(certificate.Export(X509ContentType.Pfx));
    }
}
