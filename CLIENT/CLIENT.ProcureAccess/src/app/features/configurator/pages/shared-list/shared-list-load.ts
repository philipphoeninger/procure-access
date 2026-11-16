// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { CriteriaSelection } from '../../models/configurator-draft';
import { CriteriaResponse } from '../../models/criteria';
import { CriterionDecisions } from '../../models/criteria-list';
import { FilterOptionsLayers } from '../../models/filter-options';
import { SharedList } from '../../models/share-links';
import { CatalogApiService } from '../../services/api/catalog-api.service';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import { decisionsOf, selectionOf } from '../saved-list/saved-list-load';

/**
 * What S16 shows: the shared list with the criteria and filter values of its own release and its
 * decisions by criterion key; „Link nicht mehr gültig“ for a link that does not work (any 404,
 * R-SAV-05); or the S80 server page.
 */
export type SharedListView =
  | {
      kind: 'list';
      list: SharedList;
      selection: CriteriaSelection;
      criteria: CriteriaResponse;
      options: FilterOptionsLayers;
      decisions: CriterionDecisions;
    }
  | { kind: 'invalid' }
  | { kind: 'server' };

/** The view the resolver loaded, handed to the page; one per route (route-level provider). */
@Injectable()
export class SharedListLoad {
  view: SharedListView = { kind: 'invalid' };
}

/**
 * Reads the list behind the token (SPEC §8.5) and then the criteria and filter values of its
 * release (§8.1, §8.2, `release=<version>`), as for a saved list. A 404 is a link that is
 * malformed, unknown, revoked, expired or of a deleted list — one answer for all (TEN-08) —, every
 * other failure a server failure.
 */
export async function loadSharedListView(
  configurator: ConfiguratorApiService,
  catalog: CatalogApiService,
  token: string,
): Promise<SharedListView> {
  let list: SharedList;
  try {
    list = await configurator.shared(token);
  } catch (error: unknown) {
    return error instanceof HttpErrorResponse && error.status === 404 ? { kind: 'invalid' } : { kind: 'server' };
  }

  const selection = selectionOf(list);
  try {
    const [criteria, options] = await Promise.all([
      catalog.getCriteria(selection, list.releaseVersion),
      catalog.getFilterOptions(selection, list.releaseVersion),
    ]);
    return { kind: 'list', list, selection, criteria, options: options.layers, decisions: decisionsOf(list, criteria) };
  } catch {
    // A shared list names a stored release (CAT-05); a failure here is the server's.
    return { kind: 'server' };
  }
}
