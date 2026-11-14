// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { InjectionToken } from '@angular/core';

import { environment } from '../../../../environments/environment';

/** The sign-in lockout (SPEC R-AUTH-03): after so many failed sign-ins, locked for so many minutes. */
export interface LockoutRule {
  maxFailedAttempts: number;
  minutes: number;
}

/**
 * The 🔧 lockout values (SPEC SD23) for the fixed note `auth.login.error.lockout` under a refused
 * sign-in (owner decision S5-13 1 (a)), read from the environment file of the build and kept in
 * step with the server's `Account:Lockout:*`, which decides. The server never says whether an
 * account is locked, so the note names the rule, not the state.
 */
export const LOCKOUT_RULE = new InjectionToken<LockoutRule>('LOCKOUT_RULE', {
  providedIn: 'root',
  factory: () => environment.lockout,
});
