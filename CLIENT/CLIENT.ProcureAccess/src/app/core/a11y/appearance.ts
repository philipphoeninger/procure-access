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
  WritableSignal,
} from '@angular/core';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';

export type Appearance = 'light' | 'dark';

/**
 * Appearance, motion and contrast on `<html>` (SPEC §7.5, R-SET-01, R-SET-02; Handoff §1.1, §1.7).
 * Each follows the system setting, also when it changes while the page is open, unless a
 * signed-in user's explicit choice overrides it (`withSettings`, S5-25b). Nothing is read from or
 * written to browser storage, so it works with storage blocked.
 *
 * - Appearance: the class `theme-light` or `theme-dark`, which `styles/theme.scss` turns into
 *   `color-scheme`; the system's is `prefers-color-scheme`.
 * - Motion: the class `motion-reduced` while the system (`prefers-reduced-motion: reduce`) or the
 *   explicit choice asks for it; `styles/theme.scss` shortens every CSS transition, and
 *   {@link provideAppearance} turns off the animations of every Material component created from
 *   then on.
 * - Contrast: the class `contrast-more` while the system asks for more contrast
 *   (`prefers-contrast: more`) or the explicit choice is high contrast; its colours are still the
 *   legacy ones until the high-contrast design H2 (owner decision S5-25 3 (i)).
 *
 * @spec docs/SPEC_v2.md §7.5, R-SET-01, R-SET-02
 */
@Injectable({ providedIn: 'root' })
export class AppearanceService {
  private readonly systemDark = signal(false);
  private readonly systemReducedMotion = signal(false);
  private readonly systemMoreContrast = signal(false);
  private readonly explicit = signal<Appearance | null>(null);
  private readonly explicitReducedMotion = signal(false);
  private readonly explicitHighContrast = signal(false);

  /** The active appearance: the explicit choice, otherwise the system setting. */
  readonly appearance = computed<Appearance>(
    () => this.explicit() ?? (this.systemDark() ? 'dark' : 'light'),
  );

  /** Motion is reduced: by the system or by the explicit choice. */
  readonly reducedMotion = computed(() => this.systemReducedMotion() || this.explicitReducedMotion());

  /** More contrast: asked for by the system or by the explicit choice. */
  readonly moreContrast = computed(() => this.systemMoreContrast() || this.explicitHighContrast());

  constructor() {
    const media = inject(MediaMatcher);
    const destroyRef = inject(DestroyRef);
    const follow = (query: string, target: WritableSignal<boolean>) => {
      const list = media.matchMedia(query);
      target.set(list.matches);
      const onChange = (event: MediaQueryListEvent) => target.set(event.matches);
      list.addEventListener('change', onChange);
      destroyRef.onDestroy(() => list.removeEventListener('change', onChange));
    };
    follow('(prefers-color-scheme: dark)', this.systemDark);
    follow('(prefers-reduced-motion: reduce)', this.systemReducedMotion);
    follow('(prefers-contrast: more)', this.systemMoreContrast);

    const html = inject(DOCUMENT).documentElement;
    effect(() => {
      const appearance = this.appearance();
      html.classList.toggle('theme-light', appearance === 'light');
      html.classList.toggle('theme-dark', appearance === 'dark');
    });
    effect(() => html.classList.toggle('motion-reduced', this.reducedMotion()));
    effect(() => html.classList.toggle('contrast-more', this.moreContrast()));
  }

  /** An explicit choice overrides the system setting; `null` follows the system again. */
  setExplicit(appearance: Appearance | null): void {
    this.explicit.set(appearance);
  }

  /** `true` reduces motion whatever the system says; `false` follows the system again. */
  setExplicitReducedMotion(reduced: boolean): void {
    this.explicitReducedMotion.set(reduced);
  }

  /** `true` asks for more contrast whatever the system says; `false` follows the system again. */
  setExplicitHighContrast(high: boolean): void {
    this.explicitHighContrast.set(high);
  }
}

/**
 * Starts `AppearanceService` (R-SET-02), and tells Angular Material whether animations are off
 * (Handoff §1.7). Material asks when it creates a component, so the answer is read through a
 * getter: a component created after the system setting or the user's choice changed follows it,
 * without a reload; components already shown keep their state, while `motion-reduced` shortens
 * their CSS transitions.
 */
export function provideAppearance(): EnvironmentProviders {
  return makeEnvironmentProviders([
    {
      provide: MATERIAL_ANIMATIONS,
      useFactory: () => {
        const appearance = inject(AppearanceService);
        return {
          get animationsDisabled(): boolean {
            return appearance.reducedMotion();
          },
        };
      },
    },
    provideEnvironmentInitializer(() => inject(AppearanceService)),
  ]);
}
