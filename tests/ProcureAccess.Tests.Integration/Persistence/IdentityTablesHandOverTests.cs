// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Net;
using System.Net.Http.Json;
using DAL.ProcureAccess.EFStructures;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;
using ProcureAccess.Tests.Integration.Infrastructure;
using Shouldly;
using Xunit;

namespace ProcureAccess.Tests.Integration.Persistence;

/// <summary>
/// OPS-03 for the hand-over of the users (S6-10, ARCHITECTURE ADR-014 rule 4; owner decision
/// O-S6-1 (a)): the seven ASP.NET Core Identity tables left the legacy context
/// (<c>HandOverIdentityTables</c>) and the IdentityAccess context adopts them
/// (<c>AdoptIdentityTables</c>). Both ways of building a database must give the same tables, and a
/// database that holds accounts keeps them: the rows are untouched and the old password signs in.
/// Each test uses databases of its own on the shared container.
/// </summary>
public sealed class IdentityTablesHandOverTests(ApiFixture api) : IAsyncDisposable
{
    /// <summary>The last legacy migration before the hand-over.</summary>
    private const string LegacyBefore = "20261116224059_DropUserUICustomization";

    /// <summary>The last IdentityAccess migration before the adoption.</summary>
    private const string IdentityBefore = "20261116211358_AddUserPreferences";

    private const string Email = "existing.account@example.test";
    private const string Password = "Existing-Account-1!";

    private static readonly string[] IdentityTables =
        ["Users", "Roles", "RoleClaims", "UserClaims", "UserLogins", "UserRoles", "UserTokens"];

    private readonly List<string> _databases = [];

    [Fact(DisplayName = "INV:OPS-03 " + nameof(IdentityAccessMigrations_EmptyDatabaseWithoutLegacy_CreateTheTablesTheLegacyMigrationsCreated))]
    [Trait("Invariant", "OPS-03")]
    public async Task IdentityAccessMigrations_EmptyDatabaseWithoutLegacy_CreateTheTablesTheLegacyMigrationsCreated()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var withLegacy = NewDatabase();
        var moduleOnly = NewDatabase();

        await using (var legacy = Legacy(withLegacy))
        {
            await legacy.Database.MigrateAsync(cancellationToken);
        }

        await using (var identity = IdentityAccess(withLegacy))
        {
            await identity.Database.MigrateAsync(cancellationToken);
        }

        await using (var identity = IdentityAccess(moduleOnly))
        {
            await identity.Database.MigrateAsync(cancellationToken);
            (await identity.Database.GetPendingMigrationsAsync(cancellationToken)).ShouldBeEmpty();
        }

        var expected = await LayoutAsync(withLegacy);
        expected.Count(line => line.Contains(" column ", StringComparison.Ordinal)).ShouldBe(40);
        (await LayoutAsync(moduleOnly)).ShouldBe(expected);

