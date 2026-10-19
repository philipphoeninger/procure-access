import { Criterion } from "@app/features/criteria/models/criterion.model";
import { IdentityState } from "@app/features/identity/state/with-identity";
import { ProductSave } from "@app/features/favorites/models/productSave.model";
import { FilterSet } from "@app/features/favorites/models/filterSet.model";
import { Product } from "@app/features/products/models/product.model";
import { FiltersState } from "@app/features/filters/state/with-filters";
import { initialSettingsState, SettingsState } from "@app/features/settings/state/with-settings";
import { ProposalDto } from "@app/features/proposal/models/proposal.dto";

export interface AppState {
    loadingCount: number;
    settings: SettingsState;
    identity: IdentityState;
    filters: FiltersState;
    criteria: Criterion[];
    productSaves: ProductSave[];
    filterSets: FilterSet[];
    products: Product[];
    proposals: ProposalDto[];
}

export const initialAppState: AppState = {
    loadingCount: 0,
    settings: initialSettingsState,
    identity: {
        user: null
    },
    filters: {
        filterTypes: [],
        criteriaFilters: [],
        selectedCriteriaFilters: []
    },
    criteria: [],
    productSaves: [],
    filterSets: [],
    products: [],
    proposals: []
}
