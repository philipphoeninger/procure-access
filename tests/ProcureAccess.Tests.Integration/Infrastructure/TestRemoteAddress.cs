// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ProcureAccess.Tests.Integration.Infrastructure;

/// <summary>
/// Lets a test choose the address the connection comes from: the in-memory test server has none.
/// A request with <see cref="Header"/> gets that remote address before any middleware of the API
/// runs, so the API's forwarded-headers middleware sees it as the address of the proxy or client
/// (S4-21a). Add it to a test host with <see cref="AddTo"/>.
/// </summary>
internal sealed class TestRemoteAddress : IStartupFilter
{
    public const string Header = "X-Test-Remote-Address";

    public static void AddTo(IServiceCollection services) =>
        services.AddSingleton<IStartupFilter, TestRemoteAddress>();

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
    {
        app.Use((context, nextMiddleware) =>
        {
            if (context.Request.Headers.TryGetValue(Header, out var value))
            {
                context.Connection.RemoteIpAddress = IPAddress.Parse(value.ToString());
                context.Request.Headers.Remove(Header);
            }

            return nextMiddleware(context);
        });
        next(app);
    };
}
