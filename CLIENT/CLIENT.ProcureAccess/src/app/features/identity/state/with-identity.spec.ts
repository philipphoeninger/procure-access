// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { signalStore } from '@ngrx/signals';

import { SnackbarService } from '@app/core/services/snackbar.service';
import { AuthService } from '../services/auth.service';
import { IdentityApiService } from '../services/identity-api.service';
import { withIdentity } from './with-identity';

// #168: a failed request must not leave the loading bar on (pa-loading-bar, R-SHL-09).
const IdentityTestStore = signalStore(withIdentity());

function setUp(apiService: Partial<IdentityApiService>) {
  TestBed.configureTestingModule({
    providers: [
      IdentityTestStore,
      { provide: IdentityApiService, useValue: apiService },
      { provide: AuthService, useValue: { logout: vi.fn() } },
      { provide: SnackbarService, useValue: { showInfo: vi.fn() } },
    ],
  });
  return TestBed.inject(IdentityTestStore);
}

// The store does not look at the reason; HttpClient would reject with an HttpErrorResponse.
const failure = new Error('request failed');

describe('withIdentity', () => {
  it('stops loading when deleting the account fails', async () => {
    const store = setUp({ deleteAccount: () => Promise.reject(failure) });

    await expect(store.deleteAccount()).rejects.toBe(failure);

    expect(store.isLoading()).toBe(false);
  });
});
