// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject } from '@angular/core';
import { ResolveFn, Routes } from '@angular/router';

import { PAGE_TITLE_KEY } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';

import { CatalogApiService } from '../../services/api/catalog-api.service';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import { ConfiguratorSettings } from '../../state/configurator-settings';
import { loadSharedListView, SharedListLoad } from './shared-list-load';
import { SharedListPage } from './shared-list-page';

/**
 * Loads the shared list and the configurator settings its cards need before S16 shows, so the
 * route change titles, focuses and announces „Geteilte Kriterienliste: {{name}}“ with the name
 * already known (A11Y-03), or „Link nicht mehr gültig“. The loading bar shows meanwhile.
 */
export const sharedListResolver: ResolveFn<string> = async (route) => {
  const load = inject(SharedListLoad);
  const store = inject(ProcureAccessStore);
  const settings = inject(ConfiguratorSettings);
  store.incrementLoadingCount();
  try {
    const [view, settingsReady] = await Promise.all([
      loadSharedListView(inject(ConfiguratorApiService), inject(CatalogApiService), route.paramMap.get('token') ?? ''),
      settings.ready(),
    ]);
    load.view = view.kind === 'list' && !settingsReady ? { kind: 'server' } : view;
  } finally {
    store.decrementLoadingCount();
  }
  switch (load.view.kind) {
    case 'list':
      return 'pageTitle.criteria.shared';
    case 'invalid':
      return 'pageTitle.criteria.sharedInvalid';
    default:
      return 'pageTitle.server';
  }
};

/** S16 at `/geteilt/:token`, lazy; no sign-in (R-SAV-05, TEN-09). */
export const SHARED_LIST_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    component: SharedListPage,
    providers: [SharedListLoad],
    resolve: { [PAGE_TITLE_KEY]: sharedListResolver },
  },
];
