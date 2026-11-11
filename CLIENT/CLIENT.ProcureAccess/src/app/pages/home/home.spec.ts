// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { render, screen, within } from '@testing-library/angular';

import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { Home } from './home';

describe('Home (S04, R-SHL-05)', () => {
  beforeEach(async () => {
    await render(Home, { providers: [provideTestTranslations()] });
  });

  it('shows the H1 and the intro', () => {
    expect(screen.getByRole('heading', { level: 1, name: 'Barrierefreie Software beschaffen – Schritt für Schritt' })).toBeTruthy();
    expect(screen.getByText(/ProcureAccess hilft Ihnen, die passenden Barrierefreiheitsanforderungen/)).toBeTruthy();
  });

  it('lists one card per existing target, each with its H2 and a link that needs no account', () => {
    const cards = within(screen.getByRole('list')).getAllByRole('listitem');
    expect(cards).toHaveLength(2);

    const [criteria, directory] = cards;
    expect(within(criteria).getByRole('heading', { level: 2, name: 'Kriterien zusammenstellen' })).toBeTruthy();
    expect(within(criteria).getByRole('link', { name: 'Kriterien zusammenstellen' }).getAttribute('href')).toBe('/kriterien');
    expect(within(directory).getByRole('heading', { level: 2, name: 'Produkte nachschlagen' })).toBeTruthy();
    expect(within(directory).getByRole('link', { name: 'Produktverzeichnis öffnen' }).getAttribute('href')).toBe('/products');
  });

  it('has no entry for projects, contributions or vendors yet (owner decision S2-17, 1A)', () => {
    expect(screen.queryByRole('heading', { name: 'Produkte vergleichen im Beschaffungsprojekt' })).toBeNull();
    expect(screen.queryByRole('heading', { name: 'Erfahrungen teilen' })).toBeNull();
    expect(screen.queryByRole('link', { name: /Prüfergebnis veröffentlichen/ })).toBeNull();
  });

  it('ends with the disclaimer as an info notice after the cards', () => {
    const disclaimer = screen.getByRole('note');
    expect(disclaimer.textContent).toContain('ProcureAccess ersetzt keine professionelle Barrierefreiheitsprüfung.');
    expect(screen.getByRole('list').compareDocumentPosition(disclaimer) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
  });
});
