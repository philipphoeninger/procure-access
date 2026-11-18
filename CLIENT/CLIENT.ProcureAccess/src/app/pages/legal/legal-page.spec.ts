// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { ActivatedRoute } from '@angular/router';
import { render, screen } from '@testing-library/angular';

import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { LegalPage } from './legal-page';
import { LEGAL_TEXTS, LegalPageKind } from './legal-texts';

const PLACEHOLDER = 'Platzhalter: Der Inhalt dieser Seite fehlt noch.';

async function renderPage(legalPage: LegalPageKind, texts?: Partial<Record<LegalPageKind, string>>) {
  return render(LegalPage, {
    providers: [
      provideTestTranslations(),
      { provide: ActivatedRoute, useValue: { snapshot: { data: { legalPage } } } },
      ...(texts ? [{ provide: LEGAL_TEXTS, useValue: texts }] : []),
    ],
  });
}

describe('LegalPage', () => {
  it.each<[LegalPageKind, string]>([
    ['imprint', 'Impressum'],
    // The H1 carries a soft hyphen (U+00AD, UX copy U94), so long words break at the right place.
    ['privacy', 'Datenschutz­hinweise'],
    ['accessibility', 'Erklärung zur Barriere­freiheit'],
  ])('shows the %s page of the repository with its H1 and, while it is one, the placeholder notice', async (legalPage, title) => {
    await renderPage(legalPage);

    expect(screen.getByRole('heading', { level: 1, name: title })).toBeTruthy();
    // The repository's files are placeholders until WERTE.IT delivers the texts (O-S6-6).
    expect(screen.getByText(PLACEHOLDER)).toBeTruthy();
  });

  it('renders the Markdown with real headings, paragraphs, lists and links, without the notice', async () => {
    await renderPage('imprint', {
      imprint: [
        '## Angaben zum Anbieter',
        'Verein **Beispiel**',
        '',
        '### Kontakt',
        '- Telefon',
        '- [Website](https://example.org)',
        '#### Aufsicht',
        'Text',
      ].join('\n'),
    });

    expect(screen.getByRole('heading', { level: 1, name: 'Impressum' })).toBeTruthy();
    expect(screen.getByRole('heading', { level: 2, name: 'Angaben zum Anbieter' })).toBeTruthy();
    expect(screen.getByRole('heading', { level: 3, name: 'Kontakt' })).toBeTruthy();
    expect(screen.getByRole('heading', { level: 4, name: 'Aufsicht' })).toBeTruthy();
    expect(screen.getByText('Beispiel').tagName).toBe('STRONG');
    expect(screen.getAllByRole('listitem')).toHaveLength(2);
    expect(screen.getByRole('link', { name: /^Website/ }).getAttribute('href')).toBe('https://example.org');
    expect(screen.queryByText(PLACEHOLDER)).toBeNull();
  });

  it('shows the placeholder notice above the content while the file carries the marker', async () => {
    await renderPage('privacy', { privacy: '<!-- PLATZHALTER -->\n## Entwurf\nText' });

    const notice = screen.getByText(PLACEHOLDER);
    const heading = screen.getByRole('heading', { level: 2, name: 'Entwurf' });
    expect(notice.compareDocumentPosition(heading) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
    expect(screen.queryByText(/PLATZHALTER/)).toBeNull();
  });

  it('shows the placeholder notice for an empty file', async () => {
    await renderPage('accessibility', { accessibility: '\n' });

    expect(screen.getByText(PLACEHOLDER)).toBeTruthy();
  });
});
