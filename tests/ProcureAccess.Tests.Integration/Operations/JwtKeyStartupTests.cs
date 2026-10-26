// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// The API stops at startup when the JWT signing key is missing or shorter than 32 bytes, instead
/// of failing every sign-in later (issue #65). Legacy until migration step 3 replaces the JWT.
/// A valid key needs no test of its own: every other integration test starts with a 64-byte key.
/// </summary>
public sealed class JwtKeyStartupTests(ApiFixture api)
{
    [Theory]
    [InlineData("")]
    [InlineData("change-me")]
    [InlineData("0123456789abcdef0123456789abcde")] // 31 bytes
    public async Task Startup_JwtKeyMissingOrShorterThan32Bytes_Fails(string key)
    {
        // The check runs while services are registered, before any database access.
        await using var factory = new ProcureAccessApiFactory(
            api.ConnectionStringForNewDatabase(),
            api.Clock,
            settings: new Dictionary<string, string?> { ["JWTSettings:Key"] = key });

        var exception = Should.Throw<InvalidOperationException>(() => factory.Services);

        exception.Message.ShouldContain("JWTSettings:Key");
        if (key.Length > 0)
        {
            exception.Message.ShouldNotContain(key);
        }
    }
}
