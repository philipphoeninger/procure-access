// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProcureAccess.Modules.IdentityAccess.DataProtection;
using ProcureAccess.Modules.IdentityAccess.Persistence;
using ProcureAccess.Modules.IdentityAccess.Session;
using ProcureAccess.Modules.Platform.Contracts.Persistence;

namespace ProcureAccess.Modules.IdentityAccess;

/// <summary>
/// Registration of the IdentityAccess module (ARCHITECTURE §4.3): users, memberships, contributor
/// profiles, roles and the Data Protection key store.
/// Since S3-24 it holds the key store: the Data Protection key ring lives in
/// <c>identity."DataProtectionKeys"</c> instead of the container's file system, so Identity tokens
/// today, and the session cookie and antiforgery tokens from step 3, stay valid when the container
/// is replaced (§9.1, C60). Since S4-26 the keys are encrypted at rest with a certificate from the
/// environment (<see cref="KeyRingCertificates"/>).
/// Users and the JWT are still legacy in API, MODELS and SERVICES until migration step 3.
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

        services.AddNpgsql<IdentityAccessDbContext>(connectionString, options =>
        {
            options.EnableRetryOnFailure().CommandTimeout(60);
        });
        services.AddScoped<IModuleDatabaseMigrator, IdentityAccessDatabaseMigrator>();

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

        return services;
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
    /// Maps the module's endpoints as minimal APIs below <c>/auth</c> of <paramref name="routes"/>,
    /// the host's versioned group <c>/api/v{version}</c>, as <c>MapCatalogEndpoints</c> does. Map it
    /// before the Swagger set-up (#176). Today only the session probe; sign-in and sign-out live in
    /// the host until the users move into this module (M3.1).
    /// </summary>
    public static IEndpointRouteBuilder MapIdentityAccessEndpoints(this IEndpointRouteBuilder routes)
    {
        ArgumentNullException.ThrowIfNull(routes);
        var auth = routes.MapGroup("/auth").WithTags("Auth");
        SessionProbeEndpoint.Map(auth);
        return routes;
    }

    /// <summary>
    /// Issues the antiforgery token cookie for the user of <paramref name="context"/>
    /// (<c>HttpContext.User</c>), as the session probe does (SEC-09). For the host's sign-in and
    /// sign-out endpoints, which rotate the token because it is bound to the user; it moves into the
    /// module with them (M3.1). Set <c>HttpContext.User</c> to the new principal first.
    /// </summary>
    public static void IssueAntiforgeryToken(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        AntiforgeryTokenCookie.Issue(context);
    }
}
