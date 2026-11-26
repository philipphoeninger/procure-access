// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProcureAccess.Modules.IdentityAccess.Accounts;
using ProcureAccess.Modules.IdentityAccess.Auth;
using ProcureAccess.Modules.IdentityAccess.Commands;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Contracts.Organizations;
using ProcureAccess.Modules.IdentityAccess.DataProtection;
using ProcureAccess.Modules.IdentityAccess.Organizations;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.IdentityAccess.Session;
using ProcureAccess.Modules.IdentityAccess.Tenancy;
using ProcureAccess.Modules.Platform.Contracts.Persistence;
using ProcureAccess.Modules.Platform.Contracts.Tenancy;

namespace ProcureAccess.Modules.IdentityAccess;

/// <summary>
/// Registration of the IdentityAccess module (ARCHITECTURE §4.3): users, memberships, contributor
/// profiles, roles and the Data Protection key store.
/// Since S3-24 it holds the key store: the Data Protection key ring lives in
/// <c>identity."DataProtectionKeys"</c> instead of the container's file system, so Identity tokens
/// today, and the session cookie and antiforgery tokens from step 3, stay valid when the container
/// is replaced (§9.1, C60). Since S4-26 the keys are encrypted at rest with a certificate from the
/// environment (<see cref="KeyRingCertificates"/>).
/// Since S6-10 it holds the users and roles themselves (ADR-014 rule 4; TD-72): ASP.NET Core
/// Identity on <see cref="IdentityAccessDbContext"/>, the endpoints of sign-in, registration,
/// password reset, account and organisations, the <c>roles</c> command and the seeded users. The
/// Identity cookie is the only authentication scheme (S5-08).
/// </summary>
public static class IdentityAccessModule
{
    /// <summary>Name of the connection string of the application database.</summary>
    private const string ConnectionStringName = "ProcureAccess";

    /// <summary>
    /// Data Protection application name. Fixed, because the default is the content root path:
    /// a host started from another folder (local run, container) would otherwise not read the keys.
    /// </summary>
    private const string DataProtectionApplicationName = "ProcureAccess";

    /// <summary>Registers the services of the IdentityAccess module. Called once from <c>Program.cs</c>.</summary>
    public static IServiceCollection AddIdentityAccessModule(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        // Same database and connection string as the other contexts (ADR-014).
        var connectionString = configuration.GetConnectionString(ConnectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{ConnectionStringName}' is not configured. " +
                $"Set the environment variable ConnectionStrings__{ConnectionStringName} (see .env.example).");
        }

        // The address of the client in every e-mail link (S7-09, #447): a deployment that still has
        // the default of appsettings.json stops here, before the host is built.
        ClientLinks.EnsureDeploymentAddress(configuration, environment);

        services.AddNpgsql<IdentityAccessDbContext>(connectionString, options =>
        {
            options.EnableRetryOnFailure().CommandTimeout(60);
        });
        services.AddScoped<IModuleDatabaseMigrator, IdentityAccessDatabaseMigrator>();

        // Users and roles (S6-10): ASP.NET Core Identity on this context, the 🔧 password and
        // lockout rules and the e-mail confirmation gate (SEC-10, SEC-11).
        services.AddIdentityAccessIdentity(configuration);
        services.AddScoped<IUserDirectory, UserDirectory>();
        services.AddScoped<SeedUsers>();

        // Password-reset e-mails go out in the background, so a known address answers as fast as an
        // unknown one (S5-13a, ARCHITECTURE C141); only a started web host runs the mailer.
        services.AddSingleton<PasswordResetMailQueue>();
        services.AddHostedService<PasswordResetMailer>();

        // Key ring in the database (ARCHITECTURE §9.1, C60), encrypted at rest with a certificate
        // (S4-26, owner decisions O-S4-87 – O-S4-88 (a)). Read while the services are registered, so
        // a missing or unusable certificate stops the start before the host is built.
        var certificates = KeyRingCertificates.From(configuration, environment);
        var dataProtection = services.AddDataProtection()
                                     .SetApplicationName(DataProtectionApplicationName)
                                     .PersistKeysToDbContext<IdentityAccessDbContext>();
        if (certificates.Current is not null)
        {
            // Encrypts new keys and reads the keys it encrypted. Keys stored in plain text earlier
            // are still read (O-S4-89: deleted once by hand).
            dataProtection.ProtectKeysWithCertificate(certificates.Current);
        }

