// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Provider } from '@angular/core';
import {
  TranslateLoader,
  TranslationObject,
  provideTranslateLoader,
  provideTranslateService,
} from '@ngx-translate/core';
import { Observable, of } from 'rxjs';

import de from '../../../public/i18n/de.json';

// Serves the real German texts without HTTP, so component tests query by the names users
// see (CLAUDE §9). A missing key renders as the key itself, and the role query then fails.
class GermanTestLoader implements TranslateLoader {
  getTranslation(): Observable<TranslationObject> {
    return of(de);
  }
}

export function provideTestTranslations(): Provider[] {
  return provideTranslateService({
    lang: 'de',
    loader: provideTranslateLoader(GermanTestLoader),
  });
}
