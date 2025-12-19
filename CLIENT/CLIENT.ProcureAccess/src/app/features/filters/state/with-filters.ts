import { computed } from '@angular/core';
import { initialAppState } from '@app/core/models/appState.interface';
import {
  patchState,
  signalStoreFeature,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';

export type FiltersState = { selectedFilters: number[] };

export function withFilters() {
  return signalStoreFeature(
    withState<FiltersState>({ selectedFilters: initialAppState.selectedFilters }),
    withMethods((state) => ({
      setSelectedFilters(selectedFilters: number[]) {
        patchState(state, {
          selectedFilters
        });
      }
    })),
    withComputed((state) => ({
      selectedFiltersCount: computed(() => {
        return state.selectedFilters().length;
      })
    }))
  );
}
