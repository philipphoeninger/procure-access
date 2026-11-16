// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Accounts;

/// <summary>
/// A user's settings for the personal data export (SPEC §8.4 <c>account.preferences</c>; S5-25a,
/// owner decision S5-25 4): always the effective values, the defaults for a user who never saved any.
/// </summary>
public interface IUserPreferences
{
    /// <summary>The settings of <paramref name="userId"/>, or the defaults.</summary>
    Task<UserPreferencesValues> GetAsync(string userId, CancellationToken cancellationToken);
}

/// <summary>
/// The four settings as the API spells them (<c>GET /api/v1/account/preferences</c>): language
/// <c>de</c>; appearance <c>system</c> / <c>light</c> / <c>dark</c>; motion <c>system</c> /
/// <c>reduced</c>; contrast <c>system</c> / <c>high</c>.
/// </summary>
public sealed record UserPreferencesValues(string Language, string Appearance, string Motion, string Contrast);
