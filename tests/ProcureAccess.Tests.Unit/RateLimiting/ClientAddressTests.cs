// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.Platform.RateLimiting;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Unit.RateLimiting;

/// <summary>The rate-limit partition of a client address and the proxy settings (S4-21a, O-S4-69 (a)).</summary>
public sealed class ClientAddressTests
{
    [Theory]
    [InlineData("203.0.113.10", "203.0.113.10")]
    [InlineData("::ffff:203.0.113.10", "203.0.113.10")]
    [InlineData("2001:db8:1:2:aaaa:bbbb:cccc:dddd", "2001:db8:1:2::/64")]
    [InlineData("2001:db8:1:2::1", "2001:db8:1:2::/64")]
    [InlineData("2001:db8:1:3::1", "2001:db8:1:3::/64")]
    public void PartitionOf_Address_IsTheIPv4AddressOrTheIPv6Network(string address, string expected) =>
        ClientAddress.PartitionOf(IPAddress.Parse(address)).ShouldBe(expected);

    [Fact]
    public void PartitionOf_NoAddress_IsUnknown() =>
        ClientAddress.PartitionOf((IPAddress?)null).ShouldBe(ClientAddress.Unknown);

    [Theory]
    [InlineData("172.18.0.3", true)]
    [InlineData("10.1.2.3", true)]
    [InlineData("192.168.1.1", true)]
    [InlineData("127.0.0.1", true)]
    [InlineData("::1", true)]
    [InlineData("fd00::1", true)]
    [InlineData("172.32.0.1", false)]
    [InlineData("203.0.113.10", false)]
    [InlineData("2001:db8::1", false)]
    public void TrustedNetworks_Address_TrustedOnlyInPrivateAndLoopbackNetworks(string address, bool trusted) =>
        ClientAddress.TrustedNetworks.Any(n => n.Contains(IPAddress.Parse(address))).ShouldBe(trusted);

    [Theory]
    [InlineData(null)]
    [InlineData("0")]
    [InlineData("-1")]
    public void AddClientAddress_NoForwardLimitOfAtLeastOne_FailsTheStart(string? forwardLimit)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [ClientAddress.ForwardLimitKey] = forwardLimit })
            .Build();

        Should.Throw<InvalidOperationException>(() => new ServiceCollection().AddClientAddress(configuration))
            .Message.ShouldContain(ClientAddress.ForwardLimitKey);
    }
}
