// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { signalStore } from '@ngrx/signals';

import { TResult } from '@app/core/models/result.model';
import { FiltersApiService } from '../services/api/filters-api.service';
import { withFilters } from './with-filters';

// #168: a failed load must not leave the loading bar on (pa-loading-bar, R-SHL-09).
const FiltersTestStore = signalStore(withFilters());

function setUp(apiService: Partial<FiltersApiService>) {
  TestBed.configureTestingModule({
    providers: [FiltersTestStore, { provide: FiltersApiService, useValue: apiService }],
  });
  return TestBed.inject(FiltersTestStore);
}

// The store does not look at the reason; HttpClient would reject with an HttpErrorResponse.
const failure = new Error('request failed');

describe('withFilters', () => {
  it('stops loading when the filter request fails', async () => {
    const store = setUp({
      getAllFilterTypes: () => Promise.reject(failure),
      getAllCriteriaFilters: () => Promise.resolve(new TResult(true, undefined, [])),
    });

    await expect(store.loadFilters()).rejects.toBe(failure);

    expect(store.isLoading()).toBe(false);
  });
});
