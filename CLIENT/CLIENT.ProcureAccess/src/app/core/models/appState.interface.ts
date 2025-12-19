import { Criterion } from "@app/features/criteria/models/criterion.model";
import { AppCustomization } from "./appCustomization.model";
import { IdentityState } from "@app/features/identity/state/with-identity";
import { ProductSave } from "@app/features/favorites/models/productSave.model";
import { FilterSet } from "@app/features/favorites/models/filterSet.model";

export interface AppState {
    loadingCount: number;
    appConfiguration: AppCustomization;
    identity: IdentityState;
    selectedFilters: number[];
    criteria: Criterion[];
    productSaves: ProductSave[];
    filterSets: FilterSet[];
}

export const initialAppState: AppState = {
    loadingCount: 0,
    appConfiguration: {
        foregroundColor: '',
        backgroundColor: '',
        textColor: '',
        orientation: '',
        highContrastEnabled: false
    },
    identity: {
        user: null
    },
    selectedFilters: [],
    criteria: [],
    productSaves: [],
    filterSets: []
}
