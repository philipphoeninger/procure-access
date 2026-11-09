// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { render, screen, within } from '@testing-library/angular';

import { CRITERIA_SUGGESTION_CHANNEL } from '@app/core/contact/criteria-suggestion';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { SHELL_TEST_ROUTES } from '@app/testing/shell-test-doubles';
import { Footer, SOURCE_CODE_URL } from './footer';

async function renderFooter(channel: string) {
  await render(Footer, {
    routes: SHELL_TEST_ROUTES,
    providers: [provideTestTranslations(), { provide: CRITERIA_SUGGESTION_CHANNEL, useValue: channel }],
  });
  return within(screen.getByRole('contentinfo'));
}

describe('Footer', () => {
  it('is the contentinfo landmark with the suggestion channel, the legal pages and the source code', async () => {
    const footer = await renderFooter('mailto:vorschlag@werte-it.example');

    const links = footer.getAllByRole('link');
    expect(links.map((link) => link.getAttribute('href'))).toEqual([
      'mailto:vorschlag@werte-it.example?subject=Vorschlag%20f%C3%BCr%20den%20Kriterienkatalog',
      '/barrierefreiheit',
      '/datenschutz',
      '/impressum',
      SOURCE_CODE_URL,
    ]);
    expect(links.slice(1).map((link) => link.textContent?.trim())).toEqual([
      'Erklärung zur Barrierefreiheit',
      'Datenschutz',
      'Impressum',
      'Quellcode auf GitHub',
    ]);
  });

  it('names a mail channel as opening the mail program (R-SUG-01)', async () => {
    const footer = await renderFooter('mailto:vorschlag@werte-it.example');

    const link = footer.getByRole('link', { name: 'Kriterium vorschlagen (öffnet Ihr E-Mail-Programm)' });
    expect(link.getAttribute('href')).toMatch(/^mailto:vorschlag@werte-it\.example\?subject=/);
  });

  it('names a web channel as opening an external page and links the URL as configured (R-SUG-01)', async () => {
    const footer = await renderFooter('https://werte-it.example/kriterium');

    const link = footer.getByRole('link', { name: 'Kriterium vorschlagen (öffnet eine externe Seite)' });
    expect(link.getAttribute('href')).toBe('https://werte-it.example/kriterium');
  });
});
