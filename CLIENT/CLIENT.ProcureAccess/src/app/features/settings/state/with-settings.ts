import { computed, inject } from '@angular/core';
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
      snackbarService = inject(SnackbarService)
    ) => ({
      async loadSettings() {
        let isAuthenticated = state.isAuthenticated();
        await this.loadUICustomization(isAuthenticated);
        // load more settings...
      },
      async loadUICustomization(isAuthenticated: boolean) {
        if (!isAuthenticated) {
          if (!state.loadFromLocalStorage(LocalStorageKeys.uiCustomization)) {
            this.setUICustomization(initialAppState.settings.uiCustomization);
          }
        } else if (!state.user()) { //gate
          this.setUICustomization(await settingsApiService.getUICustomization());
        }
      },
      setUICustomization(uiCustomization: UICustomization) {
          patchState(state, {
            uiCustomization
          });
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
        }
        else {
          snackbarService.showError('Error occured on saving settings. Please try again later.');
        }
        state.decrementLoadingCount();
      }
    })),
    withComputed((state) => ({ }))
);
