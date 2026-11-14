// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// No imports: app.routes.ts reads LOGIN_PATH while app.config.ts is still loading.

/** Path of the sign-in page S01 in the `login` outlet (UX copy §3: `/anmelden`). */
export const LOGIN_PATH = 'anmelden';

/** Why S01 opens: `save` shows `auth.login.reasonSave` (S11 „Liste speichern“, R-AUTH-03). */
export type LoginReason = 'save';

/** Router state that carries the {@link LoginReason} to S01; never in the URL. */
export interface LoginNavigationState {
  loginReason?: LoginReason;
}
