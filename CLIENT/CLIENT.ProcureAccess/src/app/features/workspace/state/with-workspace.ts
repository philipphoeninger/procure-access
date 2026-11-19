// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject, Injector } from '@angular/core';
import { patchState, signalStoreFeature, withMethods, withState } from '@ngrx/signals';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { LOADING_INPUT } from '@app/shared/state/with-loading';

import {
  AssessmentRow,
  AUTOSAVE_DEBOUNCE_MS,
  CandidateAssessments,
  noteTooLong,
  SAVED_ANNOUNCEMENT_INTERVAL_MS,
} from '../models/assessments';
import { Project, ProjectChange, ProjectCriteriaList, ProjectSummary } from '../models/projects';
import { CandidatesApiService } from '../services/api/candidates-api.service';
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

/**
 * - `idle`: nothing read since the start or the last reset
 * - `loading`: the assessments of `candidateId` are read and none of them are known yet
 * - `loaded`: `view` is the assessment of `candidateId`, with the user's changes
 * - `failed`: the last read failed; S34b shows the S80 server text in place
 */
export type AssessmentsStatus = 'idle' | 'loading' | 'loaded' | 'failed';

/**
 * The assessment tab S34b of one candidate (R-CAN-06; S6-19): as the server answered it, with every
 * change the user made applied at once — also while it waits for its autosave —, so the last state
 * shows again when the user comes back to the tab (Handoff §5.1 S34b).
 */
export interface AssessmentsState {
  candidateId: string | null;
  status: AssessmentsStatus;
  view: CandidateAssessments | null;
}

/** The workspace of the active organisation (S6-13); the assessment tab of a candidate since S6-19. */
export interface WorkspaceState {
  projects: ProjectsState;
  /** Waiting for S32 of its project; null when there is nothing to report. */
  criteriaAdded: CriteriaAdded | null;
  assessments: AssessmentsState;
}

export const initialWorkspaceState: WorkspaceState = {
  projects: { organizationId: null, status: 'idle', items: [] },
  criteriaAdded: null,
  assessments: { candidateId: null, status: 'idle', view: null },
};

