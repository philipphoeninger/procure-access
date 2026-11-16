// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { CriteriaSelection } from '../../models/configurator-draft';
import { CriteriaResponse } from '../../models/criteria';
import { CriterionDecision, CriterionDecisions } from '../../models/criteria-list';
import { FilterOptionsLayers } from '../../models/filter-options';
import { SavedList } from '../../models/saved-lists';
import { CatalogApiService } from '../../services/api/catalog-api.service';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';

/**
 * What the page of a saved list shows: the list with the criteria and filter values of its own
 * release and its decisions by criterion key, or the S80 page in place.
 */
export type SavedListView =
  | {
      kind: 'list';
      list: SavedList;
      selection: CriteriaSelection;
      criteria: CriteriaResponse;
      options: FilterOptionsLayers;
      decisions: CriterionDecisions;
    }
  | { kind: 'notFound' }
  | { kind: 'server' };

/** The view the resolver loaded, handed to the page; one per route (route-level provider). */
@Injectable()
export class SavedListLoad {
  view: SavedListView = { kind: 'notFound' };
}

/** The selection of a saved list as the catalog endpoints read it. */
export function selectionOf(list: SavedList): CriteriaSelection {
  const { useCaseTemplateId, appTypeIds, productPartIds, testTypeIds } = list.selection;
  return {
    ...(useCaseTemplateId === null ? {} : { useCase: useCaseTemplateId }),
    appTypeIds: [...appTypeIds],
    productPartIds: [...productPartIds],
    testTypeIds: [...testTypeIds],
  };
}

/**
 * The stored decisions by criterion key, as `pa-criterion-card` reads them: the server names each
 * criterion by its revision in the pinned release, the criteria answer of that release gives its key.
 */
export function decisionsOf(list: SavedList, criteria: CriteriaResponse): CriterionDecisions {
  const byRevision = new Map(list.criteria.map((criterion) => [criterion.criterionRevisionId, criterion]));
  const decisions: Record<string, CriterionDecision> = {};
  for (const criterion of criteria.criteria) {
    const saved = byRevision.get(criterion.revisionId);
    if (saved !== undefined) {
      decisions[criterion.key] = {
        included: saved.isIncluded,
        overrideLevel: saved.overrideLevel,
        overrideWeight: saved.overrideWeight,
        note: saved.note ?? '',
      };
    }
  }
  return decisions;
}

/**
 * Reads the list (SPEC §8.10) and then the criteria and filter values of its release (§8.1, §8.2,
 * `release=<version>`, also when a newer release is active, CAT-06). A 404 or a malformed id (400)
 * of the list is „not found“ — another organisation's list does not exist for the user (TEN-01) —,
 * every other failure a server failure.
 */
export async function loadSavedListView(
  configurator: ConfiguratorApiService,
  catalog: CatalogApiService,
  id: string,
): Promise<SavedListView> {
  let list: SavedList;
  try {
    list = await configurator.get(id);
  } catch (error: unknown) {
    const status = error instanceof HttpErrorResponse ? error.status : 0;
    return status === 404 || status === 400 ? { kind: 'notFound' } : { kind: 'server' };
  }

  const selection = selectionOf(list);
  try {
    const [criteria, options] = await Promise.all([
      catalog.getCriteria(selection, list.releaseVersion),
      catalog.getFilterOptions(selection, list.releaseVersion),
    ]);
    return { kind: 'list', list, selection, criteria, options: options.layers, decisions: decisionsOf(list, criteria) };
  } catch {
    // A stored list names a stored release (CAT-05); a failure here is the server's.
    return { kind: 'server' };
  }
}
