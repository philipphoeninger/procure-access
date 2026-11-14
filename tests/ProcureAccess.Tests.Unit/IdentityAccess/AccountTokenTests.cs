// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Buffers.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using ProcureAccess.Modules.IdentityAccess.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Domain;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.IdentityAccess;

/// <summary>
/// The parts of the account tokens that need no database (S5-12a, ARCHITECTURE §9.1, C72): the
/// form of the link's value and its hash, the entity's rules and the 🔧 lifetimes. Issue and
/// redemption run against PostgreSQL in <c>RegistrationTests</c>.
/// </summary>
public sealed class AccountTokenTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 14, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void TryHash_TokenOf32Bytes_IsItsSha256()
    {
        var bytes = RandomNumberGenerator.GetBytes(AccountTokenService.TokenBytes);

        AccountTokenService.TryHash(Base64Url.EncodeToString(bytes), out var hash).ShouldBeTrue();

        hash.ShouldBe(SHA256.HashData(bytes));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not base64url!")]
    [InlineData("AAAA")]
    public void TryHash_NoToken_IsRefused(string? token) =>
        AccountTokenService.TryHash(token, out _).ShouldBeFalse();

    [Theory]
    [InlineData(31)]
    [InlineData(33)]
    public void TryHash_WrongLength_IsRefused(int length) =>
        AccountTokenService.TryHash(Base64Url.EncodeToString(new byte[length]), out _).ShouldBeFalse();

    [Fact]
    public void Create_ValidValues_ExpiresAfterTheLifetimeAndIsUnused()
    {
        var token = AccountToken.Create("user-1", AccountTokenPurpose.EmailConfirmation, new byte[32], Now, TimeSpan.FromHours(24));

        token.ExpiresAt.ShouldBe(Now.AddHours(24));
        token.UsedAt.ShouldBeNull();
        token.Id.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public void Create_InvalidValues_Throw()
    {
        Should.Throw<ArgumentException>(() => AccountToken.Create(" ", AccountTokenPurpose.EmailConfirmation, new byte[32], Now, TimeSpan.FromHours(1)));
        Should.Throw<ArgumentException>(() => AccountToken.Create("u", AccountTokenPurpose.EmailConfirmation, new byte[31], Now, TimeSpan.FromHours(1)));
        Should.Throw<ArgumentOutOfRangeException>(() => AccountToken.Create("u", (AccountTokenPurpose)99, new byte[32], Now, TimeSpan.FromHours(1)));
        Should.Throw<ArgumentOutOfRangeException>(() => AccountToken.Create("u", AccountTokenPurpose.EmailConfirmation, new byte[32], Now, TimeSpan.Zero));
    }

    [Theory]
    [InlineData(24, 24)]
    [InlineData(23.5, 24)]
    [InlineData(0.1, 1)]
    public void ValidHours_Lifetime_IsRoundedUpToWholeHours(double hours, int expected) =>
        new IssuedAccountToken("t", Now, TimeSpan.FromHours(hours)).ValidHours.ShouldBe(expected);

    [Fact]
    public void IssuedToken_ToString_NeverShowsTheValue() =>
        new IssuedAccountToken("secret-value", Now, TimeSpan.FromHours(1)).ToString().ShouldNotContain("secret-value");

    [Fact]
    public void Settings_ConfiguredLifetime_IsRead()
    {
        var settings = AccountTokenSettings.From(Configuration("1.00:00:00"));

        settings.LifetimeOf(AccountTokenPurpose.EmailConfirmation).ShouldBe(TimeSpan.FromHours(24));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("00:00:00")]
    [InlineData("-01:00:00")]
    [InlineData("a day")]
    public void Settings_MissingOrUnusableLifetime_StopsTheStart(string? value) =>
        Should.Throw<InvalidOperationException>(() => AccountTokenSettings.From(Configuration(value)))
              .Message.ShouldContain("Account:Tokens:EmailConfirmationLifetime");

    [Fact]
    public void Settings_DefaultOfAppsettings_Is24Hours()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: false)
            .Build();

        AccountTokenSettings.From(configuration).EmailConfirmationLifetime.ShouldBe(TimeSpan.FromHours(24));
    }

    private static IConfiguration Configuration(string? lifetime) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Account:Tokens:EmailConfirmationLifetime"] = lifetime })
            .Build();
}
