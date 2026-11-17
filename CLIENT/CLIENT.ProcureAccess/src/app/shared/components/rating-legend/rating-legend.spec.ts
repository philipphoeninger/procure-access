// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { render, screen } from '@testing-library/angular';

import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { AssessmentValue, RatingLegend } from './rating-legend';

describe('RatingLegend (pa-rating-legend, Handoff §4.11, UX copy U24)', () => {
  async function setUp(inputs: { values?: AssessmentValue[]; expanded?: boolean } = {}) {
    const rendered = await render(RatingLegend, { inputs, providers: [provideTestTranslations()] });
    await rendered.fixture.whenStable();
    return rendered;
  }

  const toggle = () => screen.getByRole('button');
  const terms = (container: Element) => [...container.querySelectorAll('dt')].map((term) => term.textContent?.trim());

  it('starts collapsed: the button names the disclosure and controls the hidden panel', async () => {
    await setUp();

    const button = screen.getByRole('button', { name: 'Was bedeuten die Bewertungsstufen?' });
    expect(button.getAttribute('aria-expanded')).toBe('false');
    const panel = document.getElementById(button.getAttribute('aria-controls') ?? '');
    expect(panel?.hidden).toBe(true);
    expect(screen.queryByRole('definition')).toBeNull();
    await expectNoAxeViolations();
  });

  it('opens with every level of the scale in order, its explanation and the footnote', async () => {
    const { fixture } = await setUp();

    toggle().click();
    await fixture.whenStable();

    const button = screen.getByRole('button', { name: 'Erklärung der Bewertungsstufen ausblenden' });
    expect(button.getAttribute('aria-expanded')).toBe('true');
    const panel = document.getElementById(button.getAttribute('aria-controls') ?? '');
    expect(panel?.hidden).toBe(false);
    expect(panel?.previousElementSibling).toBe(button);
    expect(terms(panel!)).toEqual([
      'Erfüllt',
      'Leichte Einschränkung',
      'Einschränkung',
      'Barriere',
      'Blockade',
      'Nicht anwendbar',
      'Nicht eingeschätzt',
    ]);
    expect(screen.getAllByRole('definition')).toHaveLength(7);
    expect(screen.getByText('Betroffene Menschen können die Funktion gar nicht nutzen.').tagName).toBe('DD');
    expect(
      screen.getByText(/^Welche Stufen bei einem Kriterium möglich sind, steht in seinen Details\./).tagName,
    ).toBe('P');
    await expectNoAxeViolations();
  });

  it('closes again', async () => {
    const { fixture } = await setUp({ expanded: true });

    toggle().click();
    await fixture.whenStable();

    expect(screen.getByRole('button', { name: 'Was bedeuten die Bewertungsstufen?' }).getAttribute('aria-expanded')).toBe(
      'false',
    );
  });

  it('explains only the given levels, in scale order', async () => {
    await setUp({ expanded: true, values: ['blockade', 'fulfilled', 'notApplicable'] });

    expect(terms(document.body)).toEqual(['Erfüllt', 'Blockade', 'Nicht anwendbar']);
    expect(screen.queryByText('Kleine Mängel. Die Nutzung ist kaum erschwert.')).toBeNull();
  });
});
