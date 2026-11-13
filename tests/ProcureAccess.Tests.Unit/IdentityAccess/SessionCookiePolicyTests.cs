// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using ProcureAccess.Modules.IdentityAccess.Session;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.IdentityAccess;

/// <summary>
/// <see cref="SessionCookiePolicy"/> (S5-07, owner decision 3a): the cookies are <c>Secure</c>
/// everywhere, and only in <c>Development</c> and <c>E2E</c> — the plain-HTTP stacks — only on an
/// HTTPS request.
/// </summary>
public class SessionCookiePolicyTests
{
    [Theory]
    [InlineData("Development")]
    [InlineData("E2E")]
    public void For_DevelopmentOrE2E_IsSecureOnlyOnHttpsRequests(string environmentName)
    {
        var policy = SessionCookiePolicy.For(Environment(environmentName));

        policy.SecurePolicy.ShouldBe(CookieSecurePolicy.SameAsRequest);
        policy.SecureFor(Request(https: false)).ShouldBeFalse();
        policy.SecureFor(Request(https: true)).ShouldBeTrue();
    }

    [Theory]
    [InlineData("Testing")]
    [InlineData("Staging")]
    [InlineData("Production")]
    public void For_EveryOtherEnvironment_IsAlwaysSecure(string environmentName)
    {
        var policy = SessionCookiePolicy.For(Environment(environmentName));

        policy.SecurePolicy.ShouldBe(CookieSecurePolicy.Always);
        policy.SecureFor(Request(https: false)).ShouldBeTrue();
    }

    private static IHostEnvironment Environment(string name)
    {
        var environment = Substitute.For<IHostEnvironment>();
        environment.EnvironmentName.Returns(name);
        return environment;
    }

    private static HttpContext Request(bool https)
    {
        var context = new DefaultHttpContext();
        context.Request.Scheme = https ? "https" : "http";
        return context;
    }
}
