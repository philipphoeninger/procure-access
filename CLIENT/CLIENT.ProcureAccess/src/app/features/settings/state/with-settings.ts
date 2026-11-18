// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { computed, effect, inject, untracked } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { patchState, signalStoreFeature, type, withHooks, withMethods, withState } from '@ngrx/signals';

import { DEFAULT_LANGUAGE } from '@app/app.config';
import { AppearanceService } from '@app/core/a11y/appearance';
import { SessionUser } from '@app/features/identity/models/session';
import { UserPreferences } from '../models/user-preferences';
import { PreferencesApiService } from '../services/api/preferences-api.service';

export interface SettingsState {
  /** The signed-in user's settings; null without a session or until they are read. */
  preferences: UserPreferences | null;
}

/**
 * The signed-in user's settings (SPEC R-SET-01, R-SET-02, §8.12; A11Y-06; S5-25b). Follows the
 * session of `withIdentity()`: when another user signs in, their settings are read and applied —
 * an explicit appearance, reduced motion or high contrast overrides the system's —; without a
 * session everything follows the system again. Nothing is kept in browser storage, so it works with
 * storage blocked (TD-25). S60 reads them again when it opens and changes them (S5-26): a change
 * applies at once and is saved; saves run one after another, so the last change is the one stored.
 *
 * Needs `withIdentity()` before it in the store.
 */
export const withSettings = () =>
  signalStoreFeature(
    { state: type<{ session: SessionUser | null }>() },
    withState<SettingsState>({ preferences: null }),
    withMethods((store, api = inject(PreferencesApiService), appearance = inject(AppearanceService)) => {
      /**
       * Counts the reads, sign-outs and changes: only the newest one counts, so an answer for a user
       * who signed out meanwhile is dropped, a read never undoes a later change of S60, and a slower
       * read after the sign-in never replaces the one S60 made when it opened.
       */
      let latest = 0;
      /** The save in flight, or the last one; the next save starts after it. */
      let saving: Promise<unknown> = Promise.resolve();

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
          const mine = ++latest;
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
          if (mine === latest) apply(preferences);
        },

        /**
         * Reads the signed-in user's settings again and applies them, for S60 when it opens: the
         * page shows what is stored, also when the read after the sign-in failed or another
         * device changed them. Rejects when the read fails; the page shows that.
         */
        async readPreferences(): Promise<void> {
          const mine = ++latest;
          const preferences = await api.read();
          if (mine === latest) apply(preferences);
        },

        /**
         * Applies `next` at once (R-SET-01: without reload) and saves it after any save still in
         * flight. Resolves `true` when it was saved and nothing followed it — the moment to announce
         * `settings.saved` —, `false` when a later change will be saved after it. Rejects
         * when the save fails: the error interceptor shows that, and the choice stays applied for
         * this session (owner decision S5-26 D2 (a)); the next change saves all four again.
         */
        async changePreferences(next: UserPreferences): Promise<boolean> {
          const mine = ++latest;
          apply(next);
          const save = saving.catch(() => undefined).then(() => api.save(next));
          saving = save;
          await save;
          return mine === latest;
        },
      };
    }),
    withHooks({
      onInit(store) {
        // German is the only UI language of v1 (SPEC R-SET-01, SD12); <html lang> follows it (A11Y-05).
        const translate = inject(TranslateService);
        translate.setFallbackLang(DEFAULT_LANGUAGE);
        translate.use(DEFAULT_LANGUAGE);

        const userId = computed(() => store.session()?.id ?? null);
        effect(() => {
          const id = userId();
          untracked(() => void store.loadPreferences(id));
        });
      },
    }),
  );
