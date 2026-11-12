// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using ProcureAccess.Modules.IdentityAccess.DataProtection;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Tests.Integration.Infrastructure;
using ProcureAccess.Tests.Shared.DataProtection;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// The Data Protection key ring lives in the database (ARCHITECTURE §9.1, C60; S3-24, #186), so
/// what one API instance protects, the next one on the same database can read: a new container no
/// longer invalidates Identity tokens (e-mail confirmation, password reset) and, from step 3,
/// session cookies and antiforgery tokens. Since S4-26 (#235, owner decisions O-S4-87 – O-S4-89) the
/// keys are encrypted at rest with a certificate: the factory gives every host the certificate of
/// the test run, and the tests below pass their own.
/// </summary>
/// <remarks>
/// Prepares OPS-04, which stays planned until the cookie session of step 3: its test signs in on
/// one host and sends the cookie to a second host on the same database. Each test here uses
/// databases of its own, so the hosts start without keys.
/// </remarks>
public sealed class DataProtectionKeyStoreTests(ApiFixture api) : IAsyncDisposable
{
    private const string Purpose = "ProcureAccess.Tests.DataProtectionKeyStore";
    private const string Secret = "survives a new container";

    private readonly List<string> _databases = [];

    [Fact]
    public async Task Unprotect_PayloadOfAnEarlierHostOnTheSameDatabase_ReturnsTheOriginal()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();

        string payload;
        await using (var first = new ProcureAccessApiFactory(connectionString, api.Clock))
        {
            payload = Protector(first).Protect(Secret);
        }

        await using (var identity = IdentityAccess(connectionString))
        {
            (await identity.DataProtectionKeys.CountAsync(cancellationToken)).ShouldBe(1);
        }

        await using var second = new ProcureAccessApiFactory(connectionString, api.Clock);
        Protector(second).Unprotect(payload).ShouldBe(Secret);
    }

    [Fact]
    public async Task Unprotect_PayloadOfAHostOnAnotherDatabase_Fails()
    {
        // Shows that the key comes from the database: with the file-system key ring both hosts of
        // a test run would share ~/.aspnet/DataProtection-Keys and read each other's payloads.
        string payload;
        await using (var first = new ProcureAccessApiFactory(NewDatabase(), api.Clock))
        {
            payload = Protector(first).Protect(Secret);
        }

        await using var other = new ProcureAccessApiFactory(NewDatabase(), api.Clock);
        Should.Throw<CryptographicException>(() => Protector(other).Unprotect(payload));
    }

    [Fact]
    public async Task StoredKey_WithACertificate_IsEncrypted()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();

        await using (var host = new ProcureAccessApiFactory(connectionString, api.Clock))
        {
            Protector(host).Protect(Secret);
        }

        await using var identity = IdentityAccess(connectionString);
        var xml = (await identity.DataProtectionKeys.SingleAsync(cancellationToken)).Xml.ShouldNotBeNull();
        xml.ShouldContain("<encryptedSecret");
        xml.ShouldNotContain("<masterKey");
    }

    [Fact]
    public async Task Unprotect_HostWithoutTheCertificate_Fails()
    {
        // Whoever reads the database (or a pg_dump) without the certificate cannot use the keys.
        var connectionString = NewDatabase();

        string payload;
        await using (var first = new ProcureAccessApiFactory(connectionString, api.Clock))
        {
            payload = Protector(first).Protect(Secret);
        }

        await using var withoutCertificate = new ProcureAccessApiFactory(
            connectionString, api.Clock, Environments.Development, Certificates(current: ""));
        Should.Throw<CryptographicException>(() => Protector(withoutCertificate).Unprotect(payload));
    }

    [Fact]
    public async Task Unprotect_AfterARotationWithThePreviousCertificate_ReturnsTheOriginal()
    {
        var connectionString = NewDatabase();
        var old = TestCertificates.NewRsaPfxBase64("old");

        string payload;
        await using (var first = new ProcureAccessApiFactory(
                         connectionString, api.Clock, settings: Certificates(current: old)))
        {
            payload = Protector(first).Protect(Secret);
        }

        await using var rotated = new ProcureAccessApiFactory(
            connectionString, api.Clock, settings: Certificates(current: TestCertificates.NewRsaPfxBase64("new"), previous: old));
        Protector(rotated).Unprotect(payload).ShouldBe(Secret);
    }

    [Fact]
    public async Task Unprotect_KeyStoredUnencryptedBeforeTheCertificate_IsStillRead()
    {
        // Data Protection keeps reading a plain key once a certificate is configured, so plain rows
        // stay in the table until they are deleted by hand (O-S4-89 (a)).
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();

        string payload;
        await using (var unencrypted = new ProcureAccessApiFactory(
                         connectionString, api.Clock, Environments.Development, Certificates(current: "")))
        {
            payload = Protector(unencrypted).Protect(Secret);
        }

        await using (var identity = IdentityAccess(connectionString))
        {
            (await identity.DataProtectionKeys.SingleAsync(cancellationToken)).Xml.ShouldNotBeNull().ShouldContain("<masterKey");
        }

        await using var encrypted = new ProcureAccessApiFactory(connectionString, api.Clock);
        Protector(encrypted).Unprotect(payload).ShouldBe(Secret);
    }

    public async ValueTask DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        foreach (var connectionString in _databases)
        {
            await using var context = IdentityAccess(connectionString);
            await context.Database.EnsureDeletedAsync();
        }
    }

    private static Dictionary<string, string?> Certificates(string current, string? previous = null) => new()
    {
        [KeyRingCertificates.CertificateSettingName] = current,
        [KeyRingCertificates.PreviousCertificateSettingName] = previous,
    };

    private static IDataProtector Protector(ProcureAccessApiFactory factory) =>
        factory.Services.GetRequiredService<IDataProtectionProvider>().CreateProtector(Purpose);

    private string NewDatabase()
    {
        var connectionString = api.ConnectionStringForNewDatabase();
        _databases.Add(connectionString);
        return connectionString;
    }

    private static IdentityAccessDbContext IdentityAccess(string connectionString) =>
        new(new DbContextOptionsBuilder<IdentityAccessDbContext>().UseNpgsql(connectionString).Options);
}
