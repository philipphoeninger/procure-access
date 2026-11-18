// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { computed } from '@angular/core';
import {
  patchState,
  signalStoreFeature,
  type,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';

/**
 * The methods a slice uses to show the shell's loading bar while it loads. A type, not an
 * interface: NgRx's `MethodsDictionary` needs the implicit index signature of a type literal.
 */
export type LoadingCounter = {
  incrementLoadingCount(): void;
  decrementLoadingCount(): void;
};

/**
 * The input of a slice that counts its loads: `signalStoreFeature(LOADING_INPUT, …)`, or
 * `{ ...LOADING_INPUT, state: type<…>() }` when it needs more. A store must add
 * {@link withLoading} before such a slice, or it does not compile (S6-24).
 */
export const LOADING_INPUT = { methods: type<LoadingCounter>() };

/**
 * The shared loading counter of the shell's loading bar (R-SHL-09). The root store adds it once,
 * before every slice; a slice never adds it again but declares it as its input
 * ({@link LOADING_INPUT}), so the store has one `loadingCount` and NgRx does not warn about
 * overridden members in dev mode (S5-02 P3, S6-24). A test store of a slice adds it first.
 */
export function withLoading() {
  return signalStoreFeature(
    withState({ loadingCount: 0 }),
    withMethods((state): LoadingCounter => ({
      incrementLoadingCount() {
        patchState(state, (current) => ({ loadingCount: current.loadingCount + 1 }));
      },
      decrementLoadingCount() {
        patchState(state, (current) => ({ loadingCount: current.loadingCount - 1 }));
      }
    })),
    withComputed((state) => ({
      isLoading: computed(() => {
        return state.loadingCount() > 0;
      })
    }))
  );
}