        if (certificates.Previous is not null)
        {
            // Rotation: reads keys encrypted with the previous certificate until they are no longer needed.
            dataProtection.UnprotectKeysWithAnyCertificate(certificates.Previous);
        }

        // Cookie session and antiforgery (S5-07, ARCHITECTURE §9.1): the Identity cookie schemes,
        // the session lifetimes of 🔧 Session:* and the antiforgery token pair; Secure cookies only on
        // HTTPS requests in Development and E2E (SessionCookiePolicy).
        services.AddSession(configuration, environment);

        // Single-use, hashed tokens of the account e-mails (S5-12a, C72); 🔧 lifetimes read at start.
        // Expired tokens, used or not, are deleted by a background job (S6-05, TD-59), which only
        // the web host starts.
        services.AddSingleton(AccountTokenSettings.From(configuration));
        services.AddScoped<IAccountTokens, AccountTokenService>();
        services.AddHostedService<AccountTokenCleanupService>();

        // The memberships and contributor profiles of the personal data export (S5-27, SPEC §8.4).
        services.AddScoped<IAccountDataExport, AccountDataExportService>();

        // The organisation rules and the module's rows of the account deletion (S5-28a, TEN-11).
        services.AddScoped<IAccountDeletion, AccountDeletionService>();

        // The user's settings (S5-25a, SPEC R-SET-01): the endpoints and the export's section.
        services.AddScoped<UserPreferencesService>();
        services.AddScoped<IUserPreferences>(provider => provider.GetRequiredService<UserPreferencesService>());

        // Tenancy (S5-15, ARCHITECTURE §5.4, ADR-007): the memberships of the request's user, filled
        // by UseIdentityAccessOrganizationContext, read by every module's organisation query filter
        // through IOrganizationContext and by the resource-based role requirement (§9.1, TEN-05).
        services.AddScoped<OrganizationContext>();
        services.AddScoped<IOrganizationContext>(provider => provider.GetRequiredService<OrganizationContext>());
        services.AddScoped<IAuthorizationHandler, OrganizationRoleHandler>();

        // Organisations and members (S5-16a): the rules behind the organisation endpoints, which
        // add the members' addresses (owner decision S5-16 4a).
        services.AddScoped<IOrganizationMembers, OrganizationMemberService>();

        // Invitations by e-mail (S5-17a, SPEC R-ORG-02): 🔧 lifetime read at start; expired rows are
        // deleted by a background job (owner decision S5-17 D7 (a)), which only the web host starts.
        services.AddSingleton(InvitationSettings.From(configuration));
        services.AddScoped<IOrganizationInvitations, OrganizationInvitationService>();
        services.AddHostedService<InvitationCleanupService>();
        services.AddScoped<TestOrganizationSeeder>();

