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
    withMethods((state) => {
        return {
            async init() {
                // load necessary data:
                // set identity
                let user = await state.loadIdentity();

                // settings
                if (user != null) {
                    state.setUICustomization(user.uiCustomization);
                    state.setLanguage(user.uiCustomization.language);
                    state.removeSettingsFromLocalStorage();
                } else {
                    await state.reloadUICustomization();
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
