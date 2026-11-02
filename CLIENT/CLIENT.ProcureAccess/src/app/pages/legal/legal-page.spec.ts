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
    ['privacy', 'Datenschutzhinweise'],
    ['accessibility', 'Erklärung zur Barrierefreiheit'],
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
