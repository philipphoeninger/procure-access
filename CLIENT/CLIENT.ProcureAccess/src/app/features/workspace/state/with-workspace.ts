// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject } from '@angular/core';
import { patchState, signalStoreFeature, withMethods, withState } from '@ngrx/signals';

import { LOADING_INPUT } from '@app/shared/state/with-loading';

import { Project, ProjectChange, ProjectCriteriaList, ProjectSummary } from '../models/projects';
import { ProjectsApiService } from '../services/api/projects-api.service';

/**
 * - `idle`: nothing requested since the start or the last reset
 * - `loading`: a request for `organizationId` is running; the earlier list is gone
 * - `loaded`: `items` are the projects of `organizationId`
 * - `failed`: the last request failed; S30 shows the S80 server text in place
 */
export type ProjectsStatus = 'idle' | 'loading' | 'loaded' | 'failed';

/** The projects of one organisation as S30 lists them (R-PRJ-01). */
export interface ProjectsState {
  organizationId: string | null;
  status: ProjectsStatus;
  /** As the server answers them: every project, also the archived ones; S30 filters and sorts. */
  items: ProjectSummary[];
}

/**
 * The criteria S11 of a project's copy took into the list during one visit, for the notice
 * `projects.detail.criteriaAdded` on the next S32 of that project (R-PRJ-06, WSP-03; S6-12).
 */
export interface CriteriaAdded {
  projectId: string;
  count: number;
}

/** The workspace of the active organisation; candidates and assessments join with S6-15 and S6-18. */
export interface WorkspaceState {
  projects: ProjectsState;
  /** Waiting for S32 of its project; null when there is nothing to report. */
  criteriaAdded: CriteriaAdded | null;
}

export const initialWorkspaceState: WorkspaceState = {
  projects: { organizationId: null, status: 'idle', items: [] },
  criteriaAdded: null,
};

/**
 * The workspace slice of the root store (ARCHITECTURE §8.1; S6-13): the procurement projects of the
 * organisation the header shows. In memory only (R-SET-02); the sign-out and another account
 * through the login overlay clear it (`resetWorkspace`, called by the root store's `clearUserData`).
 * S30 loads the projects of the active organisation, again after the organisation switch
 * (R-SHL-03): an answer for an earlier request is dropped, so a slow answer for the previous
 * organisation never shows (TEN-01 on the client). Creating a project (S31) adds it to the list of
 * its organisation; assigning a list gives the project its own copy (S6-12). After S11 of a
 * project's copy took criteria into the list, the next S32 of that project reports how many
 * (`noteCriteriaAdded`, `takeCriteriaAdded`; any way back counts: a link, the header, the browser).
 * The legacy `withProducts()` / `withFavorites()` stay with the legacy product
 * screens until step 6 replaces them (owner decision S6-13 D3 (a)).
 */
export const withWorkspace = () =>
  signalStoreFeature(
    LOADING_INPUT,
    withState({ workspace: initialWorkspaceState }),
    withMethods((store, api = inject(ProjectsApiService)) => {
      let latestLoad = 0;

      const patchProjects = (changes: Partial<ProjectsState>) =>
        patchState(store, (state) => ({
          workspace: { ...state.workspace, projects: { ...state.workspace.projects, ...changes } },
        }));

      return {
        /**
         * Loads the projects of the organisation. Resolves to true when this answer is the one
         * shown (`loaded` or `failed`), false when a later load or a reset replaced it — the
         * caller announces only its own answer. Never rejects: a failure is the status `failed`.
         */
        async loadProjects(organizationId: string): Promise<boolean> {
          const request = ++latestLoad;
          patchProjects({ organizationId, status: 'loading', items: [] });
          store.incrementLoadingCount();
          try {
            const items = await api.list(organizationId);
            if (request !== latestLoad) return false;
            patchProjects({ status: 'loaded', items });
            return true;
          } catch {
            if (request !== latestLoad) return false;
            patchProjects({ status: 'failed' });
            return true;
          } finally {
            store.decrementLoadingCount();
          }
        },

        /**
         * Creates a project of the organisation (R-PRJ-02) and adds it to the loaded list of that
         * organisation. Rejects with the error of the request; S31 shows it.
         */
        async createProject(organizationId: string, name: string): Promise<Project> {
          const project = await api.create(organizationId, name);
          const projects = store.workspace.projects();
          if (projects.organizationId === organizationId && projects.status === 'loaded') {
            const { id, status, productCount, modifiedAt } = project;
            patchProjects({ items: [{ id, name: project.name, status, productCount, modifiedAt }, ...projects.items] });
          }
          return project;
        },

        /**
         * Changes the name, the status or both (R-PRJ-05; S32's dialog „Projekt bearbeiten“, S6-14)
         * and updates the project in the loaded list of its organisation, so S30 shows the change
         * without a new load. Rejects with the error of the request; the dialog shows it.
         */
        async changeProject(id: string, change: ProjectChange): Promise<Project> {
          const project = await api.change(id, change);
          const projects = store.workspace.projects();
          if (projects.organizationId === project.organizationId && projects.status === 'loaded') {
            const { name, status, productCount, modifiedAt } = project;
            patchProjects({
              items: projects.items.map((item) => (item.id === id ? { id, name, status, productCount, modifiedAt } : item)),
            });
          }
          return project;
        },

        /**
         * Gives the project its own copy of an organisation list (R-PRJ-02, R-PRJ-04). Rejects with
         * the error of the request; the caller shows it.
         */
        assignCriteriaList(projectId: string, sourceConfigurationId: string): Promise<ProjectCriteriaList> {
          return api.assignCriteriaList(projectId, sourceConfigurationId);
        },

        /**
         * S11 of the project's copy is left after `count` criteria were taken into the list (R-PRJ-06):
         * kept for the next S32 of that project; nothing when none were taken in.
         */
        noteCriteriaAdded(projectId: string, count: number): void {
          patchState(store, (state) => ({
            workspace: { ...state.workspace, criteriaAdded: count > 0 ? { projectId, count } : null },
          }));
        },

        /** The number S32 of the project reports once; 0 when there is nothing for it. Forgets it either way. */
        takeCriteriaAdded(projectId: string): number {
          const added = store.workspace.criteriaAdded();
          patchState(store, (state) => ({ workspace: { ...state.workspace, criteriaAdded: null } }));
          return added?.projectId === projectId ? added.count : 0;
        },

        /** Forgets the projects (sign-out, another account through the login overlay); a running load is dropped. */
        resetWorkspace(): void {
          latestLoad++;
          patchState(store, { workspace: initialWorkspaceState });
        },
      };
    }),
  );
