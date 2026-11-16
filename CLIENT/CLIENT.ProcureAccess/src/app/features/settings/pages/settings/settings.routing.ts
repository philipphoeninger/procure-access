// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject } from '@angular/core';
import { ResolveFn, Routes } from '@angular/router';

import { PAGE_TITLE_KEY } from '@app/core/a11y/page-title.strategy';
import { authGuard } from '@app/core/guards/auth.guard';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { SettingsLoad } from './settings-load';
import { SettingsPage } from './settings-page';

/**
 * Reads the signed-in user's settings before S60 shows (SPEC §8.12), so the page shows what is
 * stored — also when the read after the sign-in failed, so that a change never overwrites the other
 * settings with the defaults. A failed read shows the S80 server text in place, as S14 and S21 do;
 * the loading bar shows meanwhile.
 */
export const settingsResolver: ResolveFn<string> = async () => {
  const store = inject(ProcureAccessStore);
  const load = inject(SettingsLoad);
  store.incrementLoadingCount();
  try {
    await store.readPreferences();
    load.failed = false;
  } catch {
    load.failed = true;
  } finally {
    store.decrementLoadingCount();
  }
  return load.failed ? 'pageTitle.server' : 'pageTitle.settings';
};

/** S60 „Einstellungen“ at `/einstellungen`, lazy; signed in only (UX copy §4, SPEC R-SET-01). */
export const SETTINGS_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    component: SettingsPage,
    canActivate: [authGuard],
    providers: [SettingsLoad],
    resolve: { [PAGE_TITLE_KEY]: settingsResolver },
  },
];
