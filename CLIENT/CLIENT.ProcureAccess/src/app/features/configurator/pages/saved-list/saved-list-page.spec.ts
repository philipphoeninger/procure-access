// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { render, screen, within } from '@testing-library/angular';
import { BehaviorSubject } from 'rxjs';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { PRIORITY_WEIGHT_RANGE } from '@app/features/configurator/state/priority-weight-range';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { CriteriaResponse } from '../../models/criteria';
import { SavedList } from '../../models/saved-lists';
import { criteriaAnswer } from '../../testing/criteria-fixture';
import { filterOptionsAnswer } from '../../testing/filter-options-fixture';
import { SavedListPage } from './saved-list-page';
import { decisionsOf, SavedListLoad, SavedListView, selectionOf } from './saved-list-load';

/** The fixture answer with one revision per criterion, as a release has them. */
function answer(): CriteriaResponse {
  const base = criteriaAnswer();
  return { ...base, criteria: base.criteria.map((criterion, index) => ({ ...criterion, revisionId: 200 + index })) };
}

const LIST: SavedList = {
  id: 'list-1',
  organizationId: 'org-1',
  name: 'Vergabe Personalsoftware 2027',
  releaseVersion: 'T.1',
  createdAt: '2026-11-16T09:00:00Z',
  modifiedAt: '2026-11-16T09:00:00Z',
  selection: { useCaseTemplateId: 'hr', appTypeIds: ['web'], productPartIds: ['base'], testTypeIds: ['softwareTest'] },
  criteria: [
    { criterionRevisionId: 200, isIncluded: false, resolvedLevel: 'A', overrideLevel: null, overrideWeight: null, note: null },
    { criterionRevisionId: 201, isIncluded: true, resolvedLevel: 'B', overrideLevel: null, overrideWeight: 9, note: 'Mit Anbieter klären' },
    { criterionRevisionId: 202, isIncluded: true, resolvedLevel: 'Info', overrideLevel: 'A', overrideWeight: null, note: null },
    { criterionRevisionId: 203, isIncluded: true, resolvedLevel: 'B', overrideLevel: null, overrideWeight: null, note: null },
  ],
};

function listView(): SavedListView {
  const criteria = answer();
  return {
    kind: 'list',
    list: LIST,
    selection: selectionOf(LIST),
    criteria,
    options: filterOptionsAnswer().layers,
    decisions: decisionsOf(LIST, criteria),
  };
}

describe('SavedListPage (S11 read only, R-SAV-02, S5-20 5a)', () => {
  let setPageTitleState: ReturnType<typeof vi.fn>;

  async function openPage(view: SavedListView = listView()) {
    setPageTitleState = vi.fn();
    const load = new SavedListLoad();
    load.view = view;
    const rendered = await render(SavedListPage, {
      providers: [
        provideTestTranslations(),
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        { provide: SavedListLoad, useValue: load },
        { provide: ActivatedRoute, useValue: { paramMap: new BehaviorSubject(convertToParamMap({ id: 'list-1' })), snapshot: { data: {} } } },
        { provide: PageTitleStrategy, useValue: { setPageTitleState } },
        { provide: PRIORITY_WEIGHT_RANGE, useValue: { min: 1, max: 10 } },
      ],
    });
    await rendered.fixture.whenStable();
    return rendered.fixture;
  }

  it('shows the list name as H1, titles the page with it and shows every criterion in catalog order', async () => {
    await openPage();

    expect(screen.getByRole('heading', { level: 1, name: 'Vergabe Personalsoftware 2027' })).toBeTruthy();
    expect(setPageTitleState).toHaveBeenCalledWith({ params: { name: 'Vergabe Personalsoftware 2027' } });
    expect(screen.getAllByRole('heading', { level: 2 }).map((heading) => heading.textContent.trim())).toEqual([
      'Bilder haben einen Alternativtext',
      'Alles geht mit der Tastatur',
      'Überschriften',
      'Geprüft nach BIT inklusiv Software-Test',
    ]);
    await expectNoAxeViolations();
  });

  it('shows the stored decisions without a checkbox, priority menu or note field', async () => {
    await openPage();

    expect(screen.queryByRole('checkbox')).toBeNull();
    expect(screen.queryByRole('button', { name: /^Priorität/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /Notiz/ })).toBeNull();
    expect(screen.queryByRole('textbox')).toBeNull();
    expect(screen.queryByRole('button', { name: 'Empfehlung wiederherstellen' })).toBeNull();

    const excluded = screen.getByRole('heading', { level: 2, name: 'Bilder haben einen Alternativtext' }).closest('pa-criterion-card')!;
    expect(excluded.textContent).toContain('Nicht in Ihrer Liste');

    const weighted = screen.getByRole('heading', { level: 2, name: 'Alles geht mit der Tastatur' }).closest('pa-criterion-card') as HTMLElement;
    expect(weighted.textContent).toContain('Priorität: B – Bewertungskriterium');
    expect(weighted.textContent).toContain('Gewichtung 9');
    expect(within(weighted).getByRole('heading', { level: 3, name: 'Notiz' })).toBeTruthy();
    expect(weighted.textContent).toContain('Mit Anbieter klären');

    const overridden = screen.getByRole('heading', { level: 2, name: 'Überschriften' }).closest('pa-criterion-card')!;
    expect(overridden.textContent).toContain('Priorität: A – Mindestanforderung');
    expect(overridden.textContent).toContain('Empfohlen war: Hinweis');
  });

  it('keeps the detail panel with „Warum diese Priorität?“ (CAT-02)', async () => {
    const fixture = await openPage();

    screen.getAllByRole('button', { name: 'Details anzeigen' })[0].click();
    await fixture.whenStable();

    expect(screen.getByRole('heading', { level: 3, name: 'Warum diese Priorität?' })).toBeTruthy();
  });

  it('shows „Seite nicht gefunden“ for a list that does not exist for the user (TEN-01)', async () => {
    await openPage({ kind: 'notFound' });

    expect(screen.getByRole('heading', { level: 1, name: 'Seite nicht gefunden' })).toBeTruthy();
  });
});

describe('decisionsOf', () => {
  it('keys the stored decisions by the criterion key of the release, a missing note as empty', () => {
    const decisions = decisionsOf(LIST, answer());

    expect(decisions['11.1.1']).toEqual({ included: false, overrideLevel: null, overrideWeight: null, note: '' });
    expect(decisions['11.2.2']).toEqual({ included: true, overrideLevel: null, overrideWeight: 9, note: 'Mit Anbieter klären' });
    expect(decisions['11.3.3'].overrideLevel).toBe('A');
  });

  it('turns the stored selection into the catalog query, without a use case when none is stored', () => {
    expect(selectionOf(LIST)).toEqual({ useCase: 'hr', appTypeIds: ['web'], productPartIds: ['base'], testTypeIds: ['softwareTest'] });
    expect(selectionOf({ ...LIST, selection: { ...LIST.selection, useCaseTemplateId: null } })).not.toHaveProperty('useCase');
  });
});