/** A changed row waiting for its autosave; without a timer while its note is too long to be saved. */
interface WaitingRow {
  candidateId: string;
  criterionRevisionId: number;
  row: AssessmentRow;
  timer: ReturnType<typeof setTimeout> | undefined;
}

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
 *
 * The assessment tab S34b (S6-19): `loadAssessments` reads a candidate's assessments, and
 * `changeAssessment` applies a change at once and saves the whole row 800 ms after the row's last
 * change (R-CAN-06, Handoff §4.6); the saves of one row run one after another, so the last change is
 * the one stored (the server's last write wins, R-CAN-11). Each save announces
 * `candidate.assessment.saved`, at most every 5 s. A row whose note is too long is not saved until
 * the note is short enough; K6 shows the error. `flushAssessments` saves every waiting row at once:
 * S34b calls it when the tab is left, so a change made just before is not lost. A failed save keeps
 * the error interceptor's system states and the change on screen; the next change of the row sends
 * the whole row again.
 */
export const withWorkspace = () =>
  signalStoreFeature(
    LOADING_INPUT,
    withState({ workspace: initialWorkspaceState }),
    withMethods((store, api = inject(ProjectsApiService)) => {
      // Read when S34b first uses them, so the slice needs neither HTTP nor the texts before
      const injector = inject(Injector);
      const candidates = () => injector.get(CandidatesApiService);
      const announcer = () => injector.get(A11yAnnouncerService);
      let latestLoad = 0;
      let latestAssessmentRead = 0;
      /** Changed rows waiting for their autosave, by {@link rowKey}. */
      const waiting = new Map<string, WaitingRow>();
      /** The save of each row in flight, by {@link rowKey}; the next save of the row starts after it. */
      const sending = new Map<string, Promise<void>>();
      const rowKey = (candidateId: string, criterionRevisionId: number) => `${candidateId}/${criterionRevisionId}`;

      const patchProjects = (changes: Partial<ProjectsState>) =>
        patchState(store, (state) => ({
          workspace: { ...state.workspace, projects: { ...state.workspace.projects, ...changes } },
        }));

      const patchAssessments = (changes: Partial<AssessmentsState>) =>
        patchState(store, (state) => ({
          workspace: { ...state.workspace, assessments: { ...state.workspace.assessments, ...changes } },
        }));

      /** Sends the waiting change of the row, after the row's save in flight; resolves when it settled. */
      function send(key: string): Promise<void> {
        const entry = waiting.get(key);
        const previous = sending.get(key) ?? Promise.resolve();
        if (entry === undefined) return previous;
        clearTimeout(entry.timer);
        if (noteTooLong(entry.row.note)) {
          // Not sent, but kept as unsaved: a new read keeps what the user typed, K6 shows the error
          waiting.set(key, { ...entry, timer: undefined });
          return previous;
        }
        waiting.delete(key);

        const saved: Promise<void> = previous
          .then(() => candidates().saveAssessment(entry.candidateId, entry.criterionRevisionId, entry.row))
          .then(() => announcer().announce('candidate.assessment.saved', undefined, { throttleMs: SAVED_ANNOUNCEMENT_INTERVAL_MS }))
          // The error interceptor shows a failure; the change stays on screen
          .catch(() => undefined)
          .finally(() => {
            if (sending.get(key) === saved) sending.delete(key);
          });
        sending.set(key, saved);
        return saved;
      }

      /**
       * The answer of a read with the rows that still wait or are being saved as the user left them,
       * so a read never shows an older value than the one on its way to the server.
       */
      function withUnsaved(read: CandidateAssessments): CandidateAssessments {
        const shown = store.workspace.assessments().view;
        if (shown?.candidateId !== read.candidateId) return read;
        const unsaved = (id: number) => waiting.has(rowKey(read.candidateId, id)) || sending.has(rowKey(read.candidateId, id));
        return {
          ...read,
          criteria: read.criteria.map((criterion) => {
            const local = unsaved(criterion.criterionRevisionId)
              ? shown.criteria.find((item) => item.criterionRevisionId === criterion.criterionRevisionId)
              : undefined;
            return local === undefined ? criterion : { ...criterion, value: local.value, note: local.note };
          }),
        };
      }

      return {
        /**
         * Reads the assessment tab of a candidate (S34b) with the loading bar. Coming back to the
         * candidate shows the last state at once while it is read again. Never rejects: a failure
         * is the status `failed`; an answer a later read or a reset replaced is dropped.
         */
        async loadAssessments(candidateId: string): Promise<void> {
          const read = ++latestAssessmentRead;
          const shown = store.workspace.assessments();
          if (shown.candidateId !== candidateId || shown.view === null) {
            patchAssessments({ candidateId, status: 'loading', view: null });
          }
          store.incrementLoadingCount();
          try {
            const answer = await candidates().assessments(candidateId);
            if (read === latestAssessmentRead) patchAssessments({ candidateId, status: 'loaded', view: withUnsaved(answer) });
          } catch {
            if (read === latestAssessmentRead) patchAssessments({ candidateId, status: 'failed', view: null });
          } finally {
            store.decrementLoadingCount();
          }
        },

        /**
         * The user changed value or note of a criterion: shown at once, saved as a whole row
         * {@link AUTOSAVE_DEBOUNCE_MS} after the row's last change (R-CAN-06).
         */
        changeAssessment(candidateId: string, criterionRevisionId: number, row: AssessmentRow): void {
          const view = store.workspace.assessments().view;
          if (view?.candidateId === candidateId) {
            patchAssessments({
              view: {
                ...view,
                criteria: view.criteria.map((criterion) =>
                  criterion.criterionRevisionId === criterionRevisionId ? { ...criterion, ...row } : criterion,
                ),
              },
            });
          }
          const key = rowKey(candidateId, criterionRevisionId);
          const earlier = waiting.get(key);
          if (earlier !== undefined) clearTimeout(earlier.timer);
          waiting.set(key, {
            candidateId,
            criterionRevisionId,
            row,
            timer: setTimeout(() => void send(key), AUTOSAVE_DEBOUNCE_MS),
          });
        },

        /** Saves every waiting row now; resolves when every save in flight has settled. */
        async flushAssessments(): Promise<void> {
          for (const key of [...waiting.keys()]) void send(key);
          await Promise.all(sending.values());
        },

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
         * of the project read with `etag` (API-03) and updates the project in the loaded list of its
         * organisation, so S30 shows the change without a new load. Rejects with the error of the
         * request; the dialog shows it.
         */
        async changeProject(id: string, change: ProjectChange, etag: string): Promise<Project> {
          const project = await api.change(id, change, etag);
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
         * Gives the project its own copy of an organisation list (R-PRJ-02, R-PRJ-04), as a change
         * of the project read with `projectEtag` (API-03). Rejects with the error of the request;
         * the caller shows it.
         */
        assignCriteriaList(projectId: string, sourceConfigurationId: string, projectEtag: string): Promise<ProjectCriteriaList> {
          return api.assignCriteriaList(projectId, sourceConfigurationId, projectEtag);
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

        /**
         * Forgets the projects and assessments (sign-out, another account through the login
         * overlay); a running load is dropped, and so are the changes still waiting for their save.
         */
        resetWorkspace(): void {
          latestLoad++;
          latestAssessmentRead++;
          for (const entry of waiting.values()) clearTimeout(entry.timer);
          waiting.clear();
          patchState(store, { workspace: initialWorkspaceState });
        },
      };
    }),
  );
