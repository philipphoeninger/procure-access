// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import { of } from 'rxjs';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';

import { CriteriaResponse } from '../../models/criteria';
import { SharedList } from '../../models/share-links';
import { CatalogApiService } from '../../services/api/catalog-api.service';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import { provideLoadedConfiguratorSettings } from '../../testing/configurator-settings-stub';
import { criteriaAnswer } from '../../testing/criteria-fixture';
import { filterOptionsAnswer } from '../../testing/filter-options-fixture';
import { decisionsOf, selectionOf } from '../saved-list/saved-list-load';
import { loadSharedListView, SharedListLoad, SharedListView } from './shared-list-load';
import { SharedListPage } from './shared-list-page';

/** The fixture answer with one revision per criterion, as a release has them. */
function answer(): CriteriaResponse {
  const base = criteriaAnswer();
  return { ...base, criteria: base.criteria.map((criterion, index) => ({ ...criterion, revisionId: 200 + index })) };
}

const SHARED: SharedList = {
  name: 'Vergabe Personalsoftware 2027',
  releaseVersion: 'T.1',
  expiresAt: '2027-02-14T23:00:00Z',
  selection: { useCaseTemplateId: 'hr', appTypeIds: ['web'], productPartIds: ['base'], testTypeIds: ['softwareTest'] },
  criteria: [
    { criterionRevisionId: 200, isIncluded: false, resolvedLevel: 'A', overrideLevel: null, overrideWeight: null, note: null },
    { criterionRevisionId: 201, isIncluded: true, resolvedLevel: 'B', overrideLevel: null, overrideWeight: 9, note: 'Mit Anbieter klären' },
    { criterionRevisionId: 202, isIncluded: true, resolvedLevel: 'Info', overrideLevel: 'A', overrideWeight: null, note: null },
    { criterionRevisionId: 203, isIncluded: true, resolvedLevel: 'B', overrideLevel: null, overrideWeight: null, note: null },
  ],
};

function listView(): SharedListView {
  const criteria = answer();
  return {
    kind: 'list',
    list: SHARED,
    selection: selectionOf(SHARED),
    criteria,
    options: filterOptionsAnswer().layers,
    decisions: decisionsOf(SHARED, criteria),
  };
}

