// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// No imports: app.routes.ts reads these paths while app.config.ts is still loading.

/** Path of the sign-in page S01 in the `login` outlet (UX copy §3: `/anmelden`). */
export const LOGIN_PATH = 'anmelden';

/** Path of S02 „Konto erstellen“ (UX copy §3: `/registrieren`). */
export const REGISTER_PATH = 'registrieren';

/** Path of the confirmation page below S02, the target of the link in the e-mail (UX copy §3, U118). */
export const CONFIRM_EMAIL_PATH = `${REGISTER_PATH}/bestaetigen`;

/** Path of S03 „Passwort zurücksetzen“, the request step (UX copy §3: `/passwort`). */
export const RESET_PASSWORD_PATH = 'passwort';

/** Path of the new-password step of S03, the target of the link in the e-mail (UX copy §3, U123). */
export const NEW_PASSWORD_PATH = `${RESET_PASSWORD_PATH}/neu`;

/** Why S01 opens: `save` shows `auth.login.reasonSave` (S11 „Liste speichern“, R-AUTH-03). */
export type LoginReason = 'save';

/** Router state that carries the {@link LoginReason} and the return target to S01; never in the URL. */
export interface LoginNavigationState {
  loginReason?: LoginReason;
  /**
   * The guarded page a signed-out user opened (`authGuard`, S6-03); S01 goes there after the
   * sign-in, after `safeReturnUrl` has accepted it.
   */
  returnUrl?: string;
}
