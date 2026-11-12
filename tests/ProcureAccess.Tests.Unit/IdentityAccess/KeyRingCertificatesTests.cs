// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using ProcureAccess.Modules.IdentityAccess.DataProtection;
using ProcureAccess.Tests.Shared.DataProtection;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.IdentityAccess;

/// <summary>
/// The certificates of the key-ring encryption (S4-26, #235; owner decisions O-S4-87 (a), O-S4-88 (a)):
/// required outside Development and E2E, and a value that is set must be usable in every environment.
/// </summary>
public class KeyRingCertificatesTests
{
    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    [InlineData("Testing")]
    [InlineData("SomethingElse")]
    public void From_NoCertificateOutsideDevelopmentAndE2E_StopsTheStart(string environment) =>
        Should.Throw<InvalidOperationException>(() => KeyRingCertificates.From(Settings(), In(environment)))
            .Message.ShouldContain(KeyRingCertificates.CertificateSettingName);

    [Theory]
    [InlineData("Development")]
    [InlineData("E2E")]
    public void From_NoCertificateInDevelopmentOrE2E_KeepsTheKeysUnencrypted(string environment)
    {
        var certificates = KeyRingCertificates.From(Settings(), In(environment));

        certificates.Current.ShouldBeNull();
        certificates.Previous.ShouldBeNull();
    }

    [Fact]
    public void From_WhitespaceCertificate_CountsAsNotSet() =>
        Should.Throw<InvalidOperationException>(() => KeyRingCertificates.From(Settings(current: "  "), In("Production")));

    [Fact]
    public void From_RsaPfxWithoutPassword_IsTaken()
    {
        var certificates = KeyRingCertificates.From(
            Settings(current: TestCertificates.NewRsaPfxBase64("current"), previous: TestCertificates.NewRsaPfxBase64("previous")),
            In("Production"));

        certificates.Current.ShouldNotBeNull().Subject.ShouldBe("CN=current");
        certificates.Current.HasPrivateKey.ShouldBeTrue();
        certificates.Previous.ShouldNotBeNull().Subject.ShouldBe("CN=previous");
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Production")]
    public void From_UnusableCertificate_StopsTheStartInEveryEnvironment(string environment)
    {
        string[] unusable = ["not base64 !", Convert.ToBase64String("not a pfx"u8.ToArray()),
            TestCertificates.PublicOnlyPfxBase64(), TestCertificates.EcdsaPfxBase64()];

        foreach (var value in unusable)
        {
            Should.Throw<InvalidOperationException>(() => KeyRingCertificates.From(Settings(current: value), In(environment)))
                .Message.ShouldContain(KeyRingCertificates.CertificateSettingName);
        }
    }

    [Fact]
    public void From_UnusablePreviousCertificate_StopsTheStart() =>
        Should.Throw<InvalidOperationException>(() => KeyRingCertificates.From(
                Settings(current: TestCertificates.NewRsaPfxBase64(), previous: TestCertificates.PublicOnlyPfxBase64()),
                In("Production")))
            .Message.ShouldContain(KeyRingCertificates.PreviousCertificateSettingName);

    [Fact]
    public void From_PreviousCertificateWithoutCurrent_StopsTheStart() =>
        Should.Throw<InvalidOperationException>(() => KeyRingCertificates.From(
                Settings(previous: TestCertificates.NewRsaPfxBase64()), In("Development")))
            .Message.ShouldContain(KeyRingCertificates.CertificateSettingName);

    [Fact]
    public void From_UnusableCertificate_NeverPutsTheValueIntoTheMessage()
    {
        const string value = "c2VjcmV0LXZhbHVl";

        var exception = Should.Throw<InvalidOperationException>(() => KeyRingCertificates.From(Settings(current: value), In("Production")));

        exception.Message.ShouldNotContain(value);
        exception.InnerException?.Message.ShouldNotContain(value);
    }

    private static IHostEnvironment In(string environmentName) => new HostingEnvironment { EnvironmentName = environmentName };

    private static IConfiguration Settings(string? current = null, string? previous = null) =>
        new ConfigurationBuilder().AddInMemoryCollection([
            new(KeyRingCertificates.CertificateSettingName, current),
            new(KeyRingCertificates.PreviousCertificateSettingName, previous),
        ]).Build();
}
