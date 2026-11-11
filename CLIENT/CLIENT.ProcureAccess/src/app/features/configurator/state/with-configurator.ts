// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { computed, inject } from '@angular/core';
import { patchState, signalStoreFeature, withComputed, withMethods, withState } from '@ngrx/signals';
import { problemFieldErrors, problemTypeName } from '@app/core/errors/problem-details';
import { withLoading } from '@app/shared/state/with-loading';

import {
  CONFIGURATOR_LAYERS,
  ConfiguratorDraft,
  ConfiguratorLayer,
  EMPTY_DRAFT,
  ResetReport,
  UseCaseChoice,
} from '../models/configurator-draft';
import { Criterion, CriteriaResponse, PriorityLevel } from '../models/criteria';
import { CriteriaListFilter, CriterionDecisions, EMPTY_LIST_FILTER } from '../models/criteria-list';
import { FilterOptionsLayers } from '../models/filter-options';
import { CatalogApiService, CRITERIA_LIST_PROBLEMS } from '../services/api/catalog-api.service';
import {
  filterCriteria,
  overridePriority,
  resetPriority,
  setIncluded,
  setNote,
  setWeight,
} from './criteria-list-rules';
import { PRIORITY_WEIGHT_RANGE } from './priority-weight-range';
import {
  chooseUseCase,
  isComplete,
  layerOfField,
  SelectionChange,
  toCatalogSelection,
  toCriteriaSelection,
  toggleAppType,
  toggleProductPart,
  toggleTestType,
  withAlwaysSelectedParts,
} from './selection-rules';

/**
 * - `notLoaded`: nothing requested yet
 * - `loading`: a request is running; earlier options stay readable
 * - `loaded`: the options match the draft
 * - `noRelease`: 404 — no active release, or the pinned one is unknown (O-S4-3: the legacy filter stays)
 * - `failed`: any other failure; `loadFilterOptions()` tries again
 */
export type FilterOptionsStatus = 'notLoaded' | 'loading' | 'loaded' | 'noRelease' | 'failed';

/**
 * What the server said to „Kriterienliste anzeigen“ (R-CON-10):
 * - `accepted`: S11 may show the selection
 * - `rejected`: 400 `validation` or `selection-excluded`; `layer` of the first field error (null
 *   when the path names no layer) and its message
 * - `noRelease`: 404 — the pinned release is unknown or no release is active (O-S4-3)
 * - `failed`: any other failure; the error interceptor already shows the system state
 */
export type SubmitResult =
  | { kind: 'accepted' }
  | {
      kind: 'rejected';
      problem: 'validation' | 'selection-excluded';
      layer: ConfiguratorLayer | null;
      message: string;
    }
  | { kind: 'noRelease' }
  | { kind: 'failed' };

/**
 * - `notLoaded`: S11 has not asked yet
 * - `loading`: a request is running; the list of an earlier selection is gone
 * - `loaded`: `answer` holds the criteria of the current selection
 * - `noRelease`: 404 — the pinned release is unknown or no release is active (O-S4-3)
 * - `failed`: any other failure, also a 400 for a selection S10 had sent successfully (the error
 *   interceptor shows the generic snackbar for it, owner decision O-S4-34 (a))
 */
export type CriteriaListStatus = 'notLoaded' | 'loading' | 'loaded' | 'noRelease' | 'failed';

/** Search applies this long after the last keystroke (R-LST-06, Handoff §5.1 S11). */
export const CRITERIA_SEARCH_DEBOUNCE_MS = 300;

/** The criteria list S11 for the draft (R-LST-01 – R-LST-06), in memory like the draft (R-CON-09). */
export interface CriteriaListState {
  status: CriteriaListStatus;
  answer: CriteriaResponse | null;
  /** By criterion key; kept when the selection changes (owner decision O-S4-24 (a)). */
  decisions: CriterionDecisions;
  /** The search field as typed; `filter.query` follows it after {@link CRITERIA_SEARCH_DEBOUNCE_MS}. */
  searchInput: string;
  filter: CriteriaListFilter;
}

