// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { computed, inject, RendererFactory2 } from '@angular/core';
import {
  patchState,
  signalStoreFeature,
  withComputed,
  withMethods,
  withState
} from '@ngrx/signals';
import { UICustomization } from '../models/uiCustomization.model';
import { initialAppState } from '@app/core/models/appState.interface';
import { withLoading } from '@app/shared/state/with-loading';
import { SettingsApiService } from '../services/settings-api.service';
import { SnackbarService } from '@app/core/services/snackbar.service';
import { withIdentity } from '@app/features/identity/state/with-identity';
import { withLocalStorage } from '@app/shared/state/with-local-storage';
import { EnLanguage } from '../../../core/models/language.enum';
import { LanguageService } from '@app/features/settings/services/language.service';
import { DEFAULT_LANGUAGE } from '@app/app.config';
import { TranslateService } from '@ngx-translate/core';
import { tap } from 'rxjs';
import { AppearanceService } from '@app/core/a11y/appearance';

export type SettingsState = { 
    uiCustomization: UICustomization
};

export const initialSettingsState: SettingsState = {
    uiCustomization: {
        foregroundColor: "#111111",
        backgroundColor: "#dbdedf",
        textColor: "#111111",
        darkModeOn: false,
        highContrastOn: false,
        language: DEFAULT_LANGUAGE
    }
}

enum LocalStorageKeys {
    uiCustomization = 'uiCustomization'
}

