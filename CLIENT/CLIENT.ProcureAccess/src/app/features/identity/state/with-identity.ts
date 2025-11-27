import { computed } from '@angular/core';
import {
  patchState,
  signalStoreFeature,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';
import { User } from '../models/user.model';

export type IdentityState = { user: User | null };

export function withIdentity() {
  return signalStoreFeature(
    withState<IdentityState>({ user: null }),
    withMethods((state) => ({
      setUser(user: User | null) {
        patchState(state, {
          user
        });
      }
    })),
    withComputed((state) => ({
    //   isLoading: computed(() => {
    //     return state.loadingCount() > 0;
    //   })
    }))
  );
}