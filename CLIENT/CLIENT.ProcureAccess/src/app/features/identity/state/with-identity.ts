// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { computed, inject } from '@angular/core';
import { patchState, signalStoreFeature, withComputed, withMethods, withState } from '@ngrx/signals';

import { OrganizationSummary } from '@app/features/organization/models/organization';
import { OrganizationApiService } from '@app/features/organization/services/api/organization-api.service';
import { withLoading } from '@app/shared/state/with-loading';
import { SessionResponse, SessionUser } from '../models/session';
import { User } from '../models/user.model';
import { AuthService } from '../services/auth.service';

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
  /** The signed-in user's organisations by name (`GET /api/v1/organizations`); empty without a membership. */
  organizations: OrganizationSummary[];
  /**
   * The organisation the user chose with the switch (R-SHL-03), in memory only: a reload or a new
   * sign-in starts again from the first by name (owner decision S5-18 3a). Null until a switch.
   */
  activeOrganizationId: string | null;
}

export const initialIdentityState: IdentityState = {
  user: null,
  session: null,
  sessionExpiresAt: null,
  sessionAbsoluteExpiresAt: null,
  sessionStatus: 'unknown',
  organizations: [],
  activeOrganizationId: null,
};

/**
 * Session state of the client (ARCHITECTURE §8.3, §9.1; SPEC R-AUTH-05; S5-08): who is signed in,
 * from `GET /api/v1/auth/session` alone. The session is the Identity cookie, which the browser
 * sends on its own — nothing here reads or writes a token, `localStorage` or `sessionStorage`
 * (SEC-01). The probe runs once at application start (`ProcureAccessStore.init()`), after a sign-in
 * and a sign-out; guards wait for it through {@link sessionReady}. For a new signed-in user it also
 * reads the user's organisations, which decide the header state (SignedIn or SignedInNoOrg,
 * R-SHL-02) and the organisation of the switch (R-SHL-03; S5-18).
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
      /**
       * The organisation lists, projects and the compare select are scoped to (R-SHL-03): the one
       * chosen with the switch while the user is still its member, otherwise the first by name;
       * null without a membership (header state SignedInNoOrg, R-SHL-02). A query scope, not a
       * security boundary: the server's filter covers every membership (ARCHITECTURE C144).
       */
      activeOrganization: computed(() => {
        const organizations = state.organizations();
        return organizations.find((o) => o.id === state.activeOrganizationId()) ?? organizations[0] ?? null;
      }),
    })),
    withMethods(
      (
        state,
        authService = inject(AuthService),
        organizationApiService = inject(OrganizationApiService),
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
            organizations: [],
            activeOrganizationId: null,
          });
        }

        /**
         * Reads the signed-in user's organisations. A failure keeps the list as it was (a
         * background request, nothing is shown); an answer for a user who signed out meanwhile is
         * dropped.
         */
        async function loadOrganizations(): Promise<void> {
          const userId = state.session()?.id;
          if (!userId) {
            return;
          }
          try {
            const organizations = await organizationApiService.mine();
            if (state.session()?.id === userId) {
              patchState(state, { organizations });
            }
          } catch {
            // BACKGROUND_REQUEST: the header keeps what it shows.
          }
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
            patchState(state, { user: null, organizations: [], activeOrganizationId: null });
            return null;
          }
          if (state.user()?.id === response.user.id) {
            return null;
          }
          // Another user than before: the switch starts again from the first organisation (3a).
          patchState(state, { organizations: [], activeOrganizationId: null });
          const [user] = await Promise.all([authService.me(), loadOrganizations()]);
          patchState(state, { user });
          return user;
        }

        return {
          loadSession,

          /**
           * Reads the organisations again after a membership changed here: an organisation created
           * (S20), an invitation accepted, the organisation left (S21).
           */
          loadOrganizations,

          /**
           * After the user created or joined an organisation (S20, the invitation link): reads the
           * organisations again and makes the new one the active one, as the user works in it next.
           */
          async organizationJoined(organizationId: string): Promise<void> {
            await loadOrganizations();
            if (state.organizations().some((o) => o.id === organizationId)) {
              patchState(state, { activeOrganizationId: organizationId });
            }
          },

          /**
           * Chooses the organisation lists, projects and the compare select show (R-SHL-03). An id
           * that is not one of the user's organisations changes nothing.
           */
          switchOrganization(organizationId: string): void {
            if (state.organizations().some((o) => o.id === organizationId)) {
              patchState(state, { activeOrganizationId: organizationId });
            }
          },

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

          /**
           * Forgets the session without asking the server: the account was deleted, and the answer
           * of the deletion already ended the session there (R-AUTH-10, SPEC §8.13).
           */
          forgetSession(): void {
            clearSession();
          },

          /**
           * Sends the confirmation e-mail again to the signed-in user's address (R-AUTH-02). Rejects
           * with the `HttpErrorResponse`: 503 `email-delivery` and 429 `rate-limit` for the notice
           * `auth.confirm.required` to show.
           */
          resendConfirmation(): Promise<void> {
            return authService.resendConfirmation();
          },

          setUser(user: User | null) {
            patchState(state, { user });
          },

          hasPermission(permission: string): boolean {
            return state.permissions().includes(permission);
          },
        };
      },
    ),
  );