export const withSettings = () => signalStoreFeature(
    withState<SettingsState>({ uiCustomization: initialAppState.settings.uiCustomization }),
    withIdentity(),
    withLocalStorage(Object.values(LocalStorageKeys)),
    withLoading(),
    withMethods((
      state,
      settingsApiService = inject(SettingsApiService),
      snackbarService = inject(SnackbarService),
      languageService = inject(LanguageService),
      translateService = inject(TranslateService),
      rendererFactory = inject(RendererFactory2),
      appearance = inject(AppearanceService)
    ) => {
      const renderer = rendererFactory.createRenderer(null, null);

      // The appearance follows the system (AppearanceService, R-SET-02). Only an explicit choice
      // overrides it: the dark-mode toggle and a signed-in user's saved setting. A value from
      // localStorage is not applied, because the store also saves the system setting there
      // (S3-09, owner decision D3). Interim until the settings page S60 (step 3).
      const followSystemAppearance = () => {
        appearance.setExplicit(null);
        patchState(state, {
          uiCustomization: { ...state.uiCustomization(), darkModeOn: appearance.appearance() === 'dark' }
        });
      };

      return {
        activateMediaEventListeners() {
          // theme mode; no snackbar: a system change is not a choice to confirm, and the open
          // snackbar covered the header (S3-10). The storage writes stay until S60 (step 3).
          window
            .matchMedia('(prefers-color-scheme: dark)')
            .addEventListener('change', event => {
              if (state.signedIn()) {
                const uiCustomization = state.uiCustomization();
                uiCustomization.darkModeOn = event.matches;
                this.setUICustomization(uiCustomization);
              } else {
                // drops unsaved setting changes
                state.loadFromLocalStorage(LocalStorageKeys.uiCustomization);
                const uiCustomization = state.uiCustomization();
                uiCustomization.darkModeOn = event.matches;
                this.setUICustomization(uiCustomization);
                appearance.setExplicit(null);
                state.saveToLocalStorage(LocalStorageKeys.uiCustomization);
              }
            });
          // contrast mode
          window
            .matchMedia('(prefers-contrast: more)')
            .addEventListener('change', event => {
              if (state.signedIn()) {
                const uiCustomization = state.uiCustomization();
                uiCustomization.highContrastOn = event.matches;
                this.setUICustomization(uiCustomization);
                snackbarService.showInfo(
                  'You changed the preferred contrast mode in your browser.\n' +
                  'If you want to change the setting permanently, you must save it in the Settings page.');
              } else {
                // drops unsaved setting changes
                state.loadFromLocalStorage(LocalStorageKeys.uiCustomization);
                const uiCustomization = state.uiCustomization();
                uiCustomization.highContrastOn = event.matches;
                this.setUICustomization(uiCustomization);
                state.saveToLocalStorage(LocalStorageKeys.uiCustomization);
                snackbarService.showInfo('New contrast mode saved as cookie.');
              }
            });
        },
        async reloadUICustomization() {
          if (!state.signedIn()) {
            if (state.loadFromLocalStorage(LocalStorageKeys.uiCustomization)) {
              followSystemAppearance();

              if (state.uiCustomization().highContrastOn)
                renderer.addClass(document.documentElement, 'contrast-more');
              else renderer.removeClass(document.documentElement, 'contrast-more');

              // setup language service (<html lang> follows it: provideDocumentLanguage, A11Y-05)
              languageService.init(state.uiCustomization().language);

              return; //gate
            }
            const initialSettings = initialAppState.settings.uiCustomization;
            initialSettings.darkModeOn =
                window.matchMedia('(prefers-color-scheme: dark)').matches;
            initialSettings.highContrastOn =
                window.matchMedia('(prefers-contrast: more)').matches;
            // set language
            languageService.init(DEFAULT_LANGUAGE);
            initialSettings.language = languageService.get();
            this.setUICustomization(initialSettings);
            followSystemAppearance();
            state.saveToLocalStorage(LocalStorageKeys.uiCustomization);
          } else {
            state.incrementLoadingCount();
            try {
              this.removeSettingsFromLocalStorage();
              const uiCustomization = await settingsApiService.getUICustomization();
              languageService.init(uiCustomization.language);
              this.setUICustomization(uiCustomization ?? initialAppState.settings.uiCustomization);
            } finally {
              state.decrementLoadingCount();
            }
          }
        },
        removeSettingsFromLocalStorage() {
          state.removeFromLocalStorage(LocalStorageKeys.uiCustomization);
        },
        setUICustomization(uiCustomization: UICustomization) {
          patchState(state, {
            uiCustomization: { ...uiCustomization }
          });

          // A signed-in user's saved setting overrides the system appearance (R-SET-02)
          if (state.signedIn())
            appearance.setExplicit(state.uiCustomization().darkModeOn ? 'dark' : 'light');

          if (state.uiCustomization().highContrastOn)
            renderer.addClass(document.documentElement, 'contrast-more');
          else renderer.removeClass(document.documentElement, 'contrast-more');
        },
        async updateUICustomization(uiCustomization: UICustomization) {
          if (!state.signedIn()) {
            this.setUICustomization(uiCustomization);
            state.saveToLocalStorage(LocalStorageKeys.uiCustomization);
            translateService
              .get('SETTINGS.SAVED_AS_COOKIE')
              .pipe(tap(message => snackbarService.showInfo(message)))
              .subscribe();
            return; //gate
          }

          state.incrementLoadingCount();
          try {
            const success = await settingsApiService.updateUICustomization(uiCustomization);
            if (success) {
              this.setUICustomization(uiCustomization);
              translateService
                .get('SETTINGS.SAVED_PERMANENTLY')
                .pipe(tap(message => snackbarService.showInfo(message)))
                .subscribe();
            }
          } finally {
            state.decrementLoadingCount();
          }
        },
        toggleDarkMode() {
          const uiCustomization = state.uiCustomization();
          uiCustomization.darkModeOn = !uiCustomization.darkModeOn;
          this.setUICustomization(uiCustomization);
          appearance.setExplicit(uiCustomization.darkModeOn ? 'dark' : 'light');
        },
        toggleContrastMode() {
          const uiCustomization = state.uiCustomization();
          uiCustomization.highContrastOn = !uiCustomization.highContrastOn;
          this.setUICustomization(uiCustomization);
        },
        setLanguage(lang: EnLanguage) {
          const uiCustomization = state.uiCustomization();
          uiCustomization.language = lang;
          languageService.set(uiCustomization.language);
          this.setUICustomization(uiCustomization);
        }
      }
    }),
    withComputed((state) => ({
      darkModeOn: computed(() => state.uiCustomization().darkModeOn),
      highContrastOn: computed(() => state.uiCustomization().highContrastOn),
      language: computed(() => state.uiCustomization().language)
    }))
  );
