// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { DOCUMENT } from '@angular/common';
import { DestroyRef, EnvironmentProviders, inject, provideEnvironmentInitializer } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';

/**
 * `<html lang>` follows the active UI language of ngx-translate (A11Y-05, Handoff §7).
 * `index.html` starts with `lang="de"`, the default language, until the texts are loaded.
 *
 * @spec docs/design_handoff_procureaccess_v1.md §7 (Language)
 * @invariants A11Y-05
 */
export function provideDocumentLanguage(): EnvironmentProviders {
  return provideEnvironmentInitializer(() => {
    const translate = inject(TranslateService);
    const html = inject(DOCUMENT).documentElement;
    const current = translate.getCurrentLang();
    if (current) html.lang = current;
    const subscription = translate.onLangChange.subscribe(({ lang }) => (html.lang = lang));
    inject(DestroyRef).onDestroy(() => subscription.unsubscribe());
  });
}
