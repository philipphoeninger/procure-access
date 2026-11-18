// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';

import { loadSavedListView } from '@app/features/configurator/pages/saved-list/saved-list-load';
import { CatalogApiService } from '@app/features/configurator/services/api/catalog-api.service';
import { ConfiguratorApiService } from '@app/features/configurator/services/api/configurator-api.service';
import { decisionOf, levelCounts } from '@app/features/configurator/state/criteria-list-rules';
import { selectionNames } from '@app/features/configurator/state/selection-names';

import { Project } from '../../models/projects';
import { ProjectsApiService } from '../../services/api/projects-api.service';

/**
 * „Umfang“ of the project's list (`projects.detail.criteria.scope.value`, UX copy U55): the
 * included requirement criteria by the level that applies now, and apart from them the included
 * „Geprüft nach …“ criteria, which are named but never counted (CAT-17). A count of the list, not
 * a measure of a product.
 */
export interface ProjectListScope {
  total: number;
  a: number;
  b: number;
  info: number;
  procedures: number;
}

/**
 * What the card „Kriterienliste“ of S32 shows of the project's own copy (Handoff §5.1 S32): its
 * name — the name of the list it was copied from (`copyNote`) — and release, the scope, and the
 * fixed selection by name. `useCase` is null for „Keiner davon passt“; without test types the row
 * „Prüfverfahren“ is left out.
 */
export interface ProjectCriteriaFacts {
  listName: string;
  releaseVersion: string;
  scope: ProjectListScope;
  useCase: string | null;
  appTypes: string[];
  testTypes: string[];
}

/** What S32 shows: the project with the facts of its list (null without one), or the S80 page in place. */
export type ProjectView =
  | { kind: 'project'; project: Project; criteria: ProjectCriteriaFacts | null }
  | { kind: 'notFound' }
  | { kind: 'server' };

/** The view the resolver loaded, handed to the page; one per route (route-level provider). */
@Injectable()
export class ProjectLoad {
  view: ProjectView = { kind: 'notFound' };
}

/** The APIs the loader reads. */
export interface ProjectApis {
  projects: ProjectsApiService;
  configurator: ConfiguratorApiService;
  catalog: CatalogApiService;
}

/**
 * Reads the project and, when it has a list, its copy as S11 of the copy reads it (the list, then
 * the criteria and filter values of its release). A project the user cannot see (404, also a
 * malformed id: 400; TEN-01) is „not found“, any other failure the server's — also a failure to
 * read the copy, since the project names it.
 */
export async function loadProjectView(apis: ProjectApis, id: string): Promise<ProjectView> {
  let project: Project;
  try {
    project = await apis.projects.get(id);
  } catch (error: unknown) {
    const status = error instanceof HttpErrorResponse ? error.status : 0;
    return status === 404 || status === 400 ? { kind: 'notFound' } : { kind: 'server' };
  }
  if (project.criteriaList === null) return { kind: 'project', project, criteria: null };

  const list = await loadSavedListView(apis.configurator, apis.catalog, project.criteriaList.id);
  if (list.kind !== 'list') return { kind: 'server' };

  const included = list.criteria.criteria.filter((criterion) => decisionOf(list.decisions, criterion.key).included);
  const levels = levelCounts(included, list.decisions);
  const names = selectionNames(list.selection, list.options);
  return {
    kind: 'project',
    project,
    criteria: {
      listName: list.list.name,
      releaseVersion: list.list.releaseVersion,
      scope: {
        total: levels.total,
        a: levels.A,
        b: levels.B,
        info: levels.Info,
        procedures: included.filter((criterion) => criterion.kind !== 'requirement').length,
      },
      useCase: names.useCase,
      appTypes: names.appTypes,
      testTypes: names.testTypes,
    },
  };
}
