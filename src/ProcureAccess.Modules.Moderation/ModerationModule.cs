// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.Extensions.DependencyInjection;

namespace ProcureAccess.Modules.Moderation;

/// <summary>
/// Registration of the Moderation module (ARCHITECTURE §4.3): content reports, counter-statements
/// and the moderation audit log.
/// It registers no services yet. Its code arrives with migration step 7.
/// </summary>
public static class ModerationModule
{
    /// <summary>Registers the services of the Moderation module. Called once from <c>Program.cs</c>.</summary>
    public static IServiceCollection AddModerationModule(this IServiceCollection services) => services;
}