export const initialCriteriaListState: CriteriaListState = {
  status: 'notLoaded',
  answer: null,
  decisions: {},
  searchInput: '',
  filter: EMPTY_LIST_FILTER,
};

export interface ConfiguratorState {
  /** Release of the first answer; every later request names it, so one draft never mixes releases (ADR-002). */
  release: string | null;
  draft: ConfiguratorDraft;
  options: FilterOptionsLayers | null;
  optionsStatus: FilterOptionsStatus;
  /**
   * Highest layer changed since the last applied answer: the options of the layers below it are
   * out of date, so choices there wait (null = all options current).
   */
  changedSinceAnswer: ResetReport['changedLayer'] | null;
  /** What the last change cleared, for the reset notice; gone with the next selection (R-CON-07). */
  reset: ResetReport | null;
  list: CriteriaListState;
}

export const initialConfiguratorState: ConfiguratorState = {
  release: null,
  draft: EMPTY_DRAFT,
  options: null,
  optionsStatus: 'notLoaded',
  changedSinceAnswer: null,
  reset: null,
  list: initialCriteriaListState,
};

/**
 * Configurator state (ARCHITECTURE §8.1, R-CON-09): the four-layer draft of S10 and the filter
 * options answered for it, and the criteria list of S11 with the user's decisions (`list`, C119).
 * Memory only — never written to browser storage, so a reload loses both (S11 says so,
 * `criteria.list.draftNote`). Everything lives under `configurator`, because the root store still
 * has the legacy `appTypes`/`productParts`/`testTypes` of `withFilters()` and `criteria` of
 * `withCriteria()`. Options load after every change of steps 1–3, the list when S11 asks; only the
 * answer of the latest request is applied.
 */
