// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { HttpErrorResponse } from '@angular/common/http';
import { signalStore } from '@ngrx/signals';

import { TResult } from '@app/core/models/result.model';
import { Product } from '../models/product.model';
import { ProductsApiService } from '../services/api/products-api.service';
import { withProducts } from './with-products';

// Regression test for S1-08 N1 (with #37): a failed product request must not leave the
// loading spinner on. The slice is tested in a store of its own; only the API service is replaced.
const ProductsTestStore = signalStore(withProducts());

function setUp(apiService: Partial<ProductsApiService>) {
  TestBed.configureTestingModule({
    providers: [ProductsTestStore, { provide: ProductsApiService, useValue: apiService }],
  });
  return TestBed.inject(ProductsTestStore);
}

const serverError = new HttpErrorResponse({ status: 500 });

describe('withProducts', () => {
  it('stops loading when the filtered product request fails', async () => {
    const store = setUp({ getProductsByCriteriaFilterIds: () => Promise.reject(serverError) });

    await expect(store.getProductsBySelectedCriteriaFilterIds([1])).rejects.toBe(serverError);

    expect(store.isLoading()).toBe(false);
  });

  it('stops loading when the product list request fails', async () => {
    const store = setUp({ getAllProducts: () => Promise.reject(serverError) });

    await expect(store.loadProducts()).rejects.toBe(serverError);

    expect(store.isLoading()).toBe(false);
  });

  it('stores the filtered products and stops loading when the request succeeds', async () => {
    const products = [new Product(1, 'Beispielprodukt', 1)];
    const store = setUp({
      getProductsByCriteriaFilterIds: () => Promise.resolve(new TResult(true, undefined, products)),
    });

    await store.getProductsBySelectedCriteriaFilterIds([1]);

    expect(store.products()).toEqual(products);
    expect(store.isLoading()).toBe(false);
  });
});
