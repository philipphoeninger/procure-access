// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { ActivatedRouteSnapshot } from '@angular/router';

import { loadSavedListView } from '@app/features/configurator/pages/saved-list/saved-list-load';
import { CatalogApiService } from '@app/features/configurator/services/api/catalog-api.service';
import { ConfiguratorApiService } from '@app/features/configurator/services/api/configurator-api.service';
import { OrganizationSummary } from '@app/features/organization/models/organization';

import { Candidate } from '../../models/candidates';
import { Project } from '../../models/projects';
import { CandidatesApiService } from '../../services/api/candidates-api.service';
import { ProjectsApiService } from '../../services/api/projects-api.service';

/**
 * What S34 Überblick shows of the project's own copy of a list (R-CAN-04): its name and release
 * for the progress line and the names of its app types („Anwendungstyp“). Progress and counts come
 * with the candidate from the server (S6-18; owner decision S6-16 D3 (b)).
 */
export interface CandidateCriteria {
  listName: string;
  releaseVersion: string;
  appTypeNames: string[];
}

/**
 * What S33 and S34 show: the project and, except for a new candidate, the candidate; S34 also the
 * copy of its project (null without one). Or „Seite nicht gefunden“ / the S80 server text in place.
 */
export type CandidateView =
  | { kind: 'candidate'; project: Project; candidate: Candidate | null; criteria: CandidateCriteria | null }
  | { kind: 'notFound' }
  | { kind: 'server' };

/**
 * The view the resolver loaded, handed to the page and its tabs; one per route (route-level
 * provider). A signal: Angular keeps S34 and its tab when only the candidate id changes, and the
 * resolver's next view must reach them.
 */
@Injectable()
export class CandidateLoad {
  readonly view = signal<CandidateView>({ kind: 'notFound' });
}

/**
 * Editor or Owner of the project's organisation: who may add and change its products (R-CAN-02).
 * Whether the project is archived (R-PRJ-05) is the caller's second check.
 */
export function isProjectEditor(organizations: readonly Pick<OrganizationSummary, 'id' | 'role'>[], project: Project): boolean {
  const role = organizations.find((organization) => organization.id === project.organizationId)?.role;
  return role === 'owner' || role === 'editor';
}

/** The route parameter `name` of the route or a route above it (`:id` of the project, `:pid` of the candidate). */
export function paramOf(route: ActivatedRouteSnapshot, name: string): string {
  const values = route.pathFromRoot.map((step) => step.paramMap.get(name)).filter((value) => value !== null);
  return values.at(-1) ?? '';
}

/** The APIs the loader reads. */
export interface CandidateApis {
  projects: ProjectsApiService;
  candidates: CandidatesApiService;
  configurator: ConfiguratorApiService;
  catalog: CatalogApiService;
}

/**
 * Reads the project and, with `candidateId`, the candidate side by side. A project or candidate the
 * user cannot see (404, TEN-01) and a candidate of another project than the address names are „not
 * found“, any other failure the server's. With `withCriteria`, the project's copy of a list is read
 * too, as S11 of the copy reads it (the list, then the criteria and filter values of its release);
 * a failure there is the server's, since the project names the copy.
 */
export async function loadCandidateView(
  apis: CandidateApis,
  projectId: string,
  candidateId: string | null,
  withCriteria: boolean,
): Promise<CandidateView> {
  let project: Project;
  let candidate: Candidate | null;
  try {
    [project, candidate] = await Promise.all([
      apis.projects.get(projectId),
      candidateId === null ? Promise.resolve(null) : apis.candidates.get(candidateId),
    ]);
  } catch (error: unknown) {
    const status = error instanceof HttpErrorResponse ? error.status : 0;
    return status === 404 || status === 400 ? { kind: 'notFound' } : { kind: 'server' };
  }
  if (candidate !== null && candidate.projectId !== project.id) return { kind: 'notFound' };

  if (!withCriteria || project.criteriaList === null) return { kind: 'candidate', project, candidate, criteria: null };
  const list = await loadSavedListView(apis.configurator, apis.catalog, project.criteriaList.id);
  if (list.kind !== 'list') return { kind: 'server' };

  const selected = new Set(list.selection.appTypeIds);
  return {
    kind: 'candidate',
    project,
    candidate,
    criteria: {
      listName: list.list.name,
      releaseVersion: list.list.releaseVersion,
      // The filter values come in catalog order (R-CON-03), so the names do too.
      appTypeNames: list.options.appTypes.filter((type) => selected.has(type.id)).map((type) => type.name),
    },
  };
}
