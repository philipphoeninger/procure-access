// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Procurement projects (SPEC R-PRJ-01 – R-PRJ-06; ARCHITECTURE §7.2; S6-11 server, S6-13 client,
// S6-12 the project's own copy of a list).
// Types and route paths only, so app.routes.ts and the header can name the paths without loading a
// store or service (CLAUDE.md §3, client routing).

import type { AssessmentProgress, CandidateFulfillment, TestKind, TestStatus } from './candidates';

/**
 * S30 „Beschaffungsprojekte“ at `/projekte`, a project (S32) at `/projekte/:id`, the project's own
 * copy of a list (S11 in project-copy mode) at `/projekte/:id/kriterienliste` (owner decision S6-12 D4 (a)).
 */
export const PROJECTS_PATH = 'projekte';

/** The address of a project's overview S32. */
export function projectPath(id: string): string {
  return `/${PROJECTS_PATH}/${id}`;
}

/** The segment of S11 for the project's copy below its S32 address. */
export const PROJECT_CRITERIA_LIST_SEGMENT = 'kriterienliste';

/** The address of S11 for the project's own copy of a list (R-PRJ-06). */
export function projectCriteriaListPath(id: string): string {
  return `${projectPath(id)}/${PROJECT_CRITERIA_LIST_SEGMENT}`;
}

/** The status as the API names it; the UX copy key is `projects.status.<status>`. */
export type ProjectStatus = 'open' | 'done' | 'archived';

/** The UX copy key of a status: „Laufend“, „Abgeschlossen“ or „Archiviert“. */
export function statusKey(status: ProjectStatus): string {
  return `projects.status.${status}`;
}

/** Longest project name (SPEC §7.2, R-PRJ-02), counted like the server's `string.Length` after trimming. */
export const PROJECT_NAME_MAX_LENGTH = 120;

/** One project of S30 (`GET /api/v1/organizations/{organizationId}/projects`, R-PRJ-01). */
export interface ProjectSummary {
  id: string;
  name: string;
  status: ProjectStatus;
  /** The number of candidate products; 0 until they exist (S6-15). */
  productCount: number;
  modifiedAt: string;
}

/**
 * The project's own copy of a list (S6-12, ARCHITECTURE C53): its id — S11 reads it as a saved list —,
 * its name, which is the name of the list it was copied from, and its release.
 */
export interface ProjectCriteriaList {
  id: string;
  name: string;
  releaseVersion: string;
}

/**
 * What S32 names of a test evidence (S6-14): its kind and, for a catalog procedure, its key
 * (`testType.<key>`) or, for „Andere Prüfung“, the name entered.
 */
export interface ProjectEvidenceType {
  testKind: TestKind;
  testTypeId: string | null;
  otherTestTypeName: string | null;
}

/**
 * A candidate in the product list of S32 (R-PRJ-03). The server lists them in the order they were
 * added; S32 shows the last added first (`projects.detail.orderNote`). Progress and the seven counts
 * are null while the project has no list (S6-18).
 */
export interface ProjectCandidate {
  id: string;
  name: string;
  vendor: string | null;
  testStatus: TestStatus;
  /** In the order they were added; empty unless `tested`. */
  testEvidences: ProjectEvidenceType[];
  progress: AssessmentProgress | null;
  fulfillment: CandidateFulfillment | null;
}

/** A project (`GET /api/v1/projects/{id}` and the answers of creating and changing it). The creator is not served (S6-11 4 (a)). */
export interface Project extends ProjectSummary {
  organizationId: string;
  /** The `ETag` of the answer, sent back in `If-Match` with a change (API-03, S6-20); not a member of the body. */
  etag: string;
  createdAt: string;
  /** The project's copy of a list; null while it has none (S6-12). The answer of creating a project has none yet. */
  criteriaList: ProjectCriteriaList | null;
  /**
   * True once a candidate has an assessment other than „Nicht eingeschätzt“: the list can no longer
   * be replaced and S32 shows `projects.detail.criteriaLocked` (R-PRJ-04, WSP-02; S6-14).
   */
  criteriaListLocked: boolean;
  /** The candidate products, in the order they were added (S6-15). */
  candidates: ProjectCandidate[];
}

/**
 * Body of `PATCH /api/v1/projects/{id}` (R-PRJ-05): what is named changes. An archived project
 * changes only its status (owner decision S6-11 2 (a)).
 */
export interface ProjectChange {
  name?: string;
  status?: ProjectStatus;
}

/** The statuses in the order of the UX copy („Laufend“, „Abgeschlossen“, „Archiviert“). */
export const PROJECT_STATUSES: readonly ProjectStatus[] = ['open', 'done', 'archived'];

/** The orders of S30's „Sortieren nach“ (UX copy U162): only by date or name (Handoff §5.3). */
export type ProjectSort = 'modified' | 'name';
