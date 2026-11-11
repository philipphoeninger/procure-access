// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { computed } from '@angular/core';
import {
  patchState,
  signalStoreFeature,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';

/**
 * The shared loading counter of the shell's loading bar (R-SHL-09). Many features add this
 * feature again; each `withState` gives the store a new `loadingCount` signal, so the methods read
 * the current value inside `patchState` instead of the signal of their own feature, which would be
 * out of date (S4-12: a configurator load did not show the bar after the legacy loads at start).
 */
export function withLoading() {
  return signalStoreFeature(
    withState({ loadingCount: 0 }),
    withMethods((state) => ({
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
