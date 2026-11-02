// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { render, screen } from '@testing-library/angular';

import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { Notice } from './notice';

async function renderNotice(attributes: string) {
  return render(`<pa-notice ${attributes}>Text der Meldung</pa-notice>`, {
    imports: [Notice],
    providers: [provideTestTranslations()],
  });
}

describe('Notice', () => {
  it('names its meaning in a hidden prefix, not only by colour and icon', async () => {
    await renderNotice('kind="attention"');

    expect(screen.getByText('Achtung:').className).toContain('cdk-visually-hidden');
    expect(screen.getByText('Text der Meldung')).toBeTruthy();
  });

  it.each([
    ['kind="info"', 'note'],
    ['kind="attention" live="polite"', 'status'],
    ['kind="error" title="Titel" live="assertive"', 'alert'],
  ])('with %s has the role %s (live region only when it appears after an action)', async (attributes, role) => {
    await renderNotice(attributes);

    expect(screen.getByRole(role)).toBeTruthy();
  });

  it('is no live region when static', async () => {
    await renderNotice('kind="error" title="Titel"');

    expect(screen.queryByRole('alert')).toBeNull();
    expect(screen.queryByRole('status')).toBeNull();
  });

  it('as error summary renders the title as h2 with the prefix and takes the focus', async () => {
    await renderNotice('kind="error" title="1 Angabe muss noch korrigiert werden" [focusOnShow]="true"');

    const heading = screen.getByRole('heading', { level: 2, name: 'Fehler: 1 Angabe muss noch korrigiert werden' });
    expect(document.activeElement).toBe(heading.closest('pa-notice'));
    expect(heading.closest('pa-notice')?.getAttribute('tabindex')).toBe('-1');
  });

  it('renders a title without focus as plain text', async () => {
    await renderNotice('kind="error" title="Export fehlgeschlagen"');

    expect(screen.queryByRole('heading')).toBeNull();
    expect(screen.getByText('Export fehlgeschlagen')).toBeTruthy();
    expect(document.activeElement).toBe(document.body);
    // The input `title` must not stay a native attribute: no tooltip, no second reading.
    expect(document.querySelector('pa-notice')?.hasAttribute('title')).toBe(false);
  });
});
