import { computed } from '@angular/core';
import {
  patchState,
  signalStoreFeature,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';

export function withLoading() {
  return signalStoreFeature(
    withState({ loadingCount: 0 }),
    withMethods((state) => ({
      incrementLoadingCount() {
        patchState(state, {
          loadingCount: state.loadingCount() + 1
        });
      },
      decrementLoadingCount() {
        patchState(state, {
          loadingCount: state.loadingCount() - 1
        });
      }
    })),
    withComputed((state) => ({
      isLoading: computed(() => {
        return state.loadingCount() > 0;
      })
    }))
  );
}

