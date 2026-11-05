// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { signalStore } from '@ngrx/signals';

import { CriteriaApiService } from '../services/api/criteria-api.service';
import { withCriteria } from './with-criteria';

// #168: a failed load must not leave the loading bar on (pa-loading-bar, R-SHL-09).
const CriteriaTestStore = signalStore(withCriteria());

function setUp(apiService: Partial<CriteriaApiService>) {
  TestBed.configureTestingModule({
    providers: [CriteriaTestStore, { provide: CriteriaApiService, useValue: apiService }],
  });
  return TestBed.inject(CriteriaTestStore);
}

// The store does not look at the reason; HttpClient would reject with an HttpErrorResponse.
const failure = new Error('request failed');

describe('withCriteria', () => {
  it('stops loading when the criteria request fails', async () => {
    const store = setUp({ getCriteriaByCriteriaFilterIds: () => Promise.reject(failure) });

    await expect(store.getCriteriaBySelectedCriteriaFilterIds([1])).rejects.toBe(failure);

    expect(store.isLoading()).toBe(false);
  });
});
