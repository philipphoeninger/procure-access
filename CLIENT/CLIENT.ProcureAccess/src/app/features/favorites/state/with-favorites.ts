import { computed, inject } from '@angular/core';
import { initialAppState } from '@app/core/models/appState.interface';
import {
  patchState,
  signalStoreFeature,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';
import { withLoading } from '@app/shared/state/with-loading';
// import { withFilters } from '@app/features/filters/state/with-filters';
import { ProductSave } from '../models/productSave.model';
import { FilterSet } from '../models/filterSet.model';
import { withIdentity } from '@app/features/identity/state/with-identity';

export type FavoritesState = { productSaves: ProductSave[], filterSets: FilterSet[] };

export function withFavorites() {
  return signalStoreFeature(
    withState<FavoritesState>({ 
        productSaves: initialAppState.productSaves,
        filterSets: initialAppState.filterSets
    }),
    withLoading(),
    withIdentity(),
    // withFilters(),
    withMethods((state) => {

        return {
            addProductFavorite(productId: string) {
                // TODO: make api call
                let highestId = state.productSaves().length - 1;
                let productSave: ProductSave = {
                    id: highestId + 1,
                    userId: state.user()?.email ?? '', // TODO
                    productId
                };

                patchState(state, {
                    productSaves: [
                        ...state.productSaves(),
                        productSave
                    ]
                });
            },
            removeProductFavorite(productSaveId: number) {
                // TODO: make api call
                let updateProductSaves = state.productSaves();
                let productSaveIndex = 
                    updateProductSaves.findIndex(
                        productSave => productSave.id === productSaveId);

                updateProductSaves.splice(productSaveIndex, 1);

                patchState(state, {
                    productSaves: updateProductSaves
                });
            },
            addFilterSetFavorite() {

            },
            removeFilterSetFavorite() {

            }
        };
    }),
    withComputed((state) => ({
      favoritesCount: computed(() => {
        return state.productSaves().length + state.filterSets().length;
      }),
      productFavoritesCount: computed(() => state.productSaves().length),
      filterSetFavoritesCount: computed(() => state.filterSets().length)
    }))
  );
}