        // The platform roles and the moderators' permission exist both ways, each once; PlatformAdmin
        // lost it with the proposals (RevokePlatformAdminModerationReview, S6-23).
        (await RolesWithPermissionAsync(moduleOnly)).ShouldBe(await RolesWithPermissionAsync(withLegacy));
        (await RolesWithPermissionAsync(moduleOnly)).ShouldBe(
            [$"{PlatformRoles.CatalogEditor}:", $"{PlatformRoles.Moderator}:moderation:review", $"{PlatformRoles.PlatformAdmin}:"]);
    }

    [Fact(DisplayName = "INV:OPS-03 " + nameof(Migrate_DatabaseWithAccountsFromBeforeTheHandOver_KeepsTheRowsAndTheSignIn))]
    [Trait("Invariant", "OPS-03")]
    public async Task Migrate_DatabaseWithAccountsFromBeforeTheHandOver_KeepsTheRowsAndTheSignIn()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var connectionString = NewDatabase();

        // A database as staging holds it: both contexts at their last migration before S6-10.
        await using (var legacy = Legacy(connectionString))
        {
            await legacy.GetService<IMigrator>().MigrateAsync(LegacyBefore, cancellationToken);
        }

        User before;
        Guid organizationId;
        await using (var identity = IdentityAccess(connectionString))
        {
            await identity.GetService<IMigrator>().MigrateAsync(IdentityBefore, cancellationToken);

            before = new User
            {
                UserName = Email,
                NormalizedUserName = Email.ToUpperInvariant(),
                Email = Email,
                NormalizedEmail = Email.ToUpperInvariant(),
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString(),
                LockoutEnabled = true,
                CreatedAt = api.Clock.GetUtcNow(),
            };
            before.PasswordHash = new PasswordHasher<User>().HashPassword(before, Password);
            identity.Users.Add(before);

            var moderator = await identity.Roles.SingleAsync(r => r.Name == PlatformRoles.Moderator, cancellationToken);
            identity.UserRoles.Add(new IdentityUserRole<string> { UserId = before.Id, RoleId = moderator.Id });

            var organization = Organization.Create("Bestand", OrganizationType.PublicBody, before.Id, api.Clock.GetUtcNow());
            identity.Organizations.Add(organization);
            identity.OrganizationMemberships.Add(OrganizationMembership.Founder(organization, api.Clock.GetUtcNow()));
            organizationId = organization.Id;
            await identity.SaveChangesAsync(cancellationToken);
        }

        // The start migrates the legacy context, then the modules (Program.cs).
        await using (var host = new ProcureAccessApiFactory(connectionString, api.Clock, realAuthentication: true))
        {
            var cookies = new CookieContainer();
            using var client = host.CreateDefaultClient(
                IntegrationTest.BaseAddress, new AntiforgeryHandler(cookies), new CookieContainerHandler(cookies));

            using var login = await client.PostAsJsonAsync(
                "/api/v1/auth/login", new { email = Email, password = Password }, cancellationToken);
            login.StatusCode.ShouldBe(HttpStatusCode.NoContent);

            using var organizations = await client.GetAsync("/api/v1/organizations", cancellationToken);
            organizations.StatusCode.ShouldBe(HttpStatusCode.OK);
            (await organizations.Content.ReadAsStringAsync(cancellationToken)).ShouldContain(organizationId.ToString());
        }

        NpgsqlConnection.ClearAllPools();
        await using var after = IdentityAccess(connectionString);
        (await after.Database.GetPendingMigrationsAsync(cancellationToken)).ShouldBeEmpty();
        await using (var legacy = Legacy(connectionString))
        {
            (await legacy.Database.GetPendingMigrationsAsync(cancellationToken)).ShouldBeEmpty();
        }

        var user = await after.Users.AsNoTracking().SingleAsync(cancellationToken);
        user.Id.ShouldBe(before.Id);
        user.Email.ShouldBe(Email);
        user.PasswordHash.ShouldBe(before.PasswordHash);
        user.SecurityStamp.ShouldBe(before.SecurityStamp);
        user.EmailConfirmed.ShouldBeTrue();
        user.CreatedAt.ShouldBe(before.CreatedAt);
        (await after.UserRoles.AsNoTracking().CountAsync(r => r.UserId == before.Id, cancellationToken)).ShouldBe(1);

        // No legacy table points at the users with a constraint (ADR-014 rule 5); the proposals, which
        // did until the hand-over, are gone since RetireProposals (S6-23).
        (await ScalarAsync(connectionString, """
            SELECT count(*) FROM pg_constraint c
            JOIN pg_class t ON t.oid = c.conrelid
            JOIN pg_namespace n ON n.oid = t.relnamespace
            WHERE n.nspname = 'public' AND c.confrelid = 'identity."Users"'::regclass
            """)).ShouldBe(0);
        (await ScalarAsync(connectionString, """
            SELECT count(*) FROM pg_tables WHERE schemaname = 'public' AND tablename = 'Proposals'
            """)).ShouldBe(0);
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

    private string NewDatabase()
    {
        var connectionString = api.ConnectionStringForNewDatabase();
        _databases.Add(connectionString);
        return connectionString;
    }

    private static ApplicationDBContext Legacy(string connectionString) =>
        new(new DbContextOptionsBuilder<ApplicationDBContext>().UseNpgsql(connectionString).Options);

    private static IdentityAccessDbContext IdentityAccess(string connectionString) =>
        new(new DbContextOptionsBuilder<IdentityAccessDbContext>().UseNpgsql(connectionString).Options, FixedOrganizationContext.None);

    /// <summary>
    /// Columns (type, length, nullability, default, identity), index definitions and constraints of
    /// the seven Identity tables, one line each, in a fixed order. Column order is left out: the
    /// legacy migrations added <c>IsDeleted</c>, <c>DeletedAt</c> and <c>CreatedAt</c> at the end.
    /// </summary>
    private static async Task<List<string>> LayoutAsync(string connectionString)
    {
        const string sql = """
            SELECT table_name || ' column ' || column_name || ' ' || data_type
                   || coalesce('(' || character_maximum_length || ')', '')
                   || ' nullable=' || is_nullable || ' default=' || coalesce(column_default, '-')
                   || ' identity=' || is_identity || coalesce(' ' || identity_generation, '')
            FROM information_schema.columns
            WHERE table_schema = 'identity' AND table_name = ANY(@tables)
            UNION ALL
            SELECT tablename || ' index ' || indexdef FROM pg_indexes
            WHERE schemaname = 'identity' AND tablename = ANY(@tables)
            UNION ALL
            SELECT t.relname || ' constraint ' || c.conname || ' ' || pg_get_constraintdef(c.oid)
            FROM pg_constraint c
            JOIN pg_class t ON t.oid = c.conrelid
            JOIN pg_namespace n ON n.oid = t.relnamespace
            WHERE n.nspname = 'identity' AND t.relname = ANY(@tables) AND c.contype <> 'n'
            ORDER BY 1
            """;
        return await LinesAsync(connectionString, sql);
    }

    private static Task<List<string>> RolesWithPermissionAsync(string connectionString) =>
        LinesAsync(connectionString, """
            SELECT r."Name" || ':' || coalesce(c."ClaimValue", '')
            FROM identity."Roles" r
            LEFT JOIN identity."RoleClaims" c ON c."RoleId" = r."Id" AND c."ClaimType" = 'permission'
            WHERE @tables IS NOT NULL
            ORDER BY 1
            """);

    private static async Task<List<string>> LinesAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("tables", IdentityTables);
        var lines = new List<string>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lines.Add(reader.GetString(0));
        }

        return lines;
    }

    private static async Task<long> ScalarAsync(string connectionString, string sql)
    {
        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        return (long)(await command.ExecuteScalarAsync())!;
    }
}
