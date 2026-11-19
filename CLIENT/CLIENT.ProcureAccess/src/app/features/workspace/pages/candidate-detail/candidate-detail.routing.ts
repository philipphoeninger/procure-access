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

import { CANDIDATE_TAB_SEGMENTS, EDIT_CANDIDATE_SEGMENT, NEW_CANDIDATE_SEGMENT } from '../../models/candidates';
import { CandidatesApiService } from '../../services/api/candidates-api.service';
import { ProjectsApiService } from '../../services/api/projects-api.service';
import { CandidateFormPage } from '../candidate-form/candidate-form';
import { CandidateApis, CandidateLoad, CandidateView, isProjectEditor, loadCandidateView, paramOf } from './candidate-load';
import { CandidateAssessment } from './assessment/candidate-assessment';
import { CandidateDetailPage } from './candidate-detail';
import { CandidateEvidence } from './evidence/candidate-evidence';
import { CandidateOverview } from './overview/candidate-overview';

function apis(): CandidateApis {
  return {
    projects: inject(ProjectsApiService),
    candidates: inject(CandidatesApiService),
    configurator: inject(ConfiguratorApiService),
    catalog: inject(CatalogApiService),
  };
}

/** Loads with the loading bar and hands the view to the page (route-level {@link CandidateLoad}). */
async function loadInto(load: CandidateLoad, loading: () => Promise<CandidateView>): Promise<CandidateView> {
  const store = inject(ProcureAccessStore);
  store.incrementLoadingCount();
  try {
    load.view.set(await loading());
  } finally {
    store.decrementLoadingCount();
  }
  return load.view();
}

function errorTitle(view: CandidateView): string {
  return view.kind === 'notFound' ? 'pageTitle.notFound' : 'pageTitle.server';
}

/**
 * Loads project, candidate and the project's copy of a list before S34 shows, so the route change
 * titles, focuses and announces „Überblick – {{product}} – {{project}}“ with the names known
 * (A11Y-03). A project or candidate the user cannot see (TEN-01) shows „Seite nicht gefunden“ in
 * place, any other failure the S80 server text. A tab change keeps the view (the resolver runs only
 * when the candidate id changes).
 */
export const candidateResolver: ResolveFn<string> = async (route) => {
  const load = inject(CandidateLoad);
  const sources = apis();
  const view = await loadInto(load, () => loadCandidateView(sources, paramOf(route, 'id'), paramOf(route, 'pid'), true));
  return view.kind === 'candidate' ? 'pageTitle.candidate' : errorTitle(view);
};

/**
 * Loads the project (and for „bearbeiten“ the candidate) before S33 shows: „Produkt hinzufügen –
 * {{project}} – Beschaffungsprojekte“ or „Produkt bearbeiten – {{product}} – {{project}}“ (owner
 * decision S6-16 D1 (a)). A Reader gets „Kein Zugriff“ in place: only Editor and Owner add or change
 * products (R-CAN-02); the server refuses them too. An archived project shows the form's note.
 */
export const candidateFormResolver: ResolveFn<string> = async (route) => {
  const load = inject(CandidateLoad);
  const store = inject(ProcureAccessStore);
  const sources = apis();
  const candidateId = route.paramMap.has('pid') ? paramOf(route, 'pid') : null;
  const view = await loadInto(load, () => loadCandidateView(sources, paramOf(route, 'id'), candidateId, false));
  if (view.kind !== 'candidate') return errorTitle(view);
  if (!isProjectEditor(store.organizations(), view.project)) return 'pageTitle.forbidden';
  return candidateId === null ? 'pageTitle.candidate.new' : 'pageTitle.candidate.edit';
};

/**
 * The candidates of a project below `/projekte/:id/produkte` (lazy, signed in only): S33 „neu“ and
 * „:pid/bearbeiten“, S34 at `:pid` with its tabs as child routes (R-CAN-03; Figma mapping §2:
 * `pruefnachweis`, `einschaetzung`). The tabs share the shell's {@link CandidateLoad}.
 */
export const CANDIDATE_ROUTES: Routes = [
  {
    path: NEW_CANDIDATE_SEGMENT,
    component: CandidateFormPage,
    canActivate: [authGuard],
    providers: [CandidateLoad],
    resolve: { [PAGE_TITLE_KEY]: candidateFormResolver },
  },
  {
    path: `:pid/${EDIT_CANDIDATE_SEGMENT}`,
    component: CandidateFormPage,
    canActivate: [authGuard],
    providers: [CandidateLoad],
    resolve: { [PAGE_TITLE_KEY]: candidateFormResolver },
  },
  {
    path: ':pid',
    component: CandidateDetailPage,
    canActivate: [authGuard],
    providers: [CandidateLoad],
    resolve: { [PAGE_TITLE_KEY]: candidateResolver },
    children: [
      { path: CANDIDATE_TAB_SEGMENTS.overview, pathMatch: 'full', component: CandidateOverview },
      { path: CANDIDATE_TAB_SEGMENTS.evidence, component: CandidateEvidence },
      { path: CANDIDATE_TAB_SEGMENTS.assessment, component: CandidateAssessment },
    ],
  },
];