export const withConfigurator = () =>
  signalStoreFeature(
    withState({ configurator: initialConfiguratorState }),
    withLoading(),
    withMethods((store, api = inject(CatalogApiService), weightRange = inject(PRIORITY_WEIGHT_RANGE)) => {
      let latestRequest = 0;
      let latestLoad: Promise<void> = Promise.resolve();
      let latestListRequest = 0;
      let searchTimer: ReturnType<typeof setTimeout> | undefined;

      const patch = (changes: Partial<ConfiguratorState>) =>
        patchState(store, (state) => ({ configurator: { ...state.configurator, ...changes } }));

      const patchList = (changes: (list: CriteriaListState) => Partial<CriteriaListState>) =>
        patchState(store, (state) => ({
          configurator: {
            ...state.configurator,
            list: { ...state.configurator.list, ...changes(state.configurator.list) },
          },
        }));

      /** Applies a rule to the decisions of a criterion of the loaded list; unknown keys change nothing. */
      function decide(key: string, rule: (decisions: CriterionDecisions, criterion: Criterion) => CriterionDecisions): void {
        const { answer, decisions } = store.configurator.list();
        const criterion = answer?.criteria.find((c) => c.key === key);
        if (criterion === undefined) {
          return;
        }
        const next = rule(decisions, criterion);
        if (next !== decisions) {
          patchList(() => ({ decisions: next }));
        }
      }

      function cancelSearchTimer(): void {
        clearTimeout(searchTimer);
        searchTimer = undefined;
      }

      function loadFilterOptions(): Promise<void> {
        latestLoad = load();
        return latestLoad;
      }

      /**
       * Waits until the options answer the current draft (R-CON-01: „Weiter“ leads to a step whose
       * choices are current). True when they do; false when the last request failed or nothing
       * was loaded, so the caller stays on its step.
       */
      async function optionsCurrent(): Promise<boolean> {
        // A change during the wait starts a newer request: wait for that one too.
        while (store.configurator().changedSinceAnswer !== null && store.configurator().optionsStatus === 'loading') {
          await latestLoad;
        }
        const { changedSinceAnswer, options } = store.configurator();
        return changedSinceAnswer === null && options !== null;
      }

      async function load(): Promise<void> {
        const request = ++latestRequest;
        const { draft, release } = store.configurator();
        patch({ optionsStatus: 'loading' });
        store.incrementLoadingCount();
        try {
          const answer = await api.getFilterOptions(toCatalogSelection(draft), release ?? undefined);
          if (request !== latestRequest) {
            return;
          }
          const current = store.configurator();
          patch({
            release: current.release ?? answer.release,
            options: answer.layers,
            draft: withAlwaysSelectedParts(current.draft, answer.layers),
            optionsStatus: 'loaded',
            changedSinceAnswer: null,
          });
        } catch (error: unknown) {
          if (request === latestRequest) {
            const notFound = error instanceof HttpErrorResponse && error.status === 404;
            patch({ optionsStatus: notFound ? 'noRelease' : 'failed' });
          }
        } finally {
          store.decrementLoadingCount();
        }
      }

      function change(
        layer: ConfiguratorLayer,
        rule: (draft: ConfiguratorDraft, options: FilterOptionsLayers) => SelectionChange,
      ): void {
        const { draft, options, changedSinceAnswer } = store.configurator();
        if (options === null || isWaitingForOptions(layer, changedSinceAnswer)) {
          return;
        }
        const result = rule(draft, options);
        if (result.draft === draft) {
          return;
        }
        // A new selection hides the previous notice; a change that clears again shows its own.
        if (layer === 'testType') {
          patch({ draft: result.draft, reset: result.reset });
          return;
        }
        patch({
          draft: result.draft,
          reset: result.reset,
          changedSinceAnswer: higherOf(layer, changedSinceAnswer),
        });
        void loadFilterOptions();
      }

      /**
       * The status once the options request that is running, or a new one, has answered. The
       * guard of S10 calls it before the page opens: `noRelease` leads to the legacy filter
       * (O-S4-3). A former `noRelease` or failure asks again, because a release may have been
       * activated since.
       */
      async function filterOptionsReady(): Promise<FilterOptionsStatus> {
        const { optionsStatus } = store.configurator();
        if (optionsStatus === 'loading') {
          await latestLoad;
        } else if (optionsStatus !== 'loaded') {
          await loadFilterOptions();
        }
        return store.configurator().optionsStatus;
      }

      /** Sends the complete selection to the criteria endpoint, which checks it again (R-CON-10, CAT-12). */
      async function submitSelection(): Promise<SubmitResult> {
        const { draft, release } = store.configurator();
        try {
          await api.getCriteria(toCriteriaSelection(draft), release ?? undefined);
          return { kind: 'accepted' };
        } catch (error: unknown) {
          const problem = problemTypeName(error);
          if (problem === 'validation' || problem === 'selection-excluded') {
            const [path, messages] = Object.entries(problemFieldErrors(error))[0] ?? ['', []];
            return { kind: 'rejected', problem, layer: layerOfField(path), message: messages.join(' ') };
          }
          if (error instanceof HttpErrorResponse && error.status === 404) {
            patch({ optionsStatus: 'noRelease' });
            return { kind: 'noRelease' };
          }
          return { kind: 'failed' };
        }
      }

      /**
       * Loads the criteria of the draft for S11 with the pinned release (R-CON-10). The answer of a
       * named release is cached by the browser (ARCHITECTURE §9.6), so the request after the check
       * of „Kriterienliste anzeigen“ costs nothing. Only the latest request is applied; the
       * decisions stay (owner decision O-S4-24 (a)).
       */
      async function loadCriteriaList(): Promise<CriteriaListStatus> {
        const request = ++latestListRequest;
        const { draft, release } = store.configurator();
        patchList(() => ({ status: 'loading', answer: null }));
        store.incrementLoadingCount();
        try {
          const answer = await api.getCriteria(toCriteriaSelection(draft), release ?? undefined, CRITERIA_LIST_PROBLEMS);
          if (request === latestListRequest) {
            patchList(() => ({ status: 'loaded', answer }));
          }
        } catch (error: unknown) {
          if (request === latestListRequest) {
            const notFound = error instanceof HttpErrorResponse && error.status === 404;
            patchList(() => ({ status: notFound ? 'noRelease' : 'failed' }));
            if (notFound) {
              patch({ optionsStatus: 'noRelease' });
            }
          }
        } finally {
          store.decrementLoadingCount();
        }
        return store.configurator.list.status();
      }

      return {
        loadFilterOptions,
        loadCriteriaList,
        /** Includes or excludes a criterion; excluded ones stay in the list (R-LST-04). */
        setCriterionIncluded(key: string, included: boolean): void {
          decide(key, (decisions) => setIncluded(decisions, key, included));
        },
        /** Overrides the level; the recommendation stays next to it (R-LST-03). */
        overrideCriterionPriority(key: string, level: PriorityLevel): void {
          decide(key, (decisions, criterion) => overridePriority(decisions, criterion, level));
        },
        /** Sets the weight of a criterion whose level is B, inside the 🔧 range (R-LST-03). */
        setCriterionWeight(key: string, weight: number): void {
          decide(key, (decisions, criterion) => setWeight(decisions, criterion, weight, weightRange));
        },
        /** „Empfehlung wiederherstellen“ (R-LST-03). */
        resetCriterionPriority(key: string): void {
          decide(key, (decisions) => resetPriority(decisions, key));
        },
        /** Keeps the note as typed; the length error comes from `noteLengthError` (R-LST-05). */
        setCriterionNote(key: string, note: string): void {
          decide(key, (decisions) => setNote(decisions, key, note));
        },
        /** The search field as typed; the search applies 300 ms after the last call (R-LST-06). */
        searchCriteria(text: string): void {
          patchList(() => ({ searchInput: text }));
          cancelSearchTimer();
          searchTimer = setTimeout(() => {
            searchTimer = undefined;
            patchList((list) => ({ filter: { ...list.filter, query: text } }));
          }, CRITERIA_SEARCH_DEBOUNCE_MS);
        },
        /** Levels the list shows; empty = every level (R-LST-06, owner decision O-S4-25 (a)). */
        filterCriteriaByPriority(levels: PriorityLevel[]): void {
          patchList((list) => ({ filter: { ...list.filter, levels: [...levels] } }));
        },
        /** „Nur aufgenommene Kriterien anzeigen“ (R-LST-04). */
        showOnlyIncludedCriteria(onlyIncluded: boolean): void {
          patchList((list) => ({ filter: { ...list.filter, onlyIncluded } }));
        },
        /** „Filter zurücksetzen“ of `criteria.list.emptyFilter`: search, levels and inclusion at once. */
        clearCriteriaFilter(): void {
          cancelSearchTimer();
          patchList(() => ({ searchInput: '', filter: EMPTY_LIST_FILTER }));
        },
        optionsCurrent,
        filterOptionsReady,
        submitSelection,
        chooseUseCase(choice: Exclude<UseCaseChoice, { kind: 'unanswered' }>): void {
          change('useCase', (draft, options) => chooseUseCase(draft, choice, options));
        },
        toggleAppType(id: string): void {
          change('appType', (draft, options) => toggleAppType(draft, id, options));
        },
        toggleProductPart(id: string): void {
          change('productPart', (draft, options) => toggleProductPart(draft, id, options));
        },
        toggleTestType(id: string): void {
          change('testType', (draft, options) => toggleTestType(draft, id, options));
        },
      };
    }),
    withComputed((store) => ({
      configuratorComplete: computed(() => isComplete(store.configurator.draft())),
      /** The criteria S11 shows under the filter, in the order of the answer (R-LST-04, R-LST-06). */
      visibleCriteria: computed(() => {
        const { answer, decisions, filter } = store.configurator.list();
        return answer === null ? [] : filterCriteria(answer.criteria, decisions, filter);
      }),
    })),
  );

function isWaitingForOptions(
  layer: ConfiguratorLayer,
  changedSinceAnswer: ConfiguratorState['changedSinceAnswer'],
): boolean {
  return (
    changedSinceAnswer !== null &&
    CONFIGURATOR_LAYERS.indexOf(layer) > CONFIGURATOR_LAYERS.indexOf(changedSinceAnswer)
  );
}

function higherOf(
  layer: Exclude<ConfiguratorLayer, 'testType'>,
  other: ConfiguratorState['changedSinceAnswer'],
): Exclude<ConfiguratorLayer, 'testType'> {
  return other !== null && CONFIGURATOR_LAYERS.indexOf(other) < CONFIGURATOR_LAYERS.indexOf(layer)
    ? other
    : layer;
}
