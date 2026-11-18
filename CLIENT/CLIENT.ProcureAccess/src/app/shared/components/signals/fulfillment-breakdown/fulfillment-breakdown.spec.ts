// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { render, screen } from '@testing-library/angular';

import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { FulfillmentBreakdown, WorkspaceFulfillmentCounts } from './fulfillment-breakdown';

const COUNTS: WorkspaceFulfillmentCounts = {
  fulfilled: 22,
  slightRestriction: 3,
  restriction: 6,
  barrier: 2,
  blockade: 1,
  notApplicable: 0,
  unknown: 10,
};

describe('FulfillmentBreakdown (K1 pa-fulfillment-breakdown, scope workspace, Handoff §4.1)', () => {
  async function setUp(counts: WorkspaceFulfillmentCounts, headingLevel?: 3 | 4) {
    const rendered = await render(FulfillmentBreakdown, {
      inputs: { counts, listName: 'HR-Software 2027', ...(headingLevel ? { headingLevel } : {}) },
      providers: [provideTestTranslations()],
    });
    await rendered.fixture.whenStable();
    return rendered;
  }

  const rows = (container: Element) =>
    [...container.querySelectorAll('dl > div')].map((row) => [
      row.querySelector('dt')?.textContent?.trim(),
      row.querySelector('dd')?.textContent?.trim(),
    ]);

  it('INV:SIG-13 shows exactly seven rows in the fixed order of the scale, zero and singular included (SIG-08, tagged with step 6)', async () => {
    const { container } = await setUp(COUNTS);

    expect(rows(container)).toEqual([
      ['Erfüllt', '22 Kriterien'],
      ['Leichte Einschränkung', '3 Kriterien'],
      ['Einschränkung', '6 Kriterien'],
      ['Barriere', '2 Kriterien'],
      ['Blockade', '1 Kriterium'],
      ['Nicht anwendbar', '0 Kriterien'],
      ['Nicht eingeschätzt', '10 Kriterien'],
    ]);
    // No eighth row in the workspace: the assessment is pinned to the list's release (SIG-13)
    expect(container.textContent).not.toContain('Damals nicht im Katalog');
    await expectNoAxeViolations();
  });

  it('keeps the order when the values would sort differently, and shows no total or share (SIG-08)', async () => {
    const { container } = await setUp({ ...COUNTS, fulfilled: 0, unknown: 46 });

    expect(rows(container).map(([term]) => term)).toEqual([
      'Erfüllt',
      'Leichte Einschränkung',
      'Einschränkung',
      'Barriere',
      'Blockade',
      'Nicht anwendbar',
      'Nicht eingeschätzt',
    ]);
    expect(container.textContent).not.toMatch(/%|gesamt|Summe/i);
    expect(container.querySelector('progress, meter, [role="progressbar"], [role="meter"], svg, canvas')).toBeNull();
  });

  it('is titled by the list and introduced as the organisation’s own assessment; the title fits under an H3', async () => {
    await setUp(COUNTS, 4);

    expect(screen.getByRole('heading', { level: 4, name: 'Ihre Einschätzung nach „HR-Software 2027“' })).toBeTruthy();
    expect(screen.getByText('So haben Sie die Kriterien Ihrer Liste bisher eingeschätzt:').tagName).toBe('P');
  });

  it('has an H3 title by default and no named region (TESTING V229)', async () => {
    const { container } = await setUp(COUNTS);

    expect(screen.getByRole('heading', { level: 3 })).toBeTruthy();
    const section = container.querySelector('section');
    expect(section?.hasAttribute('aria-labelledby')).toBe(false);
    expect(section?.hasAttribute('aria-label')).toBe(false);
  });
});
