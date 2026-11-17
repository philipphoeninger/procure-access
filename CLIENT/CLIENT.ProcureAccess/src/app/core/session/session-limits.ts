// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Injectable, signal } from '@angular/core';

/** Response header with the idle limit of the session (ISO 8601 UTC). */
export const SESSION_EXPIRES_AT_HEADER = 'X-Session-Expires-At';
/** Response header with the absolute limit of the session (ISO 8601 UTC). */
export const SESSION_ABSOLUTE_EXPIRES_AT_HEADER = 'X-Session-Absolute-Expires-At';

/**
 * The two limits of the session as the server last told them (ARCHITECTURE §8.3 job 2, §9.1; SPEC
 * R-AUTH-05; SEC-07): the session probe's answer and, after it, the expiry headers of every answer
 * to a signed-in request, because each of those requests renews the idle limit. In memory only.
 *
 * A service of its own, without imports, because the session interceptor writes it and must not
 * inject the root store, whose first request runs while the store is being built. Read the limits
 * through the store (`sessionExpiresAt`, `sessionAbsoluteExpiresAt` of `withIdentity()`), which
 * shows them only while somebody is signed in.
 */
@Injectable({ providedIn: 'root' })
export class SessionLimits {
  private readonly expiry = signal<string | null>(null);
  private readonly absoluteExpiry = signal<string | null>(null);

  /** Idle limit (ISO 8601): the session ends then unless a request renews it. */
  readonly expiresAt = this.expiry.asReadonly();
  /** Absolute limit (ISO 8601): the session ends then whatever happens. */
  readonly absoluteExpiresAt = this.absoluteExpiry.asReadonly();

  set(expiresAt: string | null, absoluteExpiresAt: string | null): void {
    this.expiry.set(expiresAt);
    this.absoluteExpiry.set(absoluteExpiresAt);
  }

  clear(): void {
    this.set(null, null);
  }
}
