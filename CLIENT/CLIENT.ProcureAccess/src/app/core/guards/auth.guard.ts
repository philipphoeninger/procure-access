// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject } from '@angular/core';
import { CanActivateFn, RedirectCommand, Router } from '@angular/router';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { LOGIN_PATH, LoginNavigationState } from '@features/identity/models/login-navigation';

/**
 * Lets only a signed-in user through; the session is the probe's answer, so the guard waits for it
 * on a deep link. Everyone else is sent to the sign-in page S01 in the `login` outlet over the start
 * page, with the page they opened as `returnUrl` in the router state (memory only, never the URL or
 * browser storage, R-SET-02). S01 goes there after the sign-in (S6-03, TD-65). S01 cannot open over
 * the guarded page itself: the guard would run again for it and the page would load signed out.
 */
export const authGuard: CanActivateFn = async (_route, state) => {
  const store = inject(ProcureAccessStore);
  const router = inject(Router);
  await store.sessionReady();
  if (store.signedIn()) {
    return true;
  }
  const loginState: LoginNavigationState = { returnUrl: state.url };
  return new RedirectCommand(router.createUrlTree(['', { outlets: { login: [LOGIN_PATH] } }]), { state: loginState });
};
