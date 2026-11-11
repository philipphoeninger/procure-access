// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ActivatedRoute } from '@angular/router';
import { render, screen } from '@testing-library/angular';

import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { LegalPage, LegalPageKind } from './legal-page';

describe('LegalPage', () => {
  it.each<[LegalPageKind, string]>([
    ['imprint', 'Impressum'],
    // The H1 carries a soft hyphen (U+00AD, UX copy U94), so long words break at the right place.
    ['privacy', 'Datenschutz\u00ADhinweise'],
    ['accessibility', 'Erklärung zur Barriere\u00ADfreiheit'],
  ])('shows the %s page with its H1 and the placeholder notice', async (legalPage, title) => {
    await render(LegalPage, {
      providers: [
        provideTestTranslations(),
        { provide: ActivatedRoute, useValue: { snapshot: { data: { legalPage } } } },
      ],
    });

    expect(screen.getByRole('heading', { level: 1, name: title })).toBeTruthy();
    expect(screen.getByText('Platzhalter: Der Inhalt dieser Seite fehlt noch.')).toBeTruthy();
  });
});