        return services;
    }

    /// <summary>
    /// Loads the signed-in user's organisation memberships for the request
    /// (<see cref="OrganizationContextMiddleware"/>; TEN-01, TEN-05). Add it after authentication
    /// and before authorization.
    /// </summary>
    public static IApplicationBuilder UseIdentityAccessOrganizationContext(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<OrganizationContextMiddleware>();
    }

    /// <summary>
    /// Validates the antiforgery token of every state-changing request
    /// (<see cref="AntiforgeryValidationMiddleware"/>; SEC-02). Add it after authorization and the
    /// rate limiter, before the endpoints run.
    /// </summary>
    public static IApplicationBuilder UseIdentityAccessAntiforgery(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<AntiforgeryValidationMiddleware>();
    }

    /// <summary>
    /// Maps the module's endpoints as minimal APIs on <paramref name="routes"/>, the host's versioned
    /// group <c>/api/v{version}</c>, as <c>MapCatalogEndpoints</c> does. Map it before the Swagger
    /// set-up (#176). Below <c>/auth</c>: the session probe and its extension (S5-29), sign-in and
    /// sign-out, registration and the password reset; below <c>/account</c>: the account, its
    /// settings (S5-25a), export and deletion; below <c>/organizations</c>: organisations, members
    /// and invitations. Sign-in, account and organisations came from the host with the users (S6-10).
    /// </summary>
    public static IEndpointRouteBuilder MapIdentityAccessEndpoints(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);

        var auth = routes.MapGroup("/auth").WithTags("Auth");
        SessionProbeEndpoint.Map(auth);
        SessionExtendEndpoint.Map(auth);
        UserPreferencesEndpoints.Map(routes);

        routes.MapAuthEndpoints()
              .MapRegistrationEndpoints()
              .MapPasswordResetEndpoints()
              .MapAccountEndpoints()
              .MapAccountExportEndpoints()
              .MapAccountDeletionEndpoints()
              .MapOrganizationEndpoints()
              .MapInvitationEndpoints();
        return routes;
    }

    /// <summary>
    /// Maps <c>GET /me</c> on <paramref name="routes"/>, the host's unversioned group <c>/api</c>
    /// (<see cref="LegacyAccountEndpoint"/>): the client still reads it while the legacy screens
    /// need it.
    /// </summary>
    public static IEndpointRouteBuilder MapIdentityAccessLegacyEndpoints(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);
        LegacyAccountEndpoint.Map(routes);
        return routes;
    }

    /// <summary>
    /// True when the host was started with a <c>roles</c> command (<c>roles list|grant|revoke …</c>)
    /// instead of as the API (SPEC R-ADM-03, S4-25).
    /// </summary>
    public static bool IsRolesCommand(IReadOnlyList<string> args) => RoleCommandLine.IsCommand(args);

    /// <summary>
    /// Runs the <c>roles</c> command in <paramref name="args"/> on <paramref name="services"/>, which
    /// must hold the Platform and IdentityAccess modules. Returns the exit code: 0 done (also when
    /// nothing had to change), 1 refused (nothing written), 2 wrong usage.
    /// </summary>
    public static Task<int> RunRolesCommandAsync(
        IServiceProvider services,
        IReadOnlyList<string> args,
        TextWriter output,
        TextWriter error,
        CancellationToken cancellationToken = default) =>
        RoleCommandLine.RunAsync(services, args, output, error, cancellationToken);

    /// <summary>Exit code of a <c>roles</c> command that failed or was refused.</summary>
    public const int RolesCommandFailure = RoleCommandLine.Failure;

    /// <summary>
    /// The Development rebuild of the users (SEC-06): deletes every organisation and account and creates the sample
    /// user <see cref="SampleUserEmail"/> with <paramref name="sampleUserPassword"/>; without a
    /// password no user is created. Returns true when it created the user. The host calls it only in
    /// <c>Development</c> with <c>RebuildDatabase</c> set; nothing here checks the environment.
    /// </summary>
    public static async Task<bool> RebuildSampleUsersAsync(
        IServiceProvider services, string? sampleUserPassword, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);
        await using var scope = services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<SeedUsers>().RebuildAsync(sampleUserPassword, cancellationToken);
    }

    /// <summary>Address of the sample user of the Development rebuild.</summary>
    public const string SampleUserEmail = SeedUsers.SampleUserEmail;

    /// <summary>
    /// The E2E seeder (TESTING §6.3; S5-14, S5-16b, owner decision S5-16 7a): creates the users of
    /// <paramref name="users"/> that do not exist yet, each with <paramref name="password"/>, and the
    /// organisation <paramref name="organizationId"/> named <paramref name="organizationName"/> with
    /// <paramref name="members"/> (addresses of <paramref name="users"/>, exactly one Owner). An
    /// existing user stays as it is; an existing organisation of the same Owner stays, one of another
    /// Owner (users created again after a rebuild) is replaced. The host calls it only in
    /// <c>Development</c> and <c>E2E</c> with the switch <c>TestUsers:Password</c>, after the
    /// migrations; nothing here checks the environment.
    /// </summary>
    public static async Task SeedTestUsersAsync(
        IServiceProvider services,
        IReadOnlyList<(string Email, bool EmailConfirmed)> users,
        string password,
        Guid organizationId,
        string organizationName,
        IReadOnlyList<(string Email, OrganizationRole Role)> members,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(users);
        ArgumentNullException.ThrowIfNull(members);
        ArgumentException.ThrowIfNullOrEmpty(password);
        if (members.Count(m => m.Role == OrganizationRole.Owner) != 1)
        {
            throw new ArgumentException("A test organisation has exactly one Owner.", nameof(members));
        }

        await using var scope = services.CreateAsyncScope();
        var ids = await scope.ServiceProvider.GetRequiredService<SeedUsers>().EnsureAsync(users, password);
        var memberIds = members.Select(m => (UserId: ids[m.Email], m.Role)).ToList();
        await scope.ServiceProvider.GetRequiredService<TestOrganizationSeeder>()
                   .SeedAsync(organizationId, organizationName, memberIds, cancellationToken);
    }
}
