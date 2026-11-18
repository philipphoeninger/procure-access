// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;
using ProcureAccess.Modules.IdentityAccess.Domain;
using ProcureAccess.Modules.IdentityAccess.Persistence;

namespace ProcureAccess.Modules.IdentityAccess.Auth;

/// <summary>
/// ASP.NET Core Identity on the module's own context (S6-10; formerly the host's
/// <c>IdentityExtensions</c> on the legacy context): the user and role stores, the sign-in manager,
/// the 🔧 password and lockout rules and the e-mail confirmation gate.
/// </summary>
internal static class IdentityRegistration
{
    public static IServiceCollection AddIdentityAccessIdentity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityCore<User>(options =>
                {
                    // An unconfirmed account signs in; the confirmation gates single actions (SEC-10).
                    options.SignIn.RequireConfirmedEmail = false;
                })
                .AddUserManager<ClockedUserManager>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<IdentityAccessDbContext>()
                .AddSignInManager()
                .AddDefaultTokenProviders()
                .AddPasswordValidator<MaximumLengthPasswordValidator>();

        services.ConfigureIdentityOptions(configuration);

        // The e-mail confirmation gate (S5-12a, SEC-10) with its 403 answer email-unconfirmed.
        services.AddAuthorizationBuilder()
                .AddPolicy(IdentityAccessPolicies.ConfirmedEmail, policy => policy
                    .RequireAuthenticatedUser()
                    .AddRequirements(new ConfirmedEmailRequirement()));
        services.AddScoped<IAuthorizationHandler, ConfirmedEmailHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, ConfirmedEmailResultHandler>();

        return services;
    }

    /// <summary>
    /// Unique e-mail addresses, the password rule of <see cref="PasswordPolicy"/> (S5-12a): a
    /// 🔧 minimum length, no composition rules, and the 🔧 lockout of <see cref="LockoutPolicy"/>
    /// (S5-13a). The user name is the e-mail address, so it may hold every character an address may
    /// (the default allows only letters, digits and <c>-._@+</c>). Both settings are read here, so a
    /// missing or impossible value stops the start.
    /// </summary>
    public static IServiceCollection ConfigureIdentityOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var minLength = PasswordPolicy.MinLengthFrom(configuration);
        var lockout = LockoutPolicy.From(configuration);
        services.Configure<IdentityOptions>(options =>
        {
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = lockout.MaxFailedAttempts;
            options.Lockout.DefaultLockoutTimeSpan = lockout.Duration;

            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters = "";

            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = minLength;
            options.Password.RequiredUniqueChars = 1;
        });

        return services;
    }
}
