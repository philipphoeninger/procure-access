// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { TestBed } from '@angular/core/testing';
import { TranslateService } from '@ngx-translate/core';
import { firstValueFrom } from 'rxjs';

import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { provideDocumentLanguage } from './document-language';

describe('provideDocumentLanguage', () => {
  beforeEach(() => {
    document.documentElement.lang = 'xx';
    TestBed.configureTestingModule({ providers: [provideTestTranslations(), provideDocumentLanguage()] });
  });

  afterEach(() => {
    document.documentElement.lang = '';
  });

  it('LanguageChanged_HtmlLangFollows INV:A11Y-05', async () => {
    const translate = TestBed.inject(TranslateService);
    await firstValueFrom(translate.use('de'));
    expect(document.documentElement.lang).toBe('de');

    await firstValueFrom(translate.use('en'));
    expect(document.documentElement.lang).toBe('en');
  });
});
