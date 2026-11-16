// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import type { PriorityLevel } from './criteria';
import type { ExportCriterionEntry, ExportRequest } from './export';

// Saved criteria lists (SPEC R-SAV-01, R-SAV-02, §8.10; S5-20). Types only and the route path, so
// app.routes.ts can import the path without loading a store or service (CLAUDE.md §3, client routing).

/** S14 „Gespeicherte Kriterienlisten“ at `/kriterienlisten`, a saved list at `/kriterienlisten/:id`. */
export const SAVED_LISTS_PATH = 'kriterienlisten';

/** The address of a saved list (S11 read-only, owner decision S5-20 5a). */
export function savedListPath(id: string): string {
  return `/${SAVED_LISTS_PATH}/${id}`;
}

/**
 * Body of `POST /api/v1/configurations`: the draft in the shape of the anonymous export, without
 * its format, with the organisation the header shows and the list name (SPEC §8.10).
 */
export interface SaveListRequest {
  organizationId: string;
  name: string;
  selection: ExportRequest['selection'];
  criteria: ExportCriterionEntry[];
}

/** One list of S14 (`GET /api/v1/configurations?organizationId=`): no creator (owner decision S5-20 3a). */
export interface SavedListSummary {
  id: string;
  name: string;
  releaseVersion: string;
  createdAt: string;
  modifiedAt: string;
}

/** The decision on one criterion of a saved list, by its revision in the pinned release. */
export interface SavedListCriterion {
  criterionRevisionId: number;
  isIncluded: boolean;
  /** The recommendation when the list was saved (CAT-08). */
  resolvedLevel: PriorityLevel;
  /** Null while the recommendation applies. */
  overrideLevel: PriorityLevel | null;
  overrideWeight: number | null;
  note: string | null;
}

/** A saved list (`GET /api/v1/configurations/{id}`); texts and order come from the criteria of its release. */
export interface SavedList {
  id: string;
  organizationId: string;
  name: string;
  releaseVersion: string;
  createdAt: string;
  modifiedAt: string;
  selection: {
    useCaseTemplateId: string | null;
    appTypeIds: string[];
    productPartIds: string[];
    testTypeIds: string[];
  };
  criteria: SavedListCriterion[];
}

/** The order of S14 (owner decision S5-20b 2a): the most recently changed first, or by name. */
export type SavedListSort = 'modified' | 'name';
