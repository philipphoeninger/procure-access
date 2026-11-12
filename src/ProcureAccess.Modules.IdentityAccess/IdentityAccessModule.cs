// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProcureAccess.Modules.IdentityAccess.DataProtection;
using ProcureAccess.Modules.IdentityAccess.Persistence;
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

        return services;
    }
}
