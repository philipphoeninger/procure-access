// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { render, screen } from '@testing-library/angular';

import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { CatalogText } from './catalog-text';

async function show(text: string, maxItems: number | null = null) {
  return render(CatalogText, {
    providers: [provideTestTranslations()],
    inputs: { text, maxItems },
  });
}

describe('CatalogText (O-S4-30, O-S4-36)', () => {
  it('renders paragraphs, nested lists and bold as elements', async () => {
    const { container } = await show('**Überschrift**\n\nText.\n\n- eins\n  - eins a\n- zwei');

    expect(container.querySelector('p strong')?.textContent).toBe('Überschrift');
    const outer = screen.getAllByRole('list')[0];
    expect(outer?.children).toHaveLength(2);
    expect(outer?.querySelector('li > ul > li')?.textContent?.trim()).toBe('eins a');
    await expectNoAxeViolations();
  });

  it('renders an http(s) link that opens in a new tab and says so', async () => {
    await show(
      'Siehe ([https://pac.pdf-accessibility.org/de](https://pac.pdf-accessibility.org/de)).',
    );

    const link = screen.getByRole('link', {
      name: 'https://pac.pdf-accessibility.org/de (öffnet in neuem Tab)',
    });
    expect(link.getAttribute('href')).toBe('https://pac.pdf-accessibility.org/de');
    expect(link.getAttribute('target')).toBe('_blank');
    expect(link.getAttribute('rel')).toBe('noopener noreferrer');
    // Verbatim: no space added around the link (the text pieces sit directly against it).
    expect(link.closest('p')?.textContent).toBe(
      'Siehe (https://pac.pdf-accessibility.org/de (öffnet in neuem Tab)).',
    );
  });

  it('never renders HTML or a link of another scheme from the text', async () => {
    const { container } = await show('bei: <H2> Dies ist eine </H2> [x](javascript:alert(1))');

    expect(container.querySelector('h2')).toBeNull();
    expect(screen.queryByRole('link')).toBeNull();
    expect(container.textContent).toContain('<H2> Dies ist eine </H2> [x](javascript:alert(1))');
  });

  it('shows only the first top-level items and focuses a revealed one on request', async () => {
    const { fixture } = await show('- a\n- b\n- c', 1);

    expect(screen.getAllByRole('listitem').map((li) => li.textContent?.trim())).toEqual(['a']);

    fixture.componentRef.setInput('maxItems', null);
    fixture.componentInstance.focusItem(1);
    await fixture.whenStable();

    expect(document.activeElement?.textContent?.trim()).toBe('b');
    expect(document.activeElement?.getAttribute('tabindex')).toBe('-1');
  });
});