describe('S16 Geteilte Kriterienliste (R-SAV-05, TEN-09)', () => {
  let setPageTitleState: ReturnType<typeof vi.fn>;

  async function openPage(view: SharedListView) {
    setPageTitleState = vi.fn();
    const load = new SharedListLoad();
    load.view = view;
    const rendered = await render(SharedListPage, {
      providers: [
        provideTestTranslations(),
        provideRouter([]),
        provideLoadedConfiguratorSettings(),
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        { provide: SharedListLoad, useValue: load },
        { provide: ActivatedRoute, useValue: { data: of({}), snapshot: { data: {} } } },
        { provide: PageTitleStrategy, useValue: { setPageTitleState } },
      ],
    });
    await rendered.fixture.whenStable();
    return rendered.fixture;
  }

  it('INV:TEN-09 shows the shared list read only: no checkbox, no K5 menu, no note field, no filter row, no action bar', async () => {
    await openPage(listView());

    expect(screen.getByRole('heading', { level: 1, name: 'Geteilte Kriterienliste: Vergabe Personalsoftware 2027' })).toBeTruthy();
    expect(setPageTitleState).toHaveBeenCalledWith({ params: { name: 'Vergabe Personalsoftware 2027' } });
    expect(document.body.textContent).toContain('Diese Liste wurde mit Ihnen geteilt. Sie können sie ohne Konto lesen.');
    expect(screen.getByRole('link', { name: 'Kriterien zusammenstellen' }).getAttribute('href')).toBe('/kriterien');
    expect(screen.getAllByRole('heading', { level: 2 }).map((heading) => heading.textContent.trim())).toEqual([
      'Bilder haben einen Alternativtext',
      'Alles geht mit der Tastatur',
      'Überschriften',
      'Geprüft nach BIT inklusiv Software-Test',
    ]);
    expect(screen.queryByRole('checkbox')).toBeNull();
    expect(screen.queryByRole('button', { name: /^Priorität/ })).toBeNull();
    expect(screen.queryByRole('button', { name: /Notiz/ })).toBeNull();
    expect(screen.queryByRole('searchbox')).toBeNull();
    expect(screen.queryByRole('button', { name: 'Liste exportieren' })).toBeNull();
    // The stored decisions show as text: the note and the exclusion (SPEC §8.5).
    expect(document.body.textContent).toContain('Mit Anbieter klären');
    expect(document.body.textContent).toContain('Nicht in Ihrer Liste');
    await expectNoAxeViolations();
  });

  it('shows one collapsed rating legend after the counts, above the cards, with the levels of the list (S5-31)', async () => {
    const fixture = await openPage(listView());
    const legend = screen.getByRole('button', { name: 'Was bedeuten die Bewertungsstufen?' });

    expect(document.querySelectorAll('pa-rating-legend')).toHaveLength(1);
    expect(legend.getAttribute('aria-expanded')).toBe('false');
    expect(legend.compareDocumentPosition(screen.getByRole('list')) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();

    legend.click();
    await fixture.whenStable();

    expect([...document.querySelectorAll('pa-rating-legend dt')].map((term) => term.textContent?.trim())).toEqual([
      'Erfüllt',
      'Einschränkung',
      'Barriere',
      'Blockade',
    ]);
    await expectNoAxeViolations();
  });

  it('INV:TEN-09 keeps the detail panel with „Warum diese Priorität?“ (CAT-02)', async () => {
    const fixture = await openPage(listView());

    screen.getAllByRole('button', { name: 'Details anzeigen' })[0].click();
    await fixture.whenStable();

    expect(screen.getByRole('heading', { level: 3, name: 'Warum diese Priorität?' })).toBeTruthy();
  });

  it('shows „Link nicht mehr gültig“ for a link that does not work (R-SAV-05, TEN-08)', async () => {
    await openPage({ kind: 'invalid' });

    expect(screen.getByRole('heading', { level: 1, name: 'Link nicht mehr gültig' })).toBeTruthy();
    expect(document.body.textContent).toContain('Er ist abgelaufen oder wurde zurückgenommen.');
    await expectNoAxeViolations();
  });

  it('shows the S80 server text for any other failure', async () => {
    await openPage({ kind: 'server' });

    expect(screen.getByRole('heading', { level: 1, name: 'Etwas ist schiefgelaufen' })).toBeTruthy();
  });
});

describe('loadSharedListView', () => {
  const catalog = (getCriteria = vi.fn().mockResolvedValue(answer())) =>
    ({ getCriteria, getFilterOptions: vi.fn().mockResolvedValue(filterOptionsAnswer()) }) as unknown as CatalogApiService;

  it('reads the list and the criteria of its own release', async () => {
    const getCriteria = vi.fn().mockResolvedValue(answer());
    const shared = vi.fn().mockResolvedValue(SHARED);

    const view = await loadSharedListView({ shared } as unknown as ConfiguratorApiService, catalog(getCriteria), 'tok');

    expect(shared).toHaveBeenCalledWith('tok');
    expect(getCriteria).toHaveBeenCalledWith(selectionOf(SHARED), 'T.1');
    expect(view.kind).toBe('list');
  });

  it('takes a 404 for an invalid link and anything else for a server failure', async () => {
    const refuse = (status: number) =>
      ({ shared: vi.fn().mockRejectedValue(new HttpErrorResponse({ status })) }) as unknown as ConfiguratorApiService;

    expect((await loadSharedListView(refuse(404), catalog(), 'tok')).kind).toBe('invalid');
    expect((await loadSharedListView(refuse(500), catalog(), 'tok')).kind).toBe('server');
    expect((await loadSharedListView(refuse(0), catalog(), 'tok')).kind).toBe('server');
  });
});
