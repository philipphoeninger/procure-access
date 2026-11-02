// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { render, screen, within } from '@testing-library/angular';

import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { SHELL_TEST_ROUTES } from '@app/testing/shell-test-doubles';
import { Footer, SOURCE_CODE_URL } from './footer';

describe('Footer', () => {
  it('is the contentinfo landmark with the legal pages and the source code', async () => {
    await render(Footer, { routes: SHELL_TEST_ROUTES, providers: [provideTestTranslations()] });

    const links = within(screen.getByRole('contentinfo')).getAllByRole('link');
    expect(links.map((link) => [link.textContent?.trim(), link.getAttribute('href')])).toEqual([
      ['Erklärung zur Barrierefreiheit', '/barrierefreiheit'],
      ['Datenschutz', '/datenschutz'],
      ['Impressum', '/impressum'],
      ['Quellcode auf GitHub', SOURCE_CODE_URL],
    ]);
  });
});
