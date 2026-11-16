// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { signal } from '@angular/core';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { render, screen, within } from '@testing-library/angular';
import { fireEvent } from '@testing-library/dom';
import { BehaviorSubject, of } from 'rxjs';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { provideLoadedConfiguratorSettings } from '@app/features/configurator/testing/configurator-settings-stub';
import { OrganizationRole, OrganizationSummary } from '@app/features/organization/models/organization';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { ExportDialog } from '../../dialogs/export-dialog/export-dialog';
import { ShareDialog } from '../../dialogs/share-dialog/share-dialog';
import { CriteriaResponse } from '../../models/criteria';
import { SavedList } from '../../models/saved-lists';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import { criteriaAnswer } from '../../testing/criteria-fixture';
import { filterOptionsAnswer } from '../../testing/filter-options-fixture';
import { SavedListEditor } from './saved-list-editor';
import { SavedListPage } from './saved-list-page';
import { decisionsOf, SavedListLoad, SavedListNavigationState, SavedListView, selectionOf } from './saved-list-load';

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

function listView(activeRelease: string | null = 'T.1'): SavedListView {
  const criteria = answer();
  return {
    kind: 'list',
    list: LIST,
    selection: selectionOf(LIST),
    criteria,
    options: filterOptionsAnswer().layers,
    decisions: decisionsOf(LIST, criteria),
    activeRelease,
  };
}

function membership(role: OrganizationRole): OrganizationSummary {
  return { id: 'org-1', name: 'Stadt Musterhausen', type: 'publicBody', role };
}

