// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { render, screen } from '@testing-library/angular';

import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { SelectionNames } from '../../state/selection-names';
import { CriteriaListSummary } from './criteria-list-summary';

describe('CriteriaListSummary (S11 summary, UX copy §5.5)', () => {
  const names: SelectionNames = {
    useCase: 'HR-Anwendung',
    appTypes: ['Website oder Webanwendung', 'Desktop-Anwendung'],
    productParts: ['Basisfunktionalität', 'Audio', 'Video'],
    testTypes: [],
  };

  async function setUp(count: number, selection: SelectionNames) {
    const rendered = await render(CriteriaListSummary, {
      inputs: { count, names: selection },
      providers: [provideTestTranslations()],
    });
    await rendered.fixture.whenStable();
    return rendered;
  }

  const rows = () =>
    screen.getAllByRole('term').map((term) => [term.textContent?.trim(), term.nextElementSibling?.textContent?.trim()]);

  it('shows the count and the selection as a description list with the names joined', async () => {
    await setUp(12, names);

    expect(screen.getByText('12 Kriterien für Ihre Auswahl')).toBeTruthy();
    expect(rows()).toEqual([
      ['Verwendungszweck', 'HR-Anwendung'],
      ['Anwendungstyp', 'Website oder Webanwendung und Desktop-Anwendung'],
      ['Funktionsbereiche', 'Basisfunktionalität, Audio und Video'],
    ]);
    await expectNoAxeViolations();
  });

  it('names „Keiner davon passt“, uses the singular and adds the test types only when chosen', async () => {
    await setUp(1, { ...names, useCase: null, testTypes: ['BIT inklusiv Software-Test'] });

    expect(screen.getByText('1 Kriterium für Ihre Auswahl')).toBeTruthy();
    expect(rows()[0]).toEqual(['Verwendungszweck', 'Keiner davon passt']);
    expect(rows()[3]).toEqual(['Prüfverfahren', 'BIT inklusiv Software-Test']);
  });
});
