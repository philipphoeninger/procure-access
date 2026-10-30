// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;

namespace ProcureAccess.Modules.Platform;

/// <summary>
/// Registration of the Platform module (ARCHITECTURE §4.3): share links, file storage, e-mail
/// sending, Problem Details, rate limiting, i18n and caching; every module may use it through its
/// contracts.
/// It registers no services yet. Its code arrives with step 1: the platform code built ahead
/// (logging, health, usage events, TimeProvider) moves in with S2-07, Problem Details with S2-10.
/// </summary>
public static class PlatformModule
{
    /// <summary>Registers the services of the Platform module. Called once from <c>Program.cs</c>.</summary>
    public static IServiceCollection AddPlatformModule(this IServiceCollection services) => services;
}
