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

export function withProducts() {
  return signalStoreFeature(
    withState<ProductState>({ products: initialAppState.products }),
    withLoading(),
    withMethods((state, productsApiService = inject(ProductsApiService)) => ({      
        async loadProducts() {
          state.incrementLoadingCount();
          let products = await productsApiService.getAllProducts();
          this.setProducts(products);
          state.decrementLoadingCount();
        },
        setProducts(products: Product[]) {
          patchState(state, {
            products
          });
        },
        getProductById(productId: number) {
          let product = state.products().find(product => product.id === productId);
          return product;
        }
    })),
    withComputed((state) => ({
      productsCount: computed(() => {
        return state.products().length;
      })
    }))
  )
}
