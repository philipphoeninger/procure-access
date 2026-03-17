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

export type SettingsState = { 
    uiCustomization: UICustomization
};

export function withSettings() {
  return signalStoreFeature(
    withState<SettingsState>({ uiCustomization: initialAppState.settings.uiCustomization }),
    withIdentity(),
    withLoading(),
    withMethods((
      state,
      settingsApiService = inject(SettingsApiService),
      snackbarService = inject(SnackbarService)
    ) => ({
      setUICustomization(uiCustomization: UICustomization | null) {
        if (uiCustomization)
          patchState(state, {
            uiCustomization
          });
      },
      async updateUICustomization(uiCustomization: UICustomization) {
        state.incrementLoadingCount();
        let success = await settingsApiService.updateUICustomization(uiCustomization);

        if (success) {
          this.setUICustomization(uiCustomization);
          snackbarService.showInfo(
            state.isAuthenticated() ? "Settings saved" : "Settings saved as cookie"
          );
        }
        else {
          snackbarService.showError('Error occured on saving settings. Please try again later.');
        }
        state.decrementLoadingCount();
      }
    })),
    withComputed((state) => ({ }))
  );
}
