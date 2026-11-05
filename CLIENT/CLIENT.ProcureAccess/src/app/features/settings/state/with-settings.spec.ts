// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { signalStore } from '@ngrx/signals';

import { APP_NAME } from '@app/app.config';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { AuthService } from '@app/features/identity/services/auth.service';
import { IdentityApiService } from '@app/features/identity/services/identity-api.service';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { LanguageService } from '../services/language.service';
import { SettingsApiService } from '../services/settings-api.service';
import { withSettings } from './with-settings';

// #168: a failed request of a signed-in user must not leave the loading bar on
// (pa-loading-bar, R-SHL-09). Signed-out users never call the API here.
const SettingsTestStore = signalStore(withSettings());

function setUp(apiService: Partial<SettingsApiService>) {
  TestBed.configureTestingModule({
    providers: [
      provideTestTranslations(),
      SettingsTestStore,
      { provide: APP_NAME, useValue: 'ProcureAccess' },
      { provide: SettingsApiService, useValue: apiService },
      { provide: AuthService, useValue: { isAuthenticated: () => true } },
      { provide: IdentityApiService, useValue: {} },
      { provide: LanguageService, useValue: { init: vi.fn() } },
      { provide: SnackbarService, useValue: { showInfo: vi.fn() } },
    ],
  });
  return TestBed.inject(SettingsTestStore);
}

// The store does not look at the reason; HttpClient would reject with an HttpErrorResponse.
const failure = new Error('request failed');

describe('withSettings', () => {
  it('stops loading when loading the settings of a signed-in user fails', async () => {
    const store = setUp({ getUICustomization: () => Promise.reject(failure) });

    await expect(store.reloadUICustomization()).rejects.toBe(failure);

    expect(store.isLoading()).toBe(false);
  });

  it('stops loading when saving the settings of a signed-in user fails', async () => {
    const store = setUp({ updateUICustomization: () => Promise.reject(failure) });

    await expect(store.updateUICustomization(store.uiCustomization())).rejects.toBe(failure);

    expect(store.isLoading()).toBe(false);
  });
});
