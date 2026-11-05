// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { inject } from '@angular/core';
import {
  patchState,
  signalStoreFeature,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';
import { AuthService } from '../services/auth.service';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { withLoading } from '@app/shared/state/with-loading';
import { IdentityApiService } from '../services/identity-api.service';
import { UserDto } from '../models/user.dto';

export type IdentityState = { user: UserDto | null };

export const withIdentity = () => signalStoreFeature(
    withState<IdentityState>({ user: null }),
    withLoading(),
    withMethods((
      state, 
      authService = inject(AuthService),
      identityApiService = inject(IdentityApiService),
      snackbarService = inject(SnackbarService)
    ) => ({
      async loadIdentity() {
        if (this.isAuthenticated() && !state.user()) {
          const user = await authService.me();
          this.setUser(user);
          return user;
        }
        return null;
      },
      setUser(user: UserDto | null) {
        patchState(state, {
          user
        });
      },
      isAuthenticated() {
        return authService.isAuthenticated();
      },
      async deleteAccount() {
        state.incrementLoadingCount();
        try {
          const error = await identityApiService.deleteAccount();
          if (!error) authService.logout();
          snackbarService.showInfo(
            error ? "Something went wrong.\nPlease try again." : "Account deleted"
          );
        } finally {
          state.decrementLoadingCount();
        }
      }
    })),
    withComputed(() => ({ }))
  );
