// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { ProcureAccessStore } from '@app/core/state/app.store';

/** Route data key: the permission the route needs, e.g. `moderation:review`. */
export const PERMISSION_KEY = 'permission';

/**
 * Lets only a user whose session carries the route's permission through (legacy moderation page
 * until step 5); the permissions come from the probe, so the guard waits for it on a deep link.
 * Everyone else sees the „Kein Zugriff“ page.
 */
export const permissionGuard: CanActivateFn = async (route) => {
  const store = inject(ProcureAccessStore);
  const router = inject(Router);
  const permission = route.data[PERMISSION_KEY] as string | undefined;
  await store.sessionReady();
  if (permission !== undefined && store.hasPermission(permission)) {
    return true;
  }
  return router.createUrlTree(['/unauthorized']);
};
