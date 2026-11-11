// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { ProcureAccessStore } from '@app/core/state/app.store';

/** Where the criteria lead while no catalog release is active: the legacy filter (O-S4-3). */
export const LEGACY_FILTER_PATH = '/filters';

/** S10, the page the „Ändern“ of S11 and a selection that is gone lead back to. */
export const CONFIGURATOR_PATH = '/kriterien';

/** S11, opened after the server accepted the selection (R-CON-10). */
export const CRITERIA_LIST_PATH = '/kriterien/ergebnis';

/**
 * The one switch between S10 and the legacy filter (O-S4-3, owner decision O-S4-17 (a)): start
 * page and header always link S10, and this guard of `/kriterien` waits for the filter options.
 * Without an active release (404) it opens the legacy filter instead, before S10 renders, so the
 * user gets one route change and one announcement. Any other failure opens S10, where the error
 * interceptor shows the system state (offline notice or server error page).
 */
export const configuratorReleaseGuard: CanActivateFn = async () => {
  const store = inject(ProcureAccessStore);
  const router = inject(Router);
  const status = await store.filterOptionsReady();
  return status === 'noRelease' ? router.parseUrl(LEGACY_FILTER_PATH) : true;
};

/**
 * S11 shows only a complete selection from S10 (R-CON-10). The draft lives in memory only
 * (R-CON-09), so after a reload it is gone and the page leads back to S10.
 */
export const completeSelectionGuard: CanActivateFn = () => {
  const store = inject(ProcureAccessStore);
  return store.configuratorComplete() ? true : inject(Router).parseUrl(CONFIGURATOR_PATH);
};
