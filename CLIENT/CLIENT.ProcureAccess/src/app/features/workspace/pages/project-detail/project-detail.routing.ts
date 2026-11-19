// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject } from '@angular/core';
import { ResolveFn, Routes } from '@angular/router';

import { PAGE_TITLE_KEY } from '@app/core/a11y/page-title.strategy';
import { authGuard } from '@app/core/guards/auth.guard';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { CatalogApiService } from '@app/features/configurator/services/api/catalog-api.service';
import { ConfiguratorApiService } from '@app/features/configurator/services/api/configurator-api.service';

import { CANDIDATES_SEGMENT } from '../../models/candidates';
import { PROJECT_CRITERIA_LIST_SEGMENT } from '../../models/projects';
import { ProjectsApiService } from '../../services/api/projects-api.service';
import { loadProjectView, ProjectLoad } from './project-detail-load';
import { ProjectDetailPage } from './project-detail';

/**
 * Loads the project and the facts of its list before S32 shows, so the route change titles,
 * focuses and announces „{{project}} – Beschaffungsprojekte“ with the name already known (A11Y-03).
 * A project the user cannot see (unknown, or of another organisation: 404, TEN-01) shows „Seite
 * nicht gefunden“ in place, any other failure the S80 server text, as a saved list does
 * ({@link loadProjectView}). The loading bar shows meanwhile.
 */
export const projectResolver: ResolveFn<string> = async (route) => {
  const load = inject(ProjectLoad);
  const store = inject(ProcureAccessStore);
  const apis = { projects: inject(ProjectsApiService), configurator: inject(ConfiguratorApiService), catalog: inject(CatalogApiService) };
  store.incrementLoadingCount();
  try {
    load.view = await loadProjectView(apis, route.paramMap.get('id') ?? '');
  } finally {
    store.decrementLoadingCount();
  }
  switch (load.view.kind) {
    case 'project':
      return 'pageTitle.project';
    case 'notFound':
      return 'pageTitle.notFound';
    default:
      return 'pageTitle.server';
  }
};

/**
 * S32 at `/projekte/:id`, lazy; signed in only (R-PRJ-03 with S6-14; the placeholder of S6-13); below it S11 of the
 * project's copy and the project's products (S33, S34).
 */
export const PROJECT_ROUTES: Routes = [
  {
    path: '',
    pathMatch: 'full',
    component: ProjectDetailPage,
    canActivate: [authGuard],
    providers: [ProjectLoad],
    resolve: { [PAGE_TITLE_KEY]: projectResolver },
  },
  {
    // S11 of the project's own copy of a list (R-PRJ-06; S6-12), lazy: the configurator's page
    // stays out of S32's chunk.
    path: PROJECT_CRITERIA_LIST_SEGMENT,
    loadChildren: () =>
      import('../project-criteria-list/project-criteria-list.routing').then((m) => m.PROJECT_CRITERIA_LIST_ROUTES),
  },
  {
    // The products of the project (S6-16): S33 at `produkte/neu` and `produkte/:pid/bearbeiten`,
    // S34 with its tabs at `produkte/:pid`; lazy, so S32's chunk stays small.
    path: CANDIDATES_SEGMENT,
    loadChildren: () => import('../candidate-detail/candidate-detail.routing').then((m) => m.CANDIDATE_ROUTES),
  },
];
