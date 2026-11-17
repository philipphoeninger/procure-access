// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { patchState, signalStore, withHooks, withMethods, withState } from "@ngrx/signals";
import { AppState, initialAppState } from "../models/appState.interface";
import { withLoading } from "@app/shared/state/with-loading";
import { withIdentity } from "@app/features/identity/state/with-identity";
import { withFilters } from "@app/features/filters/state/with-filters";
import { withCriteria } from "@app/features/criteria/state/with-criteria";
import { withFavorites } from "@app/features/favorites/state/with-favorites";
import { withProducts } from "@app/features/products/state/with-products";
import { withSettings } from "@app/features/settings/state/with-settings";
import { withProposal } from "@app/features/proposal/state/with-proposal";
import { withConfigurator } from "@app/features/configurator/state/with-configurator";

export const ProcureAccessStore = signalStore(
    { providedIn: 'root' },
    withState<AppState>(initialAppState),
    withLoading(),
    withIdentity(),
    withFilters(),
    withCriteria(),
    withFavorites(),
    withProducts(),
    withProposal(),
    withSettings(),
    withConfigurator(),
    withMethods((state) => {
        return {
            /**
             * Signs in (R-AUTH-03, R-AUTH-07). Rejects with the error of the login request; the
             * sign-in form shows it. The user's settings follow the new session (withSettings).
             */
            async startSession(email: string, password: string): Promise<void> {
                await state.signIn(email, password);
                void state.loadProposals();
            },

            /**
             * Signs out (R-AUTH-08): the server deletes the cookie and issues a new antiforgery token,
             * and every in-memory state of the user goes — the configurator draft and list too, the
             * legacy proposals and favourites; appearance, motion and contrast follow the system
             * again (withSettings).
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
                state.resetConfigurator();
                state.setProposals([]);
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
            void store.init().finally(() => console.log("store initialized"));
        }
    }))
);
