// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { InjectionToken } from '@angular/core';

import { environment } from '../../../../environments/environment';

/** Length limits of a new password (SPEC R-AUTH-01): no composition rules. */
export interface PasswordRule {
  minLength: number;
  maxLength: number;
}

/** Longest password (SPEC R-AUTH-01: „maximum 128“), the same fixed value as the server's. */
export const PASSWORD_MAX_LENGTH = 128;

/**
 * The password rule of the registration. The minimum is a 🔧 value (SPEC SD23) read from the
 * environment file of the build, like the weight range of S11, and kept in step with the server's
 * `Account:Password:MinLength`; the server checks it again and decides.
 */
export const PASSWORD_RULE = new InjectionToken<PasswordRule>('PASSWORD_RULE', {
  providedIn: 'root',
  factory: () => ({ minLength: environment.passwordMinLength, maxLength: PASSWORD_MAX_LENGTH }),
});
