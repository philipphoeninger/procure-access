// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { computed, effect, inject, untracked } from '@angular/core';
import { patchState, signalStoreFeature, type, withHooks, withMethods, withState } from '@ngrx/signals';

import { DEFAULT_LANGUAGE } from '@app/app.config';
import { AppearanceService } from '@app/core/a11y/appearance';
import { SessionUser } from '@app/features/identity/models/session';
import { UserPreferences } from '../models/user-preferences';
import { PreferencesApiService } from '../services/api/preferences-api.service';
import { LanguageService } from '../services/language.service';

export interface SettingsState {
  /** The signed-in user's settings; null without a session or until they are read. */
  preferences: UserPreferences | null;
}

/**
 * The signed-in user's settings (SPEC R-SET-01, R-SET-02, §8.12; A11Y-06; S5-25b). Follows the
 * session of `withIdentity()`: when another user signs in, their settings are read and applied —
 * an explicit appearance, reduced motion or high contrast overrides the system's —; without a
 * session everything follows the system again. Nothing is kept in browser storage, so it works with
 * storage blocked (TD-25). S60 changes the settings (S5-26).
 *
 * Needs `withIdentity()` before it in the store.
 */
export const withSettings = () =>
  signalStoreFeature(
    { state: type<{ session: SessionUser | null }>() },
    withState<SettingsState>({ preferences: null }),
    withMethods((store, api = inject(PreferencesApiService), appearance = inject(AppearanceService)) => {
      /** The user whose settings were asked for last; an answer for anyone else is dropped. */
      let requestedFor: string | null = null;

      function apply(preferences: UserPreferences | null): void {
        patchState(store, { preferences });
        const chosen = preferences?.appearance;
        appearance.setExplicit(chosen === 'light' || chosen === 'dark' ? chosen : null);
        appearance.setExplicitReducedMotion(preferences?.motion === 'reduced');
        appearance.setExplicitHighContrast(preferences?.contrast === 'high');
      }

      return {
        /**
         * Reads and applies the settings of `userId`, or follows the system for `null`. A failure
         * is a background failure (nothing shown): the page follows the system.
         */
        async loadPreferences(userId: string | null): Promise<void> {
          requestedFor = userId;
          if (userId === null) {
            apply(null);
            return;
          }

          let preferences: UserPreferences | null;
          try {
            preferences = await api.get();
          } catch {
            preferences = null;
          }
          if (requestedFor === userId) apply(preferences);
        },
      };
    }),
    withHooks({
      onInit(store) {
        // German is the only UI language of v1 (SPEC R-SET-01, SD12); <html lang> follows it (A11Y-05).
        inject(LanguageService).init(DEFAULT_LANGUAGE);

        const userId = computed(() => store.session()?.id ?? null);
        effect(() => {
          const id = userId();
          untracked(() => void store.loadPreferences(id));
        });
      },
    }),
  );
