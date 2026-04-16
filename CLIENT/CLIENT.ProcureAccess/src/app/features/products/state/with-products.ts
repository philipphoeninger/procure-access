import { computed, inject } from '@angular/core';
import {
  patchState,
  signalStoreFeature,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';
import { Product } from '../models/product.model';
import { initialAppState } from '@app/core/models/appState.interface';
import { ProductsApiService } from '../services/api/products-api.service';
import { withLoading } from '@app/shared/state/with-loading';

export type ProductState = { products: Product[] };

export const withProducts = () => signalStoreFeature(
    withState<ProductState>({ products: initialAppState.products }),
    withLoading(),
    withMethods((state, productsApiService = inject(ProductsApiService)) => ({      
        async loadProducts() {
          state.incrementLoadingCount();
          let result = await productsApiService.getAllProducts();
          this.setProducts(result.value ?? []);
          state.decrementLoadingCount();
        },
        setProducts(products: Product[]) {
          patchState(state, {
            products
          });
        },
        getProductById(productId: number) {
          return state.products().find(product => product.id === productId) ?? null;
        }
    })),
    withComputed((state) => ({
      productsCount: computed(() => {
        return state.products().length;
      })
    }))
);
