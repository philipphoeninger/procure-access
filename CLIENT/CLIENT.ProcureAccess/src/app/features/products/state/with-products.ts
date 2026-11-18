// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { computed, inject } from '@angular/core';
import {
  patchState,
  signalStoreFeature,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';
import { Product } from '../models/product.model';
import { ProductsApiService } from '../services/api/products-api.service';
import { LOADING_INPUT } from '@app/shared/state/with-loading';

export type ProductState = { products: Product[], productsLoaded: boolean };

export const withProducts = () => signalStoreFeature(
    LOADING_INPUT,
    withState<ProductState>({ products: [], productsLoaded: false }),
    withMethods((state, productsApiService = inject(ProductsApiService)) => {
      // Only the newest product request may write its result, so a slow full list
      // cannot replace a filtered list the user asked for later.
      let latestRequest = 0;

      return {
        // The loading count goes down in `finally`, so a failed request does not leave the
        // spinner on (S1-08 N1). The error still reaches the caller.
        async loadProducts() {
          const request = ++latestRequest;
          state.incrementLoadingCount();
          try {
            const result = await productsApiService.getAllProducts();
            if (request === latestRequest) this.setProducts(result.value ?? []);
          } finally {
            state.decrementLoadingCount();
          }
        },
        setProducts(products: Product[]) {
          patchState(state, {
            products,
            productsLoaded: true
          });
        },
        getProductById(productId: number) {
          return state.products().find(product => product.id === productId) ?? null;
        },
        async getProductsBySelectedCriteriaFilterIds(selectedCriteriaFilterIds: number[]) {
          const request = ++latestRequest;
          state.incrementLoadingCount();
          try {
            const products = await productsApiService.getProductsByCriteriaFilterIds(selectedCriteriaFilterIds);
            if (request === latestRequest) this.setProducts(products.value ?? []);
          } finally {
            state.decrementLoadingCount();
          }
        },
      };
    }),
    withComputed((state) => ({
      productsCount: computed(() => {
        return state.products().length;
      })
    }))
);
