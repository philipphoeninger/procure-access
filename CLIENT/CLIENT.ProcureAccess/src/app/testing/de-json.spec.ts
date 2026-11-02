// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

import de from '../../../public/i18n/de.json';
import { provideTestTranslations } from './provide-test-translations';

// scripts/check-i18n.mjs checks de.json with its own copy of the ngx-translate lookup. This test
// runs the real library over every text, so a library update that changes the lookup of a dotted
// property name next to a text (#101: "menu": "Menü", "menu.close": "Menü schließen") fails here.

type Texts = { [name: string]: string | Texts };

function flatten(texts: Texts, prefix = ''): [string, string][] {
  return Object.entries(texts).flatMap(([name, value]) => {
    const key = prefix ? `${prefix}.${name}` : name;
    return typeof value === 'string' ? [[key, value] as [string, string]] : flatten(value, key);
  });
}

describe('de.json', () => {
  it('gives every text under its own key with the real ngx-translate lookup', async () => {
    TestBed.configureTestingModule({ providers: [provideTestTranslations()] });
    const translate = TestBed.inject(TranslateService);
    await firstValueFrom(translate.use('de'));

    const texts = flatten(de);
    const wrong = texts.filter(([key, text]) => translate.instant(key) !== text);

    expect(texts.length).toBeGreaterThan(200);
    expect(wrong).toEqual([]);
  });

  it('keeps a text and the longer key that starts with it side by side (#101)', async () => {
    TestBed.configureTestingModule({ providers: [provideTestTranslations()] });
    const translate = TestBed.inject(TranslateService);
    await firstValueFrom(translate.use('de'));

    expect(translate.instant('shell.nav.menu')).toBe('Menü');
    expect(translate.instant('shell.nav.menu.close')).toBe('Menü schließen');
    expect(translate.instant('shell.pageTitle', { page: 'Kriterien' })).toBe('Kriterien – ProcureAccess');
    expect(translate.instant('shell.pageTitle.error', { page: 'Kriterien' })).toBe('Fehler: Kriterien – ProcureAccess');
  });
});
