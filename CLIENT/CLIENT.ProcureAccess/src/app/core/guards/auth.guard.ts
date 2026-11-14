// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { ProcureAccessStore } from '@app/core/state/app.store';
import { LOGIN_PATH } from '@features/identity/models/login-navigation';

/**
 * Lets only a signed-in user through (legacy account pages until S5-28); the session is the
 * probe's answer, so the guard waits for it on a deep link. Everyone else is sent to the sign-in
 * page S01 in the `login` outlet over the start page.
 */
export const authGuard: CanActivateFn = async () => {
  const store = inject(ProcureAccessStore);
  const router = inject(Router);
  await store.sessionReady();
  if (store.signedIn()) {
    return true;
  }
  return router.createUrlTree(['', { outlets: { login: [LOGIN_PATH] } }]);
};
