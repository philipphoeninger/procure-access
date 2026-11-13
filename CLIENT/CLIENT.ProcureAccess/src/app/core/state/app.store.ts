// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { signalStore, withHooks, withMethods, withState } from "@ngrx/signals";
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
            async init() {
                // load necessary data:
                // the UI texts and the anonymous settings first (nobody is signed in before the probe):
                // they come from the browser, so no text waits for an API answer
                await state.reloadUICustomization();

                // the session probe (ARCHITECTURE §8.3): who is signed in, and the antiforgery cookie;
                // a signed-in user's saved settings replace the anonymous ones
                const user = await state.sessionReady().then(() => state.user());
                if (user != null) {
                    state.setUICustomization(user.uiCustomization);
                    state.setLanguage(user.uiCustomization.language);
                    state.removeSettingsFromLocalStorage();
                }
                state.activateMediaEventListeners();

                // products
                await state.loadProducts();
            }
        };
    }),
    withHooks((store) => ({
        onInit() {
            store.init().finally(() => console.log("store initialized"));
        }
    }))
);
