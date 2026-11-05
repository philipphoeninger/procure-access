// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { MediaMatcher } from '@angular/cdk/layout';
import { DOCUMENT } from '@angular/common';
import {
  computed,
  DestroyRef,
  effect,
  EnvironmentProviders,
  inject,
  Injectable,
  makeEnvironmentProviders,
  provideEnvironmentInitializer,
  signal,
} from '@angular/core';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';

export type Appearance = 'light' | 'dark';

/**
 * Light or dark appearance (SPEC §7.5, R-SET-02, Handoff §1.1). Sets the class `theme-light` or
 * `theme-dark` on `<html>`, which `styles/theme.scss` turns into `color-scheme`. Without an
 * explicit choice the class follows `prefers-color-scheme`, also when it changes while the page
 * is open. Nothing is read from or written to browser storage, so it works with storage blocked.
 *
 * `setExplicit()` is for an explicit user choice; until the settings page S60 (step 3) only the
 * legacy settings store calls it.
 *
 * @spec docs/SPEC_v2.md §7.5, R-SET-02
 */
@Injectable({ providedIn: 'root' })
export class AppearanceService {
  private readonly systemDark = signal(false);
  private readonly explicit = signal<Appearance | null>(null);

  /** The active appearance: the explicit choice, otherwise the system setting. */
  readonly appearance = computed<Appearance>(
    () => this.explicit() ?? (this.systemDark() ? 'dark' : 'light'),
  );

  constructor() {
    const query = inject(MediaMatcher).matchMedia('(prefers-color-scheme: dark)');
    this.systemDark.set(query.matches);
    const onChange = (event: MediaQueryListEvent) => this.systemDark.set(event.matches);
    query.addEventListener('change', onChange);
    inject(DestroyRef).onDestroy(() => query.removeEventListener('change', onChange));

    const html = inject(DOCUMENT).documentElement;
    effect(() => {
      const appearance = this.appearance();
      html.classList.toggle('theme-light', appearance === 'light');
      html.classList.toggle('theme-dark', appearance === 'dark');
    });
  }

  /** An explicit choice overrides the system setting; `null` follows the system again. */
  setExplicit(appearance: Appearance | null): void {
    this.explicit.set(appearance);
  }
}

/**
 * The system preferences for appearance and motion (R-SET-02): starts `AppearanceService`, and
 * turns Angular Material's animations off when `prefers-reduced-motion: reduce` is set
 * (Handoff §1.7). Material reads that setting once, so a change applies after a reload.
 */
export function provideAppearance(): EnvironmentProviders {
  return makeEnvironmentProviders([
    {
      provide: MATERIAL_ANIMATIONS,
      useFactory: () => ({
        animationsDisabled: inject(MediaMatcher).matchMedia('(prefers-reduced-motion: reduce)').matches,
      }),
    },
    provideEnvironmentInitializer(() => inject(AppearanceService)),
  ]);
}
