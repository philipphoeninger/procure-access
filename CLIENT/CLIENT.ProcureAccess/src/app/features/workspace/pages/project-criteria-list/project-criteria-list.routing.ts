// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, ResolveFn, Routes } from '@angular/router';

import { PAGE_TITLE_KEY } from '@app/core/a11y/page-title.strategy';
import { authGuard } from '@app/core/guards/auth.guard';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { SavedListEditor } from '@app/features/configurator/pages/saved-list/saved-list-editor';
import { loadSavedListView, SavedListLoad, SavedListView } from '@app/features/configurator/pages/saved-list/saved-list-load';
import { SavedListPage } from '@app/features/configurator/pages/saved-list/saved-list-page';
import { CatalogApiService } from '@app/features/configurator/services/api/catalog-api.service';
import { ConfiguratorApiService } from '@app/features/configurator/services/api/configurator-api.service';
import { ConfiguratorSettings } from '@app/features/configurator/state/configurator-settings';

import { Project } from '../../models/projects';
import { ProjectsApiService } from '../../services/api/projects-api.service';

/** The project id of the route: the `:id` of `/projekte/:id`, above this route. */
function projectIdOf(route: ActivatedRouteSnapshot): string {
  const ids = route.pathFromRoot.map((step) => step.paramMap.get('id')).filter((id) => id !== null);
  return ids.at(-1) ?? '';
}

/**
 * Reads the project, then its own copy of a list as a saved list (SPEC §8.10) with the criteria
 * and filter values of its release. A project the user cannot see (404, TEN-01) and a project
 * without a list are „not found“; any other failure is the server's.
 */
async function loadProjectCriteriaList(
  projects: ProjectsApiService,
  configurator: ConfiguratorApiService,
  catalog: CatalogApiService,
  settings: ConfiguratorSettings,
  projectId: string,
): Promise<SavedListView> {
  let project: Project;
  try {
    project = await projects.get(projectId);
  } catch (error: unknown) {
    return error instanceof HttpErrorResponse && error.status === 404 ? { kind: 'notFound' } : { kind: 'server' };
  }
  if (project.criteriaList === null) return { kind: 'notFound' };

  const [view, settingsReady] = await Promise.all([
    loadSavedListView(configurator, catalog, project.criteriaList.id),
    settings.ready(),
  ]);
  if (view.kind !== 'list') return view;
  if (!settingsReady) return { kind: 'server' };
  return { ...view, project: { id: project.id, name: project.name, archived: project.status === 'archived' } };
}

/**
 * Loads the project and its copy before S11 shows, so the route change titles, focuses and
 * announces „Kriterienliste – {{project}} – Beschaffungsprojekte“ with the name known (A11Y-03;
 * owner decision S6-12 D6 (a)). „Seite nicht gefunden“ or the S80 server text show in place, as on
 * a saved list. The loading bar shows meanwhile.
 */
export const projectCriteriaListResolver: ResolveFn<string> = async (route) => {
  const load = inject(SavedListLoad);
  const store = inject(ProcureAccessStore);
  const projects = inject(ProjectsApiService);
  const configurator = inject(ConfiguratorApiService);
  const catalog = inject(CatalogApiService);
  const settings = inject(ConfiguratorSettings);
  const projectId = projectIdOf(route);
  load.reload = () => loadProjectCriteriaList(projects, configurator, catalog, settings, projectId);
  store.incrementLoadingCount();
  try {
    load.view = await loadProjectCriteriaList(projects, configurator, catalog, settings, projectId);
  } finally {
    store.decrementLoadingCount();
  }
  switch (load.view.kind) {
    case 'list':
      return 'pageTitle.project.criteria';
    case 'notFound':
      return 'pageTitle.notFound';
    default:
      return 'pageTitle.server';
  }
};

/**
 * S11 of the project's own copy of a list at `/projekte/:id/kriterienliste` (R-PRJ-06; owner
 * decision S6-12 D4 (a)), lazy below S32: the page of a saved list in project-copy mode
 * (`SavedListPage`), signed in only.
 */
export const PROJECT_CRITERIA_LIST_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    component: SavedListPage,
    canActivate: [authGuard],
    providers: [SavedListLoad, SavedListEditor],
    resolve: { [PAGE_TITLE_KEY]: projectCriteriaListResolver },
  },
];
