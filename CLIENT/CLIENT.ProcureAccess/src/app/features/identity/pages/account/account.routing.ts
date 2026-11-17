// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject } from '@angular/core';
import { ResolveFn, Routes } from '@angular/router';

import { PAGE_TITLE_KEY } from '@app/core/a11y/page-title.strategy';
import { authGuard } from '@app/core/guards/auth.guard';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { AccountApiService } from '../../services/api/account-api.service';
import { AccountLoad } from './account-load';
import { AccountPage } from './account-page';

/**
 * Reads the account before S61 shows (SPEC §8.13): the address the deletion dialog compares and
 * whether it is confirmed. A failed read shows the S80 server text in place, as S60 does; the
 * loading bar shows meanwhile.
 */
export const accountResolver: ResolveFn<string> = async () => {
  const store = inject(ProcureAccessStore);
  const api = inject(AccountApiService);
  const load = inject(AccountLoad);
  store.incrementLoadingCount();
  try {
    load.account = await api.get();
  } catch {
    load.account = null;
  } finally {
    store.decrementLoadingCount();
  }
  return load.account ? 'pageTitle.account' : 'pageTitle.server';
};

/** S61 „Konto“ at `/konto`, lazy; signed in only (UX copy §4, SPEC R-AUTH-09, R-AUTH-10). */
export const ACCOUNT_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    component: AccountPage,
    canActivate: [authGuard],
    providers: [AccountLoad],
    resolve: { [PAGE_TITLE_KEY]: accountResolver },
  },
];
