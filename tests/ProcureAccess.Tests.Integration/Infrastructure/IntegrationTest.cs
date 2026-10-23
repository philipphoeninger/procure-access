// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.Extensions.Time.Testing;
using ProcureAccess.Tests.Shared.Auth;
using Xunit;

namespace ProcureAccess.Tests.Integration.Infrastructure;

/// <summary>
/// Base class of every integration test class. The database is emptied once before the class
/// runs (TESTING §5.2); each test stores the data it needs with <see cref="StoreAsync"/>.
/// </summary>
public abstract class IntegrationTest(ApiFixture api) : IClassFixture<DatabaseReset>
{
    /// <summary>Base address of the test client. HTTPS, so cookies marked <c>Secure</c> are kept.</summary>
    public static readonly Uri BaseAddress = new("https://localhost");

    protected ApiFixture Api { get; } = api;

    /// <summary>The clock the API reads; see <see cref="ApiFixture.Clock"/>.</summary>
    protected FakeTimeProvider Clock => Api.Clock;

    /// <summary>
    /// Creates a client with its own cookie jar and the antiforgery hook. Without a
    /// <paramref name="user"/> the client is anonymous.
    /// </summary>
    protected HttpClient CreateClient(TestUser? user = null)
    {
        var cookies = new CookieContainer();
        var client = Api.Factory.CreateDefaultClient(
            BaseAddress,
            new AntiforgeryHandler(cookies),
            new CookieContainerHandler(cookies));

        if (user is not null)
        {
            client.DefaultRequestHeaders.Add(TestAuthHandler.UserHeader, user.Name);
        }

        return client;
    }

    /// <inheritdoc cref="ApiFixture.StoreAsync"/>
    protected Task StoreAsync(params object[] entities) => Api.StoreAsync(entities);
}

/// <summary>Class fixture that empties the database once per test class.</summary>
public sealed class DatabaseReset(ApiFixture api) : IAsyncLifetime
{
    public async ValueTask InitializeAsync() => await api.ResetDatabaseAsync();

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
