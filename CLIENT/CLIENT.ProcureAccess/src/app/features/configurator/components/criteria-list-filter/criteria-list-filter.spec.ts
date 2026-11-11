// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { fireEvent } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/angular';

import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { PriorityLevel } from '../../models/criteria';
import { CriteriaListFilterRow } from './criteria-list-filter';

describe('CriteriaListFilterRow (S11 filter row, R-LST-04, R-LST-06)', () => {
  async function setUp({ searchText = '', levels = [] as PriorityLevel[], onlyIncluded = false } = {}) {
    const searchTextChange = vi.fn();
    const levelsChange = vi.fn();
    const onlyIncludedChange = vi.fn();
    const rendered = await render(CriteriaListFilterRow, {
      inputs: { searchText, levels, onlyIncluded },
      on: { searchTextChange, levelsChange, onlyIncludedChange },
      providers: [provideTestTranslations()],
    });
    await rendered.fixture.whenStable();
    return { ...rendered, searchTextChange, levelsChange, onlyIncludedChange };
  }

  const group = () => screen.getByRole('group', { name: 'Liste filtern' });
  const priority = () => within(group()).getByRole('group', { name: 'Priorität' });

  it('names the group and holds the search field, the priority levels and the inclusion filter', async () => {
    await setUp();

    expect(within(group()).getByRole('searchbox', { name: 'In Kriterien suchen' })).toBeTruthy();
    expect(within(priority()).getAllByRole('checkbox').map((box) => box.closest('mat-checkbox')?.textContent?.trim())).toEqual([
      'A – Mindestanforderung',
      'B – Bewertungskriterium',
      'Hinweis',
    ]);
    expect(within(group()).getByRole('checkbox', { name: 'Nur aufgenommene Kriterien anzeigen' })).toBeTruthy();
    await expectNoAxeViolations();
  });

  it('shows the state it gets', async () => {
    await setUp({ searchText: 'tast', levels: ['B'], onlyIncluded: true });

    expect(screen.getByRole<HTMLInputElement>('searchbox').value).toBe('tast');
    expect(screen.getByRole<HTMLInputElement>('checkbox', { name: 'B – Bewertungskriterium' }).checked).toBe(true);
    expect(screen.getByRole<HTMLInputElement>('checkbox', { name: 'A – Mindestanforderung' }).checked).toBe(false);
    expect(screen.getByRole<HTMLInputElement>('checkbox', { name: 'Nur aufgenommene Kriterien anzeigen' }).checked).toBe(
      true,
    );
  });

  it('emits the search field as typed', async () => {
    const { fixture, searchTextChange } = await setUp();
    const search = screen.getByRole<HTMLInputElement>('searchbox');

    search.value = 'Tastatur';
    fireEvent.input(search);
    await fixture.whenStable();

    expect(searchTextChange).toHaveBeenCalledWith('Tastatur');
  });

  it('emits the levels in K5 order when one is checked or unchecked (owner decision O-S4-44 (a))', async () => {
    const { fixture, levelsChange } = await setUp({ levels: ['Info'] });

    screen.getByRole('checkbox', { name: 'A – Mindestanforderung' }).click();
    await fixture.whenStable();
    expect(levelsChange).toHaveBeenLastCalledWith(['A', 'Info']);

    screen.getByRole('checkbox', { name: 'Hinweis' }).click();
    await fixture.whenStable();
    expect(levelsChange).toHaveBeenLastCalledWith([]);
  });

  it('emits „Nur aufgenommene Kriterien anzeigen“', async () => {
    const { fixture, onlyIncludedChange } = await setUp();

    screen.getByRole('checkbox', { name: 'Nur aufgenommene Kriterien anzeigen' }).click();
    await fixture.whenStable();

    expect(onlyIncludedChange).toHaveBeenCalledWith(true);
  });

  it('moves the focus into the search field on request', async () => {
    const { fixture } = await setUp();

    fixture.componentInstance.focusSearch();

    expect(document.activeElement).toBe(screen.getByRole('searchbox'));
  });
});
