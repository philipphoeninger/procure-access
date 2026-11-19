// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject, Injectable } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { ProcureAccessStore } from '@app/core/state/app.store';

import { savedListPath } from '../../models/saved-lists';
import { CatalogApiService } from '../../services/api/catalog-api.service';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import { selectionOf } from './saved-list-load';

/**
 * The saved list whose selection S10 changes at `/kriterienlisten/:id/auswahl` (owner decision
 * S5-21b 2a): set by {@link savedListSelectionGuard}, read by S10, one per route (route-level
 * provider). Null on S10 of the draft.
 */
@Injectable()
export class SavedListSelection {
  /** With the tag the list was read at, for the `If-Match` of the selection change (API-03, S6-20b). */
  list: { id: string; release: string; etag: string } | null = null;
}

/**
 * Opens S10 for a saved list only when it may change the selection there: the list exists for the
 * user (TEN-01), the user is Editor or Owner of its organisation (R-SAV-03) and its release is the
 * active one (owner decision S5-21 5a: a list of an older release is copied instead). Then the
 * draft of S10/S11 is put aside and S10 filled with the list's selection
 * (`startSavedListSelection`); S10 brings the draft back when it is left. Anything else leads to the
 * page of the list, which shows what it can (also „Seite nicht gefunden“).
 */
export const savedListSelectionGuard: CanActivateFn = async (route) => {
  const router = inject(Router);
  const store = inject(ProcureAccessStore);
  const context = inject(SavedListSelection);
  const configurator = inject(ConfiguratorApiService);
  const catalog = inject(CatalogApiService);
  const id = route.parent?.paramMap.get('id') ?? '';
  const back = router.parseUrl(savedListPath(id));

  store.incrementLoadingCount();
  try {
    const list = await configurator.get(id);
    const role = store.organizations().find((organization) => organization.id === list.organizationId)?.role;
    if (role !== 'owner' && role !== 'editor') return back;
    const active = await catalog.getFilterOptions({ appTypeIds: [], productPartIds: [] });
    if (active.release !== list.releaseVersion) return back;
    const status = await store.startSavedListSelection(selectionOf(list), list.releaseVersion);
    if (status !== 'loaded') {
      store.endSavedListSelection();
      return back;
    }
    context.list = { id: list.id, release: list.releaseVersion, etag: list.etag };
    return true;
  } catch {
    return back;
  } finally {
    store.decrementLoadingCount();
  }
};
