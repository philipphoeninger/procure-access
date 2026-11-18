// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

// Procurement projects (SPEC R-PRJ-01 – R-PRJ-05; ARCHITECTURE §7.2; S6-11 server, S6-13 client).
// Types and route paths only, so app.routes.ts and the header can name the paths without loading a
// store or service (CLAUDE.md §3, client routing).

/** S30 „Beschaffungsprojekte“ at `/projekte`, a project (S32) at `/projekte/:id`. */
export const PROJECTS_PATH = 'projekte';

/** The address of a project's overview S32. */
export function projectPath(id: string): string {
  return `/${PROJECTS_PATH}/${id}`;
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

/** A project (`GET /api/v1/projects/{id}` and the answer of creating it). The creator is not served (S6-11 4 (a)). */
export interface Project extends ProjectSummary {
  organizationId: string;
  createdAt: string;
}

/** The orders of S30's „Sortieren nach“ (UX copy U162): only by date or name (Handoff §5.3). */
export type ProjectSort = 'modified' | 'name';
