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

export type SettingsState = { 
    uiCustomization: UICustomization
};

export const initialSettingsState: SettingsState = {
    uiCustomization: {
        foregroundColor: "#111111",
        backgroundColor: "#dbdedf",
        textColor: "#111111",
        darkModeOn: false,
        orientationVertical: true,
        highContrastOn: false
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
      rendererFactory = inject(RendererFactory2)
    ) => {
      let renderer = rendererFactory.createRenderer(null, null);

      return {
        activateMediaEventListeners() {
          // theme mode
          window
            .matchMedia('(prefers-color-scheme: dark)')
            .addEventListener('change', event => {
              if (state.isAuthenticated()) {
                let uiCustomization = state.uiCustomization();
                uiCustomization.darkModeOn = event.matches;
                this.setUICustomization(uiCustomization);
                snackbarService.showInfo(
                  'You changed the preferred color scheme in your browser.\n' +
                  'If you want to change the setting permanently, you must save it in the Settings page.');
              } else {
                // drops unsaved setting changes
                state.loadFromLocalStorage(LocalStorageKeys.uiCustomization);
                let uiCustomization = state.uiCustomization();
                uiCustomization.darkModeOn = event.matches;
                this.setUICustomization(uiCustomization);
                state.saveToLocalStorage(LocalStorageKeys.uiCustomization);
                snackbarService.showInfo('New color scheme saved as cookie.');
              }
            });
          // contrast mode
          window
            .matchMedia('(prefers-contrast: more)')
            .addEventListener('change', event => {
              if (state.isAuthenticated()) {
                let uiCustomization = state.uiCustomization();
                uiCustomization.highContrastOn = event.matches;
                this.setUICustomization(uiCustomization);
                snackbarService.showInfo(
                  'You changed the preferred contrast mode in your browser.\n' +
                  'If you want to change the setting permanently, you must save it in the Settings page.');
              } else {
                // drops unsaved setting changes
                state.loadFromLocalStorage(LocalStorageKeys.uiCustomization);
                let uiCustomization = state.uiCustomization();
                uiCustomization.highContrastOn = event.matches;
                this.setUICustomization(uiCustomization);
                state.saveToLocalStorage(LocalStorageKeys.uiCustomization);
                snackbarService.showInfo('New contrast mode saved as cookie.');
              }
            });
        },
        async reloadUICustomization() {
          if (!state.isAuthenticated()) {
            if (state.loadFromLocalStorage(LocalStorageKeys.uiCustomization)) {
              // Renderer effects
              renderer.setStyle(
                document.documentElement, 
                'color-scheme', 
                state.uiCustomization().darkModeOn ? 'dark' : 'light');

              if (state.uiCustomization().highContrastOn)
                renderer.addClass(document.documentElement, 'contrast-more');
              else renderer.removeClass(document.documentElement, 'contrast-more');
              
              return; //gate
            }
            let initialSettings = initialAppState.settings.uiCustomization;
            initialSettings.darkModeOn =
                window.matchMedia('(prefers-color-scheme: dark)').matches;
            initialSettings.highContrastOn =
                window.matchMedia('(prefers-contrast: more)').matches;
            this.setUICustomization(initialSettings);
            state.saveToLocalStorage(LocalStorageKeys.uiCustomization);
          } else {
            state.incrementLoadingCount();
            this.removeSettingsFromLocalStorage();
            let uiCustomization = await settingsApiService.getUICustomization();
            this.setUICustomization(uiCustomization ?? initialAppState.settings.uiCustomization);
            state.decrementLoadingCount();
          }
        },
        removeSettingsFromLocalStorage() {
          state.removeFromLocalStorage(LocalStorageKeys.uiCustomization);
        },
        setUICustomization(uiCustomization: UICustomization) {
          patchState(state, {
            uiCustomization: { ...uiCustomization }
          });

          // Renderer effects
          renderer.setStyle(
            document.documentElement, 
            'color-scheme', 
            state.uiCustomization().darkModeOn ? 'dark' : 'light');
          
          if (state.uiCustomization().highContrastOn)
            renderer.addClass(document.documentElement, 'contrast-more');
          else renderer.removeClass(document.documentElement, 'contrast-more');
        },
        async updateUICustomization(uiCustomization: UICustomization) {
          if (!state.isAuthenticated()) {
            this.setUICustomization(uiCustomization);
            state.saveToLocalStorage(LocalStorageKeys.uiCustomization);
            snackbarService.showInfo('Settings saved as cookie');
            return; //gate
          }

          state.incrementLoadingCount();
          let success = await settingsApiService.updateUICustomization(uiCustomization);
          if (success) {
            this.setUICustomization(uiCustomization);
            snackbarService.showInfo('Settings saved permanently');
          } else {
            snackbarService.showError('Error occured on saving settings. Please try again later.');
          }
          state.decrementLoadingCount();
        },
        toggleDarkMode() {
          let uiCustomization = state.uiCustomization();
          uiCustomization.darkModeOn = !uiCustomization.darkModeOn;
          this.setUICustomization(uiCustomization);
        },
        toggleContrastMode() {
          let uiCustomization = state.uiCustomization();
          uiCustomization.highContrastOn = !uiCustomization.highContrastOn;
          this.setUICustomization(uiCustomization);
        }
     }
    }),
    withComputed((state) => ({
      darkModeOn: computed(() => state.uiCustomization().darkModeOn),
      highContrastOn: computed(() => state.uiCustomization().highContrastOn)
    }))
  );
