// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ProcureAccess.Modules.Platform.RateLimiting;

/// <summary>
/// The client IP and the scheme behind the reverse proxies (ARCHITECTURE §9.2, owner decision
/// O-S4-69 (a); scheme since S5-07). The browser reaches the API through Traefik and the client's
/// nginx, so the connection always comes from nginx; the client's own address is in
/// <c>X-Forwarded-For</c> and the scheme the client used in <c>X-Forwarded-Proto</c>. ASP.NET's
/// forwarded-headers middleware takes them from there, but only from a proxy in a private or
/// loopback network and for at most <see cref="ForwardLimitKey"/> proxies (🔧, default 2:
/// Traefik → nginx → API).
/// </summary>
/// <remarks>
/// The address is used only to choose the counter of a rate limit; it is never stored or logged
/// (ARCHITECTURE §9.3). A header sent by a caller outside those networks is ignored, so it cannot
/// choose its own counter — as long as the API port itself is not reachable from the internet.
/// </remarks>
internal static class ClientAddress
{
    public const string ForwardLimitKey = "ForwardedHeaders:ForwardLimit";

    /// <summary>Partition of a request without a remote address (e.g. the in-memory test server).</summary>
    public const string Unknown = "unknown";

    /// <summary>
    /// Networks whose proxies are trusted to forward the client address: the private IPv4 ranges
    /// (Docker networks), loopback, and unique local IPv6 addresses.
    /// </summary>
    public static readonly IReadOnlyList<System.Net.IPNetwork> TrustedNetworks =
    [
        System.Net.IPNetwork.Parse("10.0.0.0/8"),
        System.Net.IPNetwork.Parse("172.16.0.0/12"),
        System.Net.IPNetwork.Parse("192.168.0.0/16"),
        System.Net.IPNetwork.Parse("127.0.0.0/8"),
        System.Net.IPNetwork.Parse("::1/128"),
        System.Net.IPNetwork.Parse("fc00::/7"),
    ];

    /// <summary>Length of the IPv6 prefix that counts as one client: one network, usually one household or host.</summary>
    private const int Ipv6ClientPrefixLength = 64;

    public static IServiceCollection AddClientAddress(this IServiceCollection services, IConfiguration configuration)
    {
        var forwardLimit = configuration.GetValue<int?>(ForwardLimitKey);
        if (forwardLimit is null or < 1)
        {
            throw new InvalidOperationException(
                $"{ForwardLimitKey} must be at least 1: the number of reverse proxies in front of the API (Traefik and nginx: 2).");
        }

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            // The address and, since S5-07 (owner decision 1a), the scheme: Traefik terminates TLS,
            // so the API sees plain HTTP and would refuse to issue or check its Secure cookies and
            // antiforgery tokens (SecurePolicy Always → InvalidOperationException on a non-HTTPS
            // request). X-Forwarded-Proto from the same trusted proxies restores Request.IsHttps;
            // nginx passes Traefik's value through (nginx.conf). The host stays that of the connection.
            // RequireHeaderSymmetry stays off (the default), so a For header without a Proto
            // header, as the rate-limit tests send it, still sets the address.
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.ForwardLimit = forwardLimit;
            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();
            foreach (var network in TrustedNetworks)
            {
                options.KnownIPNetworks.Add(network);
            }
        });

        return services;
    }

    /// <summary>Sets the connection's remote address to the forwarded client address. Runs first in the pipeline.</summary>
    public static IApplicationBuilder UseClientAddress(this IApplicationBuilder app) =>
        app.UseForwardedHeaders();

    /// <summary>
    /// The rate-limit partition of a request: the IPv4 address, or the /64 network of an IPv6
    /// address, because one IPv6 client can pick any address of its network.
    /// </summary>
    public static string PartitionOf(HttpContext context) => PartitionOf(context.Connection.RemoteIpAddress);

    internal static string PartitionOf(IPAddress? address)
    {
        if (address is null)
        {
            return Unknown;
        }

        if (address.IsIPv4MappedToIPv6)
        {
            return address.MapToIPv4().ToString();
        }

        if (address.AddressFamily != AddressFamily.InterNetworkV6)
        {
            return address.ToString();
        }

        return new System.Net.IPNetwork(Prefix(address), Ipv6ClientPrefixLength).ToString();
    }

    private static IPAddress Prefix(IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        Array.Clear(bytes, Ipv6ClientPrefixLength / 8, bytes.Length - (Ipv6ClientPrefixLength / 8));
        return new IPAddress(bytes);
    }
}
