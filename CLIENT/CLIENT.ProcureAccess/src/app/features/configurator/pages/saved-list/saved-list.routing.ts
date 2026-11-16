// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject } from '@angular/core';
import { ResolveFn, Routes } from '@angular/router';

import { PAGE_TITLE_KEY } from '@app/core/a11y/page-title.strategy';
import { authGuard } from '@app/core/guards/auth.guard';
import { ProcureAccessStore } from '@app/core/state/app.store';

import { CatalogApiService } from '../../services/api/catalog-api.service';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import { ConfiguratorSettings } from '../../state/configurator-settings';
import { SavedListPage } from './saved-list-page';
import { SavedListEditor } from './saved-list-editor';
import { loadSavedListView, SavedListLoad } from './saved-list-load';
import { SavedListSelection, savedListSelectionGuard } from './saved-list-selection';

/**
 * Loads the saved list and the configurator settings its cards need (the weight range, SPEC §8.11)
 * before its page shows (SPEC §8.10), so the route change titles, focuses and
 * announces „{{name}} – Gespeicherte Kriterienlisten“ with the name already known (A11Y-03). A list
 * that does not exist for the user shows „Seite nicht gefunden“ in place, any other failure the S80
 * server text, as S21 does; so do settings that fail. The loading bar shows meanwhile.
 */
export const savedListResolver: ResolveFn<string> = async (route) => {
  const load = inject(SavedListLoad);
  const store = inject(ProcureAccessStore);
  const settings = inject(ConfiguratorSettings);
  store.incrementLoadingCount();
  try {
    const [view, settingsReady] = await Promise.all([
      loadSavedListView(inject(ConfiguratorApiService), inject(CatalogApiService), route.paramMap.get('id') ?? ''),
      settings.ready(),
    ]);
    load.view = view.kind === 'list' && !settingsReady ? { kind: 'server' } : view;
  } finally {
    store.decrementLoadingCount();
  }
  switch (load.view.kind) {
    case 'list':
      return 'pageTitle.criteria.list.saved';
    case 'notFound':
      return 'pageTitle.notFound';
    default:
      return 'pageTitle.server';
  }
};

/** A saved list at `/kriterienlisten/:id`, lazy; signed in only (R-SAV-02, R-SAV-03; editable for Editor and Owner since S5-21b). */
export const SAVED_LIST_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    component: SavedListPage,
    canActivate: [authGuard],
    providers: [SavedListLoad, SavedListEditor],
    resolve: { [PAGE_TITLE_KEY]: savedListResolver },
  },
  // „Auswahl ändern“ of the list (R-SAV-03; owner decision S5-21b 2a): S10 on the list's selection,
  // apart from the draft; the page fills the step number of its title, as on `/kriterien`.
  {
    path: 'auswahl',
    loadComponent: () => import('../criteria-select/criteria-select').then((m) => m.CriteriaSelect),
    canActivate: [authGuard, savedListSelectionGuard],
    providers: [SavedListSelection],
    data: { [PAGE_TITLE_KEY]: 'pageTitle.criteria.step' },
  },
];