describe('SavedListPage (R-SAV-02, R-SAV-03, R-LST-14; S5-21b)', () => {
  let setPageTitleState: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;
  let decide: ReturnType<typeof vi.fn>;
  let copyToActiveRelease: ReturnType<typeof vi.fn>;
  let paramMap: BehaviorSubject<ReturnType<typeof convertToParamMap>>;

  async function openPage({
    view = listView(),
    role = 'editor',
  }: { view?: SavedListView; role?: OrganizationRole | null } = {}) {
    setPageTitleState = vi.fn();
    announce = vi.fn().mockResolvedValue(undefined);
    decide = vi.fn().mockResolvedValue(null);
    copyToActiveRelease = vi.fn().mockResolvedValue({ id: 'list-2', releaseVersion: 'T.2', droppedValues: [], notCarried: [] });
    paramMap = new BehaviorSubject(convertToParamMap({ id: 'list-1' }));
    const load = new SavedListLoad();
    load.view = view;
    const rendered = await render(SavedListPage, {
      routes: [{ path: 'kriterienlisten/:id', children: [] }, { path: 'kriterienlisten/:id/auswahl', children: [] }],
      providers: [
        provideTestTranslations(),
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        { provide: SavedListLoad, useValue: load },
        SavedListEditor,
        { provide: ActivatedRoute, useValue: { paramMap, snapshot: { data: {} } } },
        { provide: PageTitleStrategy, useValue: { setPageTitleState } },
        provideLoadedConfiguratorSettings(),
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: ConfiguratorApiService, useValue: { decide, copyToActiveRelease } },
        { provide: ProcureAccessStore, useValue: { organizations: signal(role === null ? [] : [membership(role)]) } },
      ],
    });
    await rendered.fixture.whenStable();
    return rendered.fixture;
  }

  const card = (title: string) => screen.getByRole('heading', { level: 2, name: title }).closest('pa-criterion-card') as HTMLElement;

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

  describe('Reader (owner decision S5-21b 6a)', () => {
    it('shows the stored decisions read only, the filter row and only „Liste exportieren“', async () => {
      await openPage({ role: 'reader' });

      expect(screen.queryByRole('checkbox', { name: /^In Liste aufnehmen/ })).toBeNull();
      expect(screen.queryByRole('button', { name: /^Priorität/ })).toBeNull();
      expect(screen.queryByRole('button', { name: /Notiz/ })).toBeNull();
      expect(screen.getByRole('searchbox')).toBeTruthy();
      expect(screen.queryByRole('link', { name: 'Auswahl ändern' })).toBeNull();
      // Sharing is for Editor and Owner (owner decision S5-24 2a).
      expect(screen.queryByRole('button', { name: 'Freigabelink erstellen' })).toBeNull();
      expect(screen.getByRole('button', { name: 'Liste exportieren' })).toBeTruthy();
      // A saved list saves each change: no „Noch nicht gespeichert“ (owner decision S5-22 4).
      expect(screen.queryByText('Noch nicht gespeichert')).toBeNull();

      expect(card('Bilder haben einen Alternativtext').textContent).toContain('Nicht in Ihrer Liste');
      const weighted = card('Alles geht mit der Tastatur');
      expect(weighted.textContent).toContain('Priorität: B – Bewertungskriterium');
      expect(weighted.textContent).toContain('Gewichtung 9');
      expect(within(weighted).getByRole('heading', { level: 3, name: 'Notiz' })).toBeTruthy();
      const overridden = card('Überschriften');
      expect(overridden.textContent).toContain('Priorität: A – Mindestanforderung');
      expect(overridden.textContent).toContain('Geändert. Empfohlen war: Hinweis');
      await expectNoAxeViolations();
    });

    it('does not see the older-release notice (owner decision S5-21b 5b)', async () => {
      await openPage({ role: 'reader', view: listView('T.2') });

      expect(screen.queryByRole('button', { name: 'Neue Liste mit derselben Auswahl' })).toBeNull();
      expect(document.body.textContent).not.toContain('Inzwischen gibt es die Version');
    });
  });

  describe('Editor and Owner (owner decision S5-21b 1a)', () => {
    it('saves an exclusion right away with the whole decision of the criterion', async () => {
      const fixture = await openPage({ role: 'owner' });

      within(card('Überschriften')).getByRole('checkbox', { name: /^In Liste aufnehmen/ }).click();
      await fixture.whenStable();

      expect(decide).toHaveBeenCalledWith('list-1', 202, { isIncluded: false, overrideLevel: 'A', overrideWeight: null, note: null });
      expect(card('Überschriften').textContent).toContain('Nicht in Ihrer Liste');
      expect(announce).toHaveBeenCalledWith('criteria.list.status.count', { included: 2, total: 4 }, { throttleMs: 1000 });
    });

    it('undoes a change the server did not save', async () => {
      const fixture = await openPage();
      decide.mockRejectedValueOnce(new Error('offline'));

      within(card('Überschriften')).getByRole('checkbox', { name: /^In Liste aufnehmen/ }).click();
      await fixture.whenStable();
      await new Promise((resolve) => setTimeout(resolve));
      await fixture.whenStable();

      expect(card('Überschriften').textContent).not.toContain('Nicht in Ihrer Liste');
    });

    it('links „Auswahl ändern“ to S10 of the list and exports the stored list', async () => {
      const open = vi.spyOn(ExportDialog, 'open').mockReturnValue({ afterClosed: () => of(undefined) } as never);
      await openPage();

      expect(screen.getByRole('link', { name: 'Auswahl ändern' }).getAttribute('href')).toBe('/kriterienlisten/list-1/auswahl');
      expect(screen.queryByText('Noch nicht gespeichert')).toBeNull();
      screen.getByRole('button', { name: 'Liste exportieren' }).click();

      await vi.waitFor(() =>
        expect(open).toHaveBeenCalledWith(expect.anything(), { listId: 'list-1', release: 'T.1', included: 3, tooLongNote: null }),
      );
      open.mockRestore();
    });

    it('opens S15 for the stored list with „Freigabelink erstellen“, also on an older release (R-SAV-04, S5-24 2a)', async () => {
      const open = vi.spyOn(ShareDialog, 'open').mockReturnValue({} as never);
      await openPage({ view: listView('T.2'), role: 'owner' });

      screen.getByRole('button', { name: 'Freigabelink erstellen' }).click();

      expect(open).toHaveBeenCalledWith(expect.anything(), { listId: 'list-1' });
      open.mockRestore();
    });

    it('hands a too-long note, never stored, to S13 and opens it after „Zur Notiz“ (TD-40)', async () => {
      const open = vi.spyOn(ExportDialog, 'open').mockReturnValue({ afterClosed: () => of({ toNote: '11.3.3' }) } as never);
      const fixture = await openPage();
      const headings = card('Überschriften');
      within(headings).getByRole('button', { name: 'Notiz hinzufügen' }).click();
      await fixture.whenStable();
      const note = within(headings).getByRole<HTMLTextAreaElement>('textbox', { name: 'Ihre Notiz zu diesem Kriterium (optional)' });
      note.value = 'x'.repeat(1001);
      fireEvent.input(note);
      await fixture.whenStable();
      screen.getByRole('button', { name: 'Liste exportieren' }).focus();

      screen.getByRole('button', { name: 'Liste exportieren' }).click();

      await vi.waitFor(() => expect(document.activeElement).toBe(note));
      expect(open.mock.calls[0][1]?.tooLongNote?.key).toBe('11.3.3');
      expect(within(headings).getByText('Bitte kürzen Sie den Text auf höchstens 1000 Zeichen. Derzeit sind es 1001.')).toBeTruthy();
      open.mockRestore();
    });

    it('offers „Neue Liste mit derselben Auswahl“ on a list of an older release, without „Auswahl ändern“ (R-LST-14, S5-21 5a)', async () => {
      const fixture = await openPage({ view: listView('T.2') });
      const router = fixture.debugElement.injector.get(Router);
      const navigate = vi.spyOn(router, 'navigateByUrl').mockResolvedValue(true);

      expect(document.body.textContent).toContain('Diese Liste beruht auf der Katalogversion T.1. Inzwischen gibt es die Version T.2.');
      expect(screen.queryByRole('link', { name: 'Auswahl ändern' })).toBeNull();
      // The criteria stay editable (owner decision S5-21 5a).
      expect(within(card('Überschriften')).getByRole('checkbox', { name: /^In Liste aufnehmen/ })).toBeTruthy();
      await expectNoAxeViolations();

      screen.getByRole('button', { name: 'Neue Liste mit derselben Auswahl' }).click();
      await fixture.whenStable();
      await new Promise((resolve) => setTimeout(resolve));

      expect(copyToActiveRelease).toHaveBeenCalledWith('list-1');
      expect(navigate).toHaveBeenCalledWith('/kriterienlisten/list-2', {
        state: { savedListArrival: { kind: 'copied', releaseVersion: 'T.2', droppedValues: [], notCarried: [] } },
      });
    });
  });

  it('INV:CAT-22 shows and announces the report of a copy on arrival', async () => {
    const fixture = await openPage();
    const router = fixture.debugElement.injector.get(Router);
    const state: SavedListNavigationState = {
      savedListArrival: {
        kind: 'copied',
        releaseVersion: 'T.2',
        droppedValues: ['Audio'],
        notCarried: [
          { criterionRevisionId: 7, number: '99.7', title: 'Entfallenes Kriterium' },
          { criterionRevisionId: 4, number: null, title: 'Geprüft nach App-Test' },
        ],
      },
    };
    vi.spyOn(router, 'currentNavigation').mockReturnValue({ extras: { state } } as never);

    paramMap.next(convertToParamMap({ id: 'list-2' }));
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));

    const text = document.body.textContent;
    expect(text).toContain('Neue Liste nach Katalogversion T.2 erstellt. Ihre bisherige Liste bleibt unverändert.');
    expect(text).toContain('Diese Auswahl gibt es in der neuen Katalogversion nicht mehr oder sie ist dort ausgeschlossen: Audio.');
    expect(text).toContain('Ihre Änderungen an 2 Kriterien ließen sich nicht übernehmen');
    expect(text).toContain(': 99.7 Entfallenes Kriterium, Geprüft nach App-Test.');
    expect(announce).toHaveBeenCalledWith('criteria.list.copy.done', { release: 'T.2' });
    expect(announce).toHaveBeenCalledWith('criteria.list.copy.notCarried', {
      count: 2,
      criteria: '99.7 Entfallenes Kriterium, Geprüft nach App-Test',
    });
  });

  it('shows and announces the decisions a selection change dropped (owner decision S5-21 4a)', async () => {
    const fixture = await openPage();
    const router = fixture.debugElement.injector.get(Router);
    const state: SavedListNavigationState = {
      savedListArrival: {
        kind: 'selectionChanged',
        droppedDecisions: [{ criterionRevisionId: 9, number: null, title: 'Geprüft nach BIT inklusiv Software-Test' }],
      },
    };
    vi.spyOn(router, 'currentNavigation').mockReturnValue({ extras: { state } } as never);

    paramMap.next(convertToParamMap({ id: 'list-1' }));
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));

    expect(document.body.textContent).toContain(
      'Ihre Änderung an 1 Kriterium wurde entfernt, weil es nicht mehr zu Ihrer Auswahl gehört: Geprüft nach BIT inklusiv Software-Test.',
    );
    expect(announce).toHaveBeenCalledWith('criteria.list.selectionChanged.dropped.one', {
      count: 1,
      criteria: 'Geprüft nach BIT inklusiv Software-Test',
    });
  });

  it('keeps the detail panel with „Warum diese Priorität?“ (CAT-02)', async () => {
    const fixture = await openPage({ role: 'reader' });

    screen.getAllByRole('button', { name: 'Details anzeigen' })[0].click();
    await fixture.whenStable();

    expect(screen.getByRole('heading', { level: 3, name: 'Warum diese Priorität?' })).toBeTruthy();
  });

  it('shows „Seite nicht gefunden“ for a list that does not exist for the user (TEN-01)', async () => {
    await openPage({ view: { kind: 'notFound' } });

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
