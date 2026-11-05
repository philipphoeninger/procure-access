// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { signalStore } from '@ngrx/signals';

import { APP_NAME } from '@app/app.config';
import { AppearanceService } from '@app/core/a11y/appearance';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { AuthService } from '@app/features/identity/services/auth.service';
import { IdentityApiService } from '@app/features/identity/services/identity-api.service';
import { DARK_SCHEME, FakeMediaMatcher, provideFakeMediaMatcher } from '@app/testing/fake-media-matcher';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { LanguageService } from '../services/language.service';
import { SettingsApiService } from '../services/settings-api.service';
import { initialSettingsState, withSettings } from './with-settings';

// #168: a failed request of a signed-in user must not leave the loading bar on
// (pa-loading-bar, R-SHL-09). Signed-out users never call the API here.
const SettingsTestStore = signalStore(withSettings());

function setUp(
  apiService: Partial<SettingsApiService>,
  { signedIn = true, systemDark = false }: { signedIn?: boolean; systemDark?: boolean } = {},
) {
  TestBed.configureTestingModule({
    providers: [
      provideTestTranslations(),
      provideFakeMediaMatcher(new FakeMediaMatcher({ [DARK_SCHEME]: systemDark })),
      SettingsTestStore,
      { provide: APP_NAME, useValue: 'ProcureAccess' },
      { provide: SettingsApiService, useValue: apiService },
      { provide: AuthService, useValue: { isAuthenticated: () => signedIn } },
      { provide: IdentityApiService, useValue: {} },
      { provide: LanguageService, useValue: { init: vi.fn(), get: () => 'de' } },
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

  // S3-09, owner decision D3: only an explicit choice overrides the system appearance (R-SET-02).
  describe('appearance', () => {
    const storageKey = 'procureaccess-uiCustomization';
    const appearance = () => TestBed.inject(AppearanceService).appearance();

    afterEach(() => localStorage.removeItem(storageKey));

    it('follows the system after sign-out and ignores a stored dark mode value', async () => {
      const stored = { ...initialSettingsState.uiCustomization, darkModeOn: true };
      localStorage.setItem(storageKey, JSON.stringify(stored));
      const store = setUp({}, { signedIn: false, systemDark: false });
      TestBed.inject(AppearanceService).setExplicit('dark'); // the signed-in user's setting

      await store.reloadUICustomization();

      expect(appearance()).toBe('light');
      expect(store.darkModeOn()).toBe(false);
    });

    it('applies the dark mode toggle of a signed-out user', async () => {
      localStorage.setItem(storageKey, JSON.stringify(initialSettingsState.uiCustomization));
      const store = setUp({}, { signedIn: false, systemDark: false });
      await store.reloadUICustomization();

      store.toggleDarkMode();

      expect(appearance()).toBe('dark');
    });

    it('applies the saved setting of a signed-in user over the system', async () => {
      const saved = { ...initialSettingsState.uiCustomization, darkModeOn: true };
      const store = setUp({ getUICustomization: () => Promise.resolve(saved) }, { systemDark: false });

      await store.reloadUICustomization();

      expect(appearance()).toBe('dark');
    });
  });
});
