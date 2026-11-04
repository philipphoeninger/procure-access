// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ActivatedRoute, provideRouter } from '@angular/router';
import { render, screen } from '@testing-library/angular';

import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { ERROR_PAGE_KEY, ErrorPage, ErrorPageKind } from './error-page';

describe('ErrorPage (S80, R-SHL-08)', () => {
  it.each<[ErrorPageKind, string, RegExp]>([
    ['notFound', 'Seite nicht gefunden', /^Diese Seite gibt es nicht oder nicht mehr\./],
    ['forbidden', 'Kein Zugriff', /^Für diese Seite fehlen Ihnen die Rechte\./],
    ['server', 'Etwas ist schiefgelaufen', /^Das ist ein Fehler bei uns\. Ihre Eingaben wurden nicht gespeichert\./],
  ])('shows the %s page: H1, text and the link to the start page', async (kind, title, body) => {
    await render(ErrorPage, {
      providers: [
        provideTestTranslations(),
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { snapshot: { data: { [ERROR_PAGE_KEY]: kind } } } },
      ],
    });

    expect(screen.getByRole('heading', { level: 1, name: title })).toBeTruthy();
    expect(screen.getByText(body)).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Zur Startseite' }).getAttribute('href')).toBe('/');
  });
});
