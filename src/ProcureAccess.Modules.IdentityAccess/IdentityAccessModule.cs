// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;

namespace ProcureAccess.Modules.IdentityAccess;

/// <summary>
/// Registration of the IdentityAccess module (ARCHITECTURE §4.3): users, memberships, contributor
/// profiles, roles and the Data Protection key store.
/// It registers no services yet. Its code arrives with migration step 3; until then users and the
/// JWT are legacy in API, MODELS and SERVICES.
/// </summary>
public static class IdentityAccessModule
{
    /// <summary>Registers the services of the IdentityAccess module. Called once from <c>Program.cs</c>.</summary>
    public static IServiceCollection AddIdentityAccessModule(this IServiceCollection services) => services;
}
