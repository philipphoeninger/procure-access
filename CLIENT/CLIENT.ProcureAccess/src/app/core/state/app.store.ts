// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { patchState, signalStore, withHooks, withMethods } from "@ngrx/signals";
import { withLoading } from "@app/shared/state/with-loading";
import { withIdentity } from "@app/features/identity/state/with-identity";
import { withFilters } from "@app/features/filters/state/with-filters";
import { withCriteria } from "@app/features/criteria/state/with-criteria";
import { withFavorites } from "@app/features/favorites/state/with-favorites";
import { withProducts } from "@app/features/products/state/with-products";
import { withSettings } from "@app/features/settings/state/with-settings";
import { withConfigurator } from "@app/features/configurator/state/with-configurator";
import { withWorkspace } from "@app/features/workspace/state/with-workspace";

/**
 * The root store: the shared loading counter first, then the slices (ARCHITECTURE §8.1). A slice
 * declares what it needs from the features before it (`LOADING_INPUT`, `type<…>()`) and never adds
 * them again, so every member exists once and NgRx does not warn in dev mode (S6-24).
 */
export const ProcureAccessStore = signalStore(
    { providedIn: 'root' },
    withLoading(),
    withIdentity(),
    withFilters(),
    withCriteria(),
    withFavorites(),
    withProducts(),
    withSettings(),
    withConfigurator(),
    withWorkspace(),
    withMethods((state) => {
        return {
            /**
             * Signs in (R-AUTH-03, R-AUTH-07). Rejects with the error of the login request; the
             * sign-in form shows it. The user's settings follow the new session (withSettings).
             */
            async startSession(email: string, password: string): Promise<void> {
                await state.signIn(email, password);
            },

            /**
             * Signs out (R-AUTH-08): the server deletes the cookie and issues a new antiforgery token,
             * and every in-memory state of the user goes — the configurator draft and list too, the
             * projects of the workspace, the legacy favourites; appearance, motion and contrast
             * follow the system again (withSettings).
             */
            async endSession(): Promise<void> {
                try {
                    await state.signOut();
                } finally {
                    this.forgetUser();
                }
            },

            /**
             * Forgets the user without a sign-out request: every in-memory state goes, as after the
             * sign-out. For the deleted account, whose session the server ended with the deletion
             * (R-AUTH-10, S61).
             */
            forgetUser(): void {
                state.forgetSession();
                this.clearUserData();
            },

            /**
             * Clears the in-memory data of the user that a sign-out clears, but keeps the session:
             * another account signed in through the login overlay must not see or send the
             * previous user's draft, list, projects or legacy data (owner decision S5-30 D3 (b)).
             */
            clearUserData(): void {
                state.resetConfigurator();
                state.resetWorkspace();
                patchState(state, { productSaves: [], filterSets: [] });
            },

            async init() {
                // the session probe (ARCHITECTURE §8.3): who is signed in, and the antiforgery cookie;
                // a signed-in user's settings are read when the session arrives (withSettings)
                await state.sessionReady();

                // products
                await state.loadProducts();
            }
        };
    }),
    withHooks((store) => ({
        onInit() {
            // The error interceptor reports a failed start-up read (offline notice, S80 page or
            // snackbar); caught here so that it does not end as an unhandled rejection.
            store.init().catch(() => undefined);
        }
    }))
);
