import { computed, inject } from '@angular/core';
import {
  patchState,
  signalStoreFeature,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';
import { User } from '../models/user.model';
import { AuthService } from '../services/auth.service';

export type IdentityState = { user: User | null };

export const withIdentity = () => signalStoreFeature(
    withState<IdentityState>({ user: null }),
    withMethods((state, authService = inject(AuthService)) => ({
      setUser(user: User | null) {
        patchState(state, {
          user
        });
      },
      isAuthenticated() {
        return authService.isAuthenticated();
      }
    })),
    withComputed((state) => ({ }))
  );
