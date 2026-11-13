// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { computed, inject } from '@angular/core';
import { Router } from '@angular/router';
import { patchState, signalStoreFeature, withComputed, withMethods, withState } from '@ngrx/signals';

import { SnackbarService } from '@app/core/services/snackbar.service';
import { withLoading } from '@app/shared/state/with-loading';
import { SessionResponse, SessionUser } from '../models/session';
import { User } from '../models/user.model';
import { AuthService } from '../services/auth.service';
import { IdentityApiService } from '../services/identity-api.service';

/** `unknown` until the first probe answered or failed; `ready` from then on. */
export type SessionStatus = 'unknown' | 'ready';

export interface IdentityState {
  /** The legacy user with the UI settings (`GET /api/me`), read while the legacy screens need it. */
  user: User | null;
  /** The session as the probe reports it; null while nobody is signed in (SEC-01). */
  session: SessionUser | null;
  /** Idle limit of the session (ISO 8601), for the warning of step 4; null without a session. */
  sessionExpiresAt: string | null;
  /** Absolute limit of the session (ISO 8601); null without a session. */
  sessionAbsoluteExpiresAt: string | null;
  sessionStatus: SessionStatus;
}

export const initialIdentityState: IdentityState = {
  user: null,
  session: null,
  sessionExpiresAt: null,
  sessionAbsoluteExpiresAt: null,
  sessionStatus: 'unknown',
};

/**
 * Session state of the client (ARCHITECTURE §8.3, §9.1; SPEC R-AUTH-05; S5-08): who is signed in,
 * from `GET /api/v1/auth/session` alone. The session is the Identity cookie, which the browser
 * sends on its own — nothing here reads or writes a token, `localStorage` or `sessionStorage`
 * (SEC-01). The probe runs once at application start (`ProcureAccessStore.init()`), after a sign-in
 * and a sign-out; guards wait for it through {@link sessionReady}.
 */
export const withIdentity = () =>
  signalStoreFeature(
    withState<IdentityState>(initialIdentityState),
    withLoading(),
    withComputed((state) => ({
      /** Somebody is signed in. Replaces the legacy `isAuthenticated()`, which read the JWT. */
      signedIn: computed(() => state.session() !== null),
      /** The permissions of the signed-in user's roles (e.g. `moderation:review`); empty when signed out. */
      permissions: computed(() => state.session()?.permissions ?? []),
    })),
    withMethods(
      (
        state,
        authService = inject(AuthService),
        identityApiService = inject(IdentityApiService),
        snackbarService = inject(SnackbarService),
        router = inject(Router),
      ) => {
        let firstProbe: Promise<User | null> | null = null;

        function setSession(response: SessionResponse): void {
          patchState(state, {
            session: response.user,
            sessionExpiresAt: response.expiresAt ?? null,
            sessionAbsoluteExpiresAt: response.absoluteExpiresAt ?? null,
            sessionStatus: 'ready',
          });
        }

        function clearSession(): void {
          patchState(state, {
            user: null,
            session: null,
            sessionExpiresAt: null,
            sessionAbsoluteExpiresAt: null,
            sessionStatus: 'ready',
          });
        }

        /**
         * Asks the probe who is signed in and, for a signed-in user, loads the legacy user with the
         * UI settings once. A failed probe counts as „nobody signed in“ and never rejects: the app
         * starts either way, and the next state-changing request answers 400 `antiforgery`, which
         * its caller handles like any other failure.
         */
        async function loadSession(): Promise<User | null> {
          let response: SessionResponse;
          try {
            response = await authService.session();
          } catch {
            clearSession();
            return null;
          }
          setSession(response);
          if (response.user === null) {
            patchState(state, { user: null });
            return null;
          }
          if (state.user()?.id === response.user.id) {
            return null;
          }
          const user = await authService.me();
          patchState(state, { user });
          return user;
        }

        return {
          loadSession,

          /** Resolves once the first probe answered; starts it when nobody did yet (guards). */
          async sessionReady(): Promise<void> {
            firstProbe ??= loadSession();
            await firstProbe;
          },

          /**
           * Signs in and reloads the session. Rejects with the `HttpErrorResponse` of the login
           * (400 `validation` for a wrong address or password); the form shows it (R-AUTH-03).
           */
          async signIn(email: string, password: string): Promise<User | null> {
            await authService.login(email, password);
            patchState(state, { user: null });
            return loadSession();
          },

          /** Ends the session on the server and forgets it here, also when the request fails (R-AUTH-08). */
          async signOut(): Promise<void> {
            try {
              await authService.logout();
            } finally {
              clearSession();
            }
          },

          setUser(user: User | null) {
            patchState(state, { user });
          },

          hasPermission(permission: string): boolean {
            return state.permissions().includes(permission);
          },

          async deleteAccount() {
            state.incrementLoadingCount();
            try {
              const error = await identityApiService.deleteAccount();
              if (!error) {
                await this.signOut();
                void router.navigateByUrl('/');
              }
              snackbarService.showInfo(error ? 'Something went wrong.\nPlease try again.' : 'Account deleted');
            } finally {
              state.decrementLoadingCount();
            }
          },
        };
      },
    ),
  );
