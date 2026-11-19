// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { render, screen, within } from '@testing-library/angular';

import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';

import { Breadcrumb } from './breadcrumb';

describe('pa-breadcrumb', () => {
  it('names the navigation, links the pages above and marks the page shown as current, without separator text', async () => {
    await render(Breadcrumb, {
      inputs: {
        items: [
          { label: 'Projekte', link: '/projekte' },
          { label: 'Neues Bewerbermanagement 2027', link: '/projekte/p-1' },
          { label: 'BewerberPro' },
        ],
      },
      routes: [],
      providers: [provideTestTranslations()],
    });

    const path = screen.getByRole('navigation', { name: 'Sie sind hier' });
    const items = within(path).getAllByRole('listitem');
    expect(items.map((item) => item.textContent?.trim())).toEqual(['Projekte', 'Neues Bewerbermanagement 2027', 'BewerberPro']);
    expect(within(path).getAllByRole('link').map((link) => link.getAttribute('href'))).toEqual(['/projekte', '/projekte/p-1']);
    const current = within(items[2]).getByText('BewerberPro');
    expect(current.tagName).toBe('SPAN');
    expect(current.getAttribute('aria-current')).toBe('page');
    await expectNoAxeViolations();
  });
});
