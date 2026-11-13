// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

/**
 * The signed-in user as `GET /api/v1/auth/session` reports it (SPEC §8.8; the contract record
 * `SessionUser` of the IdentityAccess module, #320): the platform roles and the permissions those
 * roles grant, e.g. `moderation:review`. The client shows or hides controls by `permissions`; the
 * server stays the authority through its policies.
 */
export interface SessionUser {
  id: string;
  email: string;
  roles: string[];
  permissions: string[];
}

/**
 * Answer of the session probe: `user` is null without a session. The expiry times (ISO 8601) are
 * left out for a sign-in without session limits (the test scheme of the integration host).
 */
export interface SessionResponse {
  user: SessionUser | null;
  expiresAt?: string;
  absoluteExpiresAt?: string;
}
