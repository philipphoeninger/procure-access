// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

using System.Text.Json.Serialization;

namespace ProcureAccess.Modules.IdentityAccess.Contracts.Session;

/// <summary>
/// Answer of the session probe <c>GET /api/v1/auth/session</c> (SPEC §8.8, ARCHITECTURE §8.3,
/// C133): <see cref="Anonymous"/> for a request without a session, otherwise the signed-in
/// <see cref="User"/> with the two expiry times of the session. The client's session model
/// (<c>withIdentity()</c>) mirrors this record (#320).
/// </summary>
/// <param name="User">The signed-in user, or null.</param>
/// <param name="ExpiresAt">
/// The idle limit: when the session ends without a further request. Left out for a sign-in
/// without session limits (the test scheme of the integration host).
/// </param>
/// <param name="AbsoluteExpiresAt">
/// The absolute limit: sign-in time plus <c>Session:AbsoluteTimeout</c>. Left out like
/// <paramref name="ExpiresAt"/>.
/// </param>
public sealed record SessionResponse(
    SessionUser? User,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DateTimeOffset? ExpiresAt,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] DateTimeOffset? AbsoluteExpiresAt)
{
    /// <summary>The answer for a request without a session.</summary>
    public static readonly SessionResponse Anonymous = new(null, null, null);
}

/// <summary>
/// The signed-in user as the probe reports it, from the claims of the session principal: id,
/// e-mail address, the names of the platform roles (ARCHITECTURE §8.1) and the permissions those
/// roles grant (role claims of type <c>permission</c>, e.g. <c>moderation:review</c>). The client
/// shows or hides moderation controls by <paramref name="Permissions"/>; the server stays the
/// authority through its policies (owner decision S5-08 3 (a)).
/// </summary>
public sealed record SessionUser(
    string Id,
    string Email,
    IReadOnlyList<string> Roles,
    IReadOnlyList<string> Permissions);
