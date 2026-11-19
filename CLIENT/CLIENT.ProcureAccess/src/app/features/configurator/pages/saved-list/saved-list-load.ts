// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { CriteriaSelection } from '../../models/configurator-draft';
import { CriteriaResponse } from '../../models/criteria';
import { CriterionDecision, CriterionDecisions } from '../../models/criteria-list';
import { FilterOptionsLayers } from '../../models/filter-options';
import { SavedList, SavedListCriterionName } from '../../models/saved-lists';
import { CatalogApiService } from '../../services/api/catalog-api.service';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';

/**
 * The procurement project whose own copy of a list the page shows (S11 in project-copy mode,
 * R-PRJ-06; S6-12): its name for `criteria.list.projectCopy` and the title, and whether it is
 * archived — then the copy is read-only (owner decision S6-12 D3 (a)).
 */
export interface SavedListProject {
  id: string;
  name: string;
  archived: boolean;
}

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
      /** The release that is active now; null without one. Another one than the list's shows the older-release notice (R-LST-14). */
      activeRelease: string | null;
      /** The project for a project's copy at `/projekte/:id/kriterienliste`; null for a list of the organisation. */
      project: SavedListProject | null;
    }
  | { kind: 'notFound' }
  | { kind: 'server' };

/** The view the resolver loaded, handed to the page; one per route (route-level provider). */
@Injectable()
export class SavedListLoad {
  view: SavedListView = { kind: 'notFound' };
  /**
   * Reads the same list again as the resolver did — „Neue Fassung laden“ of S37 (R-CAN-11,
   * S6-21b). Set by the resolver; without one the view stays as it is.
   */
  reload: () => Promise<SavedListView> = () => Promise.resolve(this.view);
}

/**
 * What a page that led to a saved list hands it in the navigation state (memory only, like the
 * draft): the report of „Neue Liste mit derselben Auswahl“ (R-LST-14) or of „Auswahl ändern“
 * (owner decision S5-21 4a), shown once on arrival.
 */
export type SavedListArrival =
  | { kind: 'copied'; releaseVersion: string; droppedValues: string[]; notCarried: SavedListCriterionName[] }
  | { kind: 'selectionChanged'; droppedDecisions: SavedListCriterionName[] };

/** The navigation state of a link to a saved list. */
export interface SavedListNavigationState {
  savedListArrival?: SavedListArrival;
}

/** The selection of a saved or shared list (S16) as the catalog endpoints read it. */
export function selectionOf(list: Pick<SavedList, 'selection'>): CriteriaSelection {
  const { useCaseTemplateId, appTypeIds, productPartIds, testTypeIds } = list.selection;
  return {
    ...(useCaseTemplateId === null ? {} : { useCase: useCaseTemplateId }),
    appTypeIds: [...appTypeIds],
    productPartIds: [...productPartIds],
    testTypeIds: [...testTypeIds],
  };
}

/**
 * The stored decisions of a saved or shared list (S16) by criterion key, as `pa-criterion-card` reads them: the server names each
 * criterion by its revision in the pinned release, the criteria answer of that release gives its key.
 */
export function decisionsOf(list: Pick<SavedList, 'criteria'>, criteria: CriteriaResponse): CriterionDecisions {
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
 * `release=<version>`, also when a newer release is active, CAT-06), and which release is active
 * (the filter values without a release; none when that answers 404). A 404 or a malformed id (400)
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
    const [criteria, options, active] = await Promise.all([
      catalog.getCriteria(selection, list.releaseVersion),
      catalog.getFilterOptions(selection, list.releaseVersion),
      catalog.getFilterOptions({ appTypeIds: [], productPartIds: [] }).then(
        (answer) => answer.release,
        () => null,
      ),
    ]);
    return {
      kind: 'list',
      list,
      selection,
      criteria,
      options: options.layers,
      decisions: decisionsOf(list, criteria),
      activeRelease: active,
      project: null,
    };
  } catch {
    // A stored list names a stored release (CAT-05); a failure here is the server's.
    return { kind: 'server' };
  }
}
