// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { computed, inject } from '@angular/core';
import { patchState, signalStoreFeature, withComputed, withMethods, withState } from '@ngrx/signals';
import { withLoading } from '@app/shared/state/with-loading';

import {
  CONFIGURATOR_LAYERS,
  ConfiguratorDraft,
  ConfiguratorLayer,
  EMPTY_DRAFT,
  ResetReport,
  UseCaseChoice,
} from '../models/configurator-draft';
import { FilterOptionsLayers } from '../models/filter-options';
import { CatalogApiService } from '../services/api/catalog-api.service';
import {
  chooseUseCase,
  isComplete,
  SelectionChange,
  toCatalogSelection,
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
}

export const initialConfiguratorState: ConfiguratorState = {
  release: null,
  draft: EMPTY_DRAFT,
  options: null,
  optionsStatus: 'notLoaded',
  changedSinceAnswer: null,
  reset: null,
};

/**
 * Configurator S10 state (ARCHITECTURE §8.1, R-CON-09): the four-layer draft and the filter options
 * answered for it. Memory only — never written to browser storage, so a reload loses the draft
 * (S11 says so, `criteria.list.draftNote`). Everything lives under `configurator`, because the root
 * store still has the legacy `appTypes`/`productParts`/`testTypes` of `withFilters()`.
 * Options load after every change of steps 1–3; only the answer of the latest request is applied.
 */
export const withConfigurator = () =>
  signalStoreFeature(
    withState({ configurator: initialConfiguratorState }),
    withLoading(),
    withMethods((store, api = inject(CatalogApiService)) => {
      let latestRequest = 0;
      let latestLoad: Promise<void> = Promise.resolve();

      const patch = (changes: Partial<ConfiguratorState>) =>
        patchState(store, (state) => ({ configurator: { ...state.configurator, ...changes } }));

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

      return {
        loadFilterOptions,
        optionsCurrent,
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
