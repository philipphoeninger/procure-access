// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Npgsql;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Operations;

/// <summary>
/// The API applies the migrations at startup and stops when that fails, instead of serving
/// requests against a database whose schema does not match the model.
/// </summary>
public sealed class MigrationStartupTests(ApiFixture api)
{
    [Fact]
    public async Task Startup_MigrationFails_StopsWithTheError()
    {
        // Wrong credentials make Database.Migrate() fail before it changes anything.
        var connectionString = new NpgsqlConnectionStringBuilder(api.ConnectionStringForNewDatabase())
        {
            Password = "wrong-password",
        }.ConnectionString;
        await using var factory = new ProcureAccessApiFactory(connectionString, api.Clock);

        var exception = Should.Throw<Exception>(() => factory.Services);

        exception.ShouldBeAssignableTo<NpgsqlException>();
    }
}
