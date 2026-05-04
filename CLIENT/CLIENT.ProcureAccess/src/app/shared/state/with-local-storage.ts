import { isPlatformBrowser } from '@angular/common';
import { inject, isSignal, PLATFORM_ID } from '@angular/core';
import { APP_NAME } from '@app/app.config';
import {
  patchState,
  signalStoreFeature,
  withMethods,
  withState
} from '@ngrx/signals';

export function withLocalStorage(storageKeys: string[]) {
  return signalStoreFeature(
    withState({}),
    withMethods((state) => {
      const isBrowser = isPlatformBrowser(inject(PLATFORM_ID));
      const appName = inject(APP_NAME);

      return {
        saveAllToLocalStorage() {
          const stateValue: Record<string, unknown> = {};
          for (const key of storageKeys) {
            // get from state
            const sliceSignal = (<Record<string, unknown>>state)[key];
            if (isSignal(sliceSignal)) stateValue[key] = sliceSignal();
            if (!stateValue) continue;
            // update local storage
            window.localStorage.setItem(
              appName.toLowerCase() + '-' + key,
              JSON.stringify(stateValue[key])
            );
          }
        },
        saveToLocalStorage(stateKey: string) {
          if (!isBrowser) return false; // gate

          // get from state
          const sliceSignal = (<Record<string, unknown>>state)[stateKey];
          let stateValue: any = null;
          if (isSignal(sliceSignal)) stateValue = sliceSignal();
          if (!stateValue) return false; //gate

          // update local storage
          window.localStorage.setItem(
            appName.toLowerCase() + '-' + stateKey, JSON.stringify(stateValue)
          );
          return true;
        },
        loadFromLocalStorage(storageKey: string) {
          if (!isBrowser) return false; // gate

          const storageValue = window.localStorage.getItem(
            appName.toLowerCase() + '-' + storageKey
          );
          if (!storageValue) return false; //gate

          let updatedState: Record<string, unknown> = {};
          updatedState[storageKey] = JSON.parse(storageValue);

          patchState(state, updatedState);
          return true;
        },
        removeFromLocalStorage(storageKey: string) {
          if (!isBrowser) return false; // gate
          
          // update local storage
          window.localStorage.removeItem(
            appName.toLowerCase() + '-' + storageKey
          );
          return true;
        }
    }})
  );
}
