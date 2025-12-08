import { signalStore, withMethods, withState } from "@ngrx/signals";
import { AppState, initialAppState } from "../models/appState.interface";
import { withLoading } from "@app/shared/state/with-loading";
import { withIdentity } from "@app/features/identity/state/with-identity";
import { withFilters } from "@app/features/filters/state/with-filters";
import { withCriteria } from "@app/features/criteria/state/with-criteria";

export const ProcureAccessStore = signalStore(
    { providedIn: 'root' },
    withState<AppState>(initialAppState),
    withLoading(),
    withIdentity(),
    withFilters(),
    withCriteria(),
    withMethods((state) => {

        return {};
    })
);