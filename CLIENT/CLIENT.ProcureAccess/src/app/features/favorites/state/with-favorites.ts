// SPDX-FileCopyrightText: 2025-2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { computed } from '@angular/core';
import {
  patchState,
  signalStoreFeature,
  type,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';
import { User } from '@app/features/identity/models/user.model';
import { ProductSave } from '../models/productSave.model';
import { FilterSet } from '../models/filterSet.model';

export type FavoritesState = { productSaves: ProductSave[], filterSets: FilterSet[] };

/** Legacy favourites, in memory only (reworked into `withWorkspace()`, ARCHITECTURE §8.1). Needs `withIdentity()` before it in the store. */
export const withFavorites = () => signalStoreFeature(
    { state: type<{ user: User | null }>() },
    withState<FavoritesState>({ productSaves: [], filterSets: [] }),
    withMethods((state) => {

        return {
            addProductFavorite(productId: string) {
                // TODO: make api call
                const highestId = state.productSaves().length - 1;
                const productSave: ProductSave = {
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
                const updateProductSaves = state.productSaves();
                const productSaveIndex = 
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
