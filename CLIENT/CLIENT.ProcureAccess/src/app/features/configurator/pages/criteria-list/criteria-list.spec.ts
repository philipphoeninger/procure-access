// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { Location } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { fireEvent } from '@testing-library/dom';
import { provideRouter, Router } from '@angular/router';
import { signalStore, withMethods } from '@ngrx/signals';
import { render, screen, within } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { CatalogApiService } from '../../services/api/catalog-api.service';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import { ExportApiService } from '../../services/api/export-api.service';
import { CRITERIA_SEARCH_DEBOUNCE_MS, withConfigurator } from '../../state/with-configurator';
import { criteriaAnswer } from '../../testing/criteria-fixture';
import { filterOptionsAnswer } from '../../testing/filter-options-fixture';
import { CriteriaListPage } from './criteria-list';

// The real configurator slice stands in for the root store, so the page runs the S4-14 rules; the
// sign-in state (`signedIn` of withIdentity(), from the session probe) is a flag here.
let signedIn = false;
const ConfiguratorTestStore = signalStore(
  withConfigurator(),
  withMethods(() => ({ signedIn: () => signedIn })),
);

@Component({ template: '' })
class Blank {}

describe('CriteriaListPage (S11, R-LST-01 – R-LST-10, R-LST-13)', () => {
  let api: { getFilterOptions: ReturnType<typeof vi.fn>; getCriteria: ReturnType<typeof vi.fn> };
  let announce: ReturnType<typeof vi.fn>;

  async function setUp(getCriteria: ReturnType<typeof vi.fn>, { signedIn: asUser = false } = {}) {
    signedIn = asUser;
    api = {
      getFilterOptions: vi.fn().mockResolvedValue(filterOptionsAnswer()),
      getCriteria,
    };
    announce = vi.fn().mockResolvedValue(undefined);
    const rendered = await render(CriteriaListPage, {
      providers: [
        provideTestTranslations(),
        provideRouter([
          { path: 'filters', component: Blank },
          { path: 'kriterien', component: Blank },
        ]),
        { provide: ProcureAccessStore, useClass: ConfiguratorTestStore },
        { provide: CatalogApiService, useValue: api },
        { provide: ConfiguratorApiService, useValue: { reportCriteriaListShown: () => Promise.resolve(null) } },
        { provide: ExportApiService, useValue: { exportList: vi.fn() } },
        { provide: A11yAnnouncerService, useValue: { announce } },
        // jsdom never ends a CSS transition, so the K5 menu would never finish closing.
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
      ],
    });
    await rendered.fixture.whenStable();
    return rendered;
  }

  it('shows H1, „Auswahl ändern“ back to S10 and one card per criterion in catalog order', async () => {
    await setUp(vi.fn().mockResolvedValue(criteriaAnswer()));

    expect(screen.getByRole('heading', { level: 1, name: 'Ihre Kriterienliste' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Auswahl ändern' }).getAttribute('href')).toBe('/kriterien');
    const cards = within(screen.getByRole('list')).getAllByRole('listitem');
    expect(cards.map((card) => within(card).getByRole('heading', { level: 2 }).textContent?.trim())).toEqual([
      'Bilder haben einen Alternativtext',
      'Alles geht mit der Tastatur',
      'Überschriften',
      'Geprüft nach BIT inklusiv Software-Test',
    ]);
    await expectNoAxeViolations();
  });

  it('asks for the criteria list on every entry, as S11 of the current draft', async () => {
    await setUp(vi.fn().mockResolvedValue(criteriaAnswer()));

    expect(api.getCriteria).toHaveBeenCalledTimes(1);
    expect(api.getCriteria.mock.calls[0]?.[2]).toEqual(['not-found']);
  });

  it('INV:CAT-02 names the matched rule with the catalog names of the draft', async () => {
    const answer = criteriaAnswer();
    const first = answer.criteria[0];
    if (first === undefined) throw new Error('fixture has criteria');
    first.resolvedPriority.matchedRule = { kind: 'appType', appType: 'web', useCase: null };
    const { fixture } = await setUp(vi.fn().mockResolvedValue(answer));
    const store = fixture.debugElement.injector.get<InstanceType<typeof ConfiguratorTestStore>>(ProcureAccessStore);
    await store.loadFilterOptions();
    await fixture.whenStable();

    screen.getAllByRole('button', { name: 'Details anzeigen' })[0]?.click();
    await fixture.whenStable();

    expect(screen.getByText('Diese Empfehlung gilt für Web, unabhängig vom Verwendungszweck.')).toBeTruthy();
  });

  it('opens the legacy filter when no release is active (404, O-S4-3)', async () => {
    const { fixture } = await setUp(
      vi.fn().mockRejectedValue(new HttpErrorResponse({ status: 404, error: { type: 'x/problems/not-found' } })),
    );
    await fixture.whenStable();

    expect(fixture.debugElement.injector.get(Router).url).toBe('/filters');
  });

  it('stays where the user went when the 404 answers after they left S11', async () => {
    let answer404: (error: unknown) => void = () => undefined;
    const pending = new Promise((_, reject) => (answer404 = reject));
    const { fixture } = await setUp(vi.fn().mockReturnValue(pending));
    const router = fixture.debugElement.injector.get(Router);

    fixture.destroy();
    answer404(new HttpErrorResponse({ status: 404, error: { type: 'x/problems/not-found' } }));
    await pending.catch(() => undefined);
    await new Promise((resolve) => setTimeout(resolve));

    expect(router.url).toBe('/');
  });

  it('keeps H1 and „Auswahl ändern“ and shows no list when loading failed (O-S4-34)', async () => {
    const { fixture } = await setUp(vi.fn().mockRejectedValue(new HttpErrorResponse({ status: 400 })));

    expect(screen.getByRole('heading', { level: 1, name: 'Ihre Kriterienliste' })).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Auswahl ändern' })).toBeTruthy();
    expect(screen.queryByRole('list')).toBeNull();
    expect(fixture.debugElement.injector.get(Router).url).toBe('/');
  });

  it('keeps the user’s changes of a card in the draft; an excluded card stays in the list (R-LST-03 – R-LST-05)', async () => {
    const { fixture } = await setUp(vi.fn().mockResolvedValue(criteriaAnswer()));
    const store = fixture.debugElement.injector.get<InstanceType<typeof ConfiguratorTestStore>>(ProcureAccessStore);
    const act = async (element: HTMLElement) => {
      element.click();
      await fixture.whenStable();
    };
    const card = () => screen.getAllByRole('listitem')[0];

    await act(within(card()).getByRole('checkbox', { name: /^In Liste aufnehmen Bilder/ }));
    await act(within(card()).getByRole('button', { name: /^Priorität:/ }));
    await act(screen.getByRole('menuitemradio', { name: 'Hinweis' }));
    await act(within(card()).getByRole('button', { name: 'Notiz hinzufügen' }));
    const note = within(card()).getByRole('textbox', { name: 'Ihre Notiz zu diesem Kriterium (optional)' });
    (note as HTMLTextAreaElement).value = 'Nur für die Webanwendung.';
    fireEvent.input(note);
    await fixture.whenStable();

    expect(store.configurator.list.decisions()['11.1.1']).toEqual({
      included: false,
      overrideLevel: 'Info',
      overrideWeight: null,
      note: 'Nur für die Webanwendung.',
    });
    expect(screen.getAllByRole('listitem')).toHaveLength(4);
    expect(within(card()).getByText('Nicht in Ihrer Liste')).toBeTruthy();
    expect(within(card()).getByText('Von Ihnen geändert. Empfohlen war: A – Mindestanforderung.')).toBeTruthy();
    expect(within(card()).getByRole('button', { name: 'Notiz bearbeiten' })).toBeTruthy();
    await expectNoAxeViolations();

    await act(within(card()).getByRole('button', { name: 'Empfehlung wiederherstellen' }));

    expect(store.configurator.list.decisions()['11.1.1']?.overrideLevel).toBeNull();
    expect(within(card()).queryByText(/Von Ihnen geändert/)).toBeNull();
  });

  it('sets the weight of a recommended B in the draft', async () => {
    const { fixture } = await setUp(vi.fn().mockResolvedValue(criteriaAnswer()));
    const store = fixture.debugElement.injector.get<InstanceType<typeof ConfiguratorTestStore>>(ProcureAccessStore);
    const card = screen.getAllByRole('listitem')[1];

    within(card).getByRole('combobox', { name: 'Gewichtung' }).click();
    await fixture.whenStable();
    screen.getByRole('option', { name: '6' }).click();
    await fixture.whenStable();

    expect(store.configurator.list.decisions()['11.2.2']?.overrideWeight).toBe(6);
    expect(within(card).getByText('Von Ihnen geändert. Empfohlen war: B – Bewertungskriterium, Gewichtung 3.')).toBeTruthy();
  });

  describe('search and priority filter (R-LST-06)', () => {
    const titles = () =>
      within(screen.getByRole('list'))
        .getAllByRole('heading', { level: 2 })
        .map((heading) => heading.textContent?.trim());
    const resultAnnouncements = () =>
      (announce.mock.calls as [string, unknown, unknown][]).filter(([key]) => key.startsWith('common.results.shown'));

    async function typeSearch(fixture: { whenStable(): Promise<unknown> }, text: string) {
      const search = screen.getByRole<HTMLInputElement>('searchbox', { name: 'In Kriterien suchen' });
      search.value = text;
      fireEvent.input(search);
      await new Promise((resolve) => setTimeout(resolve, CRITERIA_SEARCH_DEBOUNCE_MS + 20));
      await fixture.whenStable();
    }

    it('shows the filter row above the cards and announces nothing on the first load', async () => {
      await setUp(vi.fn().mockResolvedValue(criteriaAnswer()));

      const filter = screen.getByRole('group', { name: 'Liste filtern' });
      expect(filter.compareDocumentPosition(screen.getByRole('list')) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
      expect(resultAnnouncements()).toEqual([]);
      await expectNoAxeViolations();
    });

    it('applies the search 300 ms after the last keystroke and announces the result once', async () => {
      const { fixture } = await setUp(vi.fn().mockResolvedValue(criteriaAnswer()));

      await typeSearch(fixture, 'tastatur');

      expect(titles()).toEqual(['Alles geht mit der Tastatur']);
      expect(resultAnnouncements()).toEqual([
        [
          'common.results.shown.one',
          { count: 1 },
          { throttleMs: 1000, throttleKey: 'common.results.shown' },
        ],
      ]);
    });

    it('filters by the levels checked and announces each change in its plural form', async () => {
      const { fixture } = await setUp(vi.fn().mockResolvedValue(criteriaAnswer()));

      screen.getByRole('checkbox', { name: 'B – Bewertungskriterium' }).click();
      await fixture.whenStable();

      expect(titles()).toEqual(['Alles geht mit der Tastatur', 'Geprüft nach BIT inklusiv Software-Test']);
      expect(resultAnnouncements().map(([key, params]) => [key, params])).toEqual([['common.results.shown', { count: 2 }]]);
    });

    it('hides excluded cards with „Nur aufgenommene Kriterien anzeigen“ (R-LST-04)', async () => {
      const { fixture } = await setUp(vi.fn().mockResolvedValue(criteriaAnswer()));
      within(screen.getAllByRole('listitem')[0]).getByRole('checkbox', { name: /^In Liste aufnehmen/ }).click();
      await fixture.whenStable();
      expect(resultAnnouncements()).toEqual([]);

      screen.getByRole('checkbox', { name: 'Nur aufgenommene Kriterien anzeigen' }).click();
      await fixture.whenStable();

      expect(titles()).toEqual([
        'Alles geht mit der Tastatur',
        'Überschriften',
        'Geprüft nach BIT inklusiv Software-Test',
      ]);
    });

    it('shows the empty state of the filter; „Filter zurücksetzen“ brings the list back with the focus in the search', async () => {
      const { fixture } = await setUp(vi.fn().mockResolvedValue(criteriaAnswer()));

      await typeSearch(fixture, 'gibt es nicht');

      expect(screen.queryByRole('list')).toBeNull();
      expect(screen.getByText('Keine Kriterien passen zu Ihrem Filter.')).toBeTruthy();
      expect(resultAnnouncements().at(-1)?.[0]).toBe('common.results.shown.zero');
      await expectNoAxeViolations();

      screen.getByRole('button', { name: 'Filter zurücksetzen' }).click();
      await fixture.whenStable();

      expect(screen.getAllByRole('listitem')).toHaveLength(4);
      expect(document.activeElement).toBe(screen.getByRole('searchbox', { name: 'In Kriterien suchen' }));
      expect((document.activeElement as HTMLInputElement).value).toBe('');
      expect(resultAnnouncements().at(-1)?.slice(0, 2)).toEqual(['common.results.shown', { count: 4 }]);
    });
  });

  describe('„Geprüft nach …“ cards (R-LST-07)', () => {
    it('INV:CAT-16 lists the procedure criterion with priority B and lets the user deselect it; it stays in the list', async () => {
      const { fixture } = await setUp(vi.fn().mockResolvedValue(criteriaAnswer()));
      const store = fixture.debugElement.injector.get<InstanceType<typeof ConfiguratorTestStore>>(ProcureAccessStore);
      const card = () => screen.getAllByRole('listitem')[3];

      expect(within(card()).getByText('B – Bewertungskriterium')).toBeTruthy();
      within(card()).getByRole('checkbox', { name: 'In Liste aufnehmen Geprüft nach BIT inklusiv Software-Test' }).click();
      await fixture.whenStable();

      expect(store.configurator.list.decisions()['PROC-softwareTest']?.included).toBe(false);
      expect(screen.getAllByRole('listitem')).toHaveLength(4);
      expect(within(card()).getByText('Nicht in Ihrer Liste')).toBeTruthy();

      within(card()).getByRole('button', { name: 'Details anzeigen' }).click();
      await fixture.whenStable();
      expect(within(card()).getByRole('heading', { level: 3, name: 'Warum diese Priorität?' })).toBeTruthy();
      expect(within(card()).getByText(/Im Abgleich mit Kriterien zählt dieses Kriterium nicht mit/)).toBeTruthy();
    });
  });

  describe('app types without criteria (R-LST-08)', () => {
    // The priority note (R-LST-09) is a note too; this one names the app types.
    const noCriteriaNotice = () => screen.queryAllByRole('note').find((note) => note.textContent?.includes('noch keine Kriterien')) ?? null;
    const hardware = { id: 'hardware', name: 'Hardware' };
    const relay = { id: 'relay', name: 'Vermittlungs- oder Notrufdienst' };

    it('INV:CAT-20 shows the info notice above the list, not an error, with „Auswahl ändern“ to step 2', async () => {
      const answer = { ...criteriaAnswer(), appTypesWithoutCriteria: [hardware, relay] };
      const { fixture } = await setUp(vi.fn().mockResolvedValue(answer));

      const notice = noCriteriaNotice();
      if (notice === null) throw new Error('notice shown');
      expect(notice.textContent).toContain(
        'Für Hardware und Vermittlungs- oder Notrufdienst gibt es noch keine Kriterien, weil BIT inklusiv dafür noch kein Prüfverfahren enthält.',
      );
      expect(screen.queryByRole('alert')).toBeNull();
      expect(notice.compareDocumentPosition(screen.getByRole('list')) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
      expect(screen.getAllByRole('listitem')).toHaveLength(4);
      await expectNoAxeViolations();

      within(notice).getByRole('link', { name: 'Auswahl ändern' }).click();
      await fixture.whenStable();

      const router = fixture.debugElement.injector.get(Router);
      expect(router.url).toBe('/kriterien');
      const state = fixture.debugElement.injector.get(Location).getState() as { configuratorStep?: string } | null;
      expect(state?.configuratorStep).toBe('appType');
    });

    it('INV:CAT-20 shows the notice as the empty state without filter row when no criterion is left', async () => {
      const answer = { ...criteriaAnswer(), criteria: [], appTypesWithoutCriteria: [hardware] };
      await setUp(vi.fn().mockResolvedValue(answer));

      expect(noCriteriaNotice()?.textContent).toContain('Für Hardware gibt es noch keine Kriterien');
      expect(screen.queryByRole('group', { name: 'Liste filtern' })).toBeNull();
      expect(screen.queryByRole('list')).toBeNull();
      expect(screen.queryByText(/Das sollte nicht vorkommen/)).toBeNull();
      await expectNoAxeViolations();
    });

    it('shows „criteria.list.empty“ for an empty answer without such app types', async () => {
      await setUp(vi.fn().mockResolvedValue({ ...criteriaAnswer(), criteria: [] }));

      expect(screen.getByText(/Für diese Auswahl gibt es keine Kriterien. Das sollte nicht vorkommen./)).toBeTruthy();
      expect(noCriteriaNotice()).toBeNull();
      expect(screen.queryByRole('group', { name: 'Liste filtern' })).toBeNull();
    });
  });

  describe('summary, notices, suggestion link and action bar (S4-18a, R-LST-09, R-LST-10, R-LST-13)', () => {
    const order = (...elements: HTMLElement[]) =>
      elements.every(
        (element, i) => i === 0 || (elements[i - 1].compareDocumentPosition(element) & Node.DOCUMENT_POSITION_FOLLOWING) !== 0,
      );

    it('shows summary, both notices and the counts above the filter row, the link and the action bar after the list', async () => {
      await setUp(vi.fn().mockResolvedValue(criteriaAnswer()));

      const summary = screen.getByText('3 Kriterien für Ihre Auswahl');
      const [priorityNote] = screen.getAllByRole('note');
      const draftNote = screen.getByText(/Ihre Auswahl wird nicht gespeichert, solange Sie nicht angemeldet sind/);
      const counts = screen.getByText('1 Mindestanforderung, 1 Bewertungskriterium, 1 Hinweis');
      const filter = screen.getByRole('group', { name: 'Liste filtern' });
      const suggest = screen.getByRole('link', { name: /^Fehlt ein Kriterium oder ist eine Formulierung unklar\?/ });
      const status = screen.getByText('4 von 4 Kriterien in Ihrer Liste');
      const exportButton = screen.getByRole('button', { name: 'Liste exportieren' });

      expect(priorityNote.textContent).toContain('Die Prioritäten sind eine Empfehlung des Projektteams');
      expect(
        order(summary, priorityNote, draftNote, counts, filter, screen.getByRole('list'), suggest, status, exportButton),
      ).toBe(true);
      expect(screen.queryByRole('button', { name: 'Liste speichern' })).toBeNull();
      expect(screen.queryByText('Noch nicht gespeichert')).toBeNull();
      expect(screen.queryByRole('button', { name: /schließen|ausblenden/i })).toBeNull();
      await expectNoAxeViolations();
    });

    it('shows no draft note for a signed-in user', async () => {
      await setUp(vi.fn().mockResolvedValue(criteriaAnswer()), { signedIn: true });

      expect(screen.getByText(/Die Prioritäten sind eine Empfehlung des Projektteams/)).toBeTruthy();
      expect(screen.queryByText(/Ihre Auswahl wird nicht gespeichert/)).toBeNull();
    });

    it('opens the configured suggestion channel with the mail subject and names where it leads (R-LST-13)', async () => {
      await setUp(vi.fn().mockResolvedValue(criteriaAnswer()));

      const link = screen.getByRole('link', { name: /^Fehlt ein Kriterium/ });
      expect(link.textContent?.replace(/\s+/g, ' ').trim()).toBe(
        'Fehlt ein Kriterium oder ist eine Formulierung unklar? Schlagen Sie eine Änderung vor (öffnet Ihr E-Mail-Programm)',
      );
      expect(link.getAttribute('href')).toBe(
        'mailto:kriterium-vorschlagen@example.invalid?subject=Vorschlag%20f%C3%BCr%20den%20Kriterienkatalog',
      );
    });

    it('counts the level that applies now and announces the live counter only after the user’s change', async () => {
      const { fixture } = await setUp(vi.fn().mockResolvedValue(criteriaAnswer()));
      const card = () => screen.getAllByRole('listitem')[0];
      expect(announce).not.toHaveBeenCalled();

      within(card()).getByRole('checkbox', { name: /^In Liste aufnehmen Bilder/ }).click();
      await fixture.whenStable();

      expect(screen.getByText('3 von 4 Kriterien in Ihrer Liste')).toBeTruthy();
      expect(announce).toHaveBeenCalledWith('criteria.list.status.count', { included: 3, total: 4 }, { throttleMs: 1000 });
      // Excluded criteria keep their level in the counts (owner decision O-S4-51 (a)).
      expect(screen.getByText('1 Mindestanforderung, 1 Bewertungskriterium, 1 Hinweis')).toBeTruthy();

      within(card()).getByRole('button', { name: /^Priorität:/ }).click();
      await fixture.whenStable();
      screen.getByRole('menuitemradio', { name: 'Hinweis' }).click();
      await fixture.whenStable();

      expect(screen.getByText('0 Mindestanforderungen, 1 Bewertungskriterium, 2 Hinweise')).toBeTruthy();
      expect(announce.mock.calls.filter(([key]) => key === 'criteria.list.status.count')).toHaveLength(1);
    });

    it('leaves the action bar out when the list has no card', async () => {
      await setUp(vi.fn().mockResolvedValue({ ...criteriaAnswer(), criteria: [] }));

      expect(screen.queryByRole('button', { name: 'Liste exportieren' })).toBeNull();
      expect(screen.getByRole('link', { name: /^Fehlt ein Kriterium/ })).toBeTruthy();
    });

    it('opens the export dialog S13 with the format choice; „Abbrechen“ closes it', async () => {
      const { fixture } = await setUp(vi.fn().mockResolvedValue(criteriaAnswer()));

      screen.getByRole('button', { name: 'Liste exportieren' }).click();
      await fixture.whenStable();

      const dialog = screen.getByRole('dialog', { name: 'Kriterienliste exportieren' });
      expect(within(dialog).getByRole('radio', { name: 'Excel-Datei (.xlsx) – für Vergabeunterlagen' })).toBeTruthy();
      within(dialog).getByRole('button', { name: 'Abbrechen' }).click();
      await fixture.whenStable();

      expect(screen.queryByRole('dialog')).toBeNull();
    });
  });
});
