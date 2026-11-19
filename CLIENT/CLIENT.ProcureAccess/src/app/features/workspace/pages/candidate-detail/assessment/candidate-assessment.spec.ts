// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { provideRouter } from '@angular/router';
import { signalStore, withMethods } from '@ngrx/signals';
import { fireEvent } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/angular';

import { API_URL } from '@app/app.config';
import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { OrganizationRole } from '@app/features/organization/models/organization';
import { AssessmentLegendSetting, SYSTEM_PREFERENCES, UserPreferences } from '@app/features/settings/models/user-preferences';
import { withLoading } from '@app/shared/state/with-loading';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { provideLayout } from '@app/testing/shell-test-doubles';

import { CandidateAssessments } from '../../../models/assessments';
import { withWorkspace } from '../../../state/with-workspace';
import { bodyOf, candidateView, TEST_ASSESSMENTS, TEST_CANDIDATE, TEST_PROJECT } from '../../../testing/candidate-fixtures';
import { CandidateLoad, CandidateView } from '../candidate-load';
import { CandidateAssessment } from './candidate-assessment';

const role = signal<OrganizationRole>('editor');
const preferences = signal<UserPreferences | null>(SYSTEM_PREFERENCES);
const rememberAssessmentLegend = vi.fn<(state: AssessmentLegendSetting) => void>();

/**
 * The root store as S34b reads it: the real workspace slice, the user's role in `org-1` and the
 * user's settings with the legend state (`withSettings()`, S6-19b).
 */
const AssessmentTestStore = signalStore(
  withLoading(),
  withWorkspace(),
  withMethods(() => ({
    organizations: () => [{ id: 'org-1', name: 'Personalamt', type: 'publicBody' as const, role: role() }],
    preferences: () => preferences(),
    rememberAssessmentLegend: (state: AssessmentLegendSetting) => rememberAssessmentLegend(state),
  })),
);

const URL = '/api/v1/candidates/c-1/assessments';
const TITLES = [
  'Formulare lassen sich vollständig mit der Tastatur bedienen',
  'Inhalte bleiben bei 200 % Vergrößerung vollständig nutzbar',
  'Fehlermeldungen werden dem betroffenen Eingabefeld zugeordnet und vorgelesen',
];

/** Longer than the 800 ms of the autosave. */
const afterAutosave = () => new Promise((resolve) => setTimeout(resolve, 850));

describe('CandidateAssessment (S34b „Einschätzung nach Kriterien“, R-CAN-06, R-CAN-07)', () => {
  let backend: HttpTestingController;
  let announce: ReturnType<typeof vi.fn>;
  let setPageTitleState: ReturnType<typeof vi.fn>;
  let load: CandidateLoad;

  async function openTab(
    { view = candidateView(), wide = true, answer = TEST_ASSESSMENTS }: { view?: CandidateView; wide?: boolean; answer?: CandidateAssessments | null } = {},
  ) {
    announce = vi.fn().mockResolvedValue(undefined);
    setPageTitleState = vi.fn();
    load = new CandidateLoad();
    load.view.set(view);
    const rendered = await render(CandidateAssessment, {
      providers: [
        provideTestTranslations(),
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        provideLayout(wide),
        { provide: API_URL, useValue: '/api' },
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        { provide: CandidateLoad, useValue: load },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: PageTitleStrategy, useValue: { setPageTitleState } },
        { provide: ProcureAccessStore, useClass: AssessmentTestStore },
      ],
    });
    backend = TestBed.inject(HttpTestingController);
    if (answer !== null) {
      backend.expectOne(URL).flush(answer);
      await rendered.fixture.whenStable();
    }
    return rendered;
  }

  beforeEach(() => {
    role.set('editor');
    preferences.set(SYSTEM_PREFERENCES);
    rememberAssessmentLegend.mockReset();
  });
  afterEach(() => backend.verify());

  it('shows the intro, one K6 per requirement criterion with its allowed levels, and the derived rows (INV:CAT-15)', async () => {
    await openTab();

    expect(screen.getByText(/^Schätzen Sie für jedes Kriterium ein, wie gut das Produkt es erfüllt\./)).toBeTruthy();
    const groups = screen.getAllByRole('group');
    // The plain-language title, the title until there is one
    expect(groups.map((group) => group.querySelector('legend')?.textContent?.trim())).toEqual(TITLES);
    expect(within(groups[0]).queryByRole('radio', { name: 'Nicht anwendbar' })).toBeNull();
    expect(within(groups[1]).getByRole('radio', { name: 'Nicht anwendbar' })).toBeTruthy();
    expect(within(groups[0]).getByRole<HTMLInputElement>('radio', { name: 'Erfüllt' }).checked).toBe(true);
    expect(within(groups[1]).getByRole<HTMLInputElement>('radio', { name: 'Einschränkung' }).checked).toBe(true);
    expect(within(groups[2]).getByRole<HTMLInputElement>('radio', { name: 'Nicht eingeschätzt' }).checked).toBe(true);
    expect(within(groups[0]).getByRole<HTMLTextAreaElement>('textbox').value).toBe('Eigener Test mit der Tastatur.');
    expect(screen.queryByText(/Ihre Liste gilt für mehrere Anwendungstypen/)).toBeNull();
    expect(setPageTitleState).toHaveBeenCalledWith({
      params: { tab: 'Einschätzung nach Kriterien', product: 'BewerberPro', project: 'Neues Bewerbermanagement 2027' },
    });
    await expectNoAxeViolations();
  });

  it('shows each „Geprüft nach …“ criterion read only with its derived value, the hint and a link to S34a (INV:CAT-17, R-CAN-07)', async () => {
    await openTab();

    const title = screen.getByText('Geprüft nach BIT inklusiv Software-Test');
    const row = title.parentElement!;
    expect(within(row).getByText('Ja')).toBeTruthy();
    expect(within(row).getByText('Ergibt sich aus dem Prüfnachweis. Zählt im Abgleich mit Kriterien nicht mit.')).toBeTruthy();
    expect(within(row).queryByRole('radio')).toBeNull();
    expect(within(row).queryByRole('textbox')).toBeNull();
    const link = within(row).getByRole('link', { name: 'Prüfnachweis bearbeiten' });
    expect(link.getAttribute('href')).toBe('/projekte/p-1/produkte/c-1/pruefnachweis');
    expect(link.getAttribute('aria-describedby')).toBe(title.id);

    const second = screen.getByText('Geprüft nach BIK BITV-Test (Web)').parentElement!;
    expect(within(second).getByText('Nicht angegeben')).toBeTruthy();
    // Procedure rows follow the requirement criteria and are no K6
    expect(screen.getAllByRole('group')).toHaveLength(3);
  });

  it('shows the compact K6 below 600 px (Handoff §2.2, S34b-M)', async () => {
    await openTab({ wide: false });

    expect(screen.queryByRole('radiogroup')).toBeNull();
    const selects = screen.getAllByRole('combobox', { name: 'Einschätzung' });
    expect(selects).toHaveLength(3);
    expect(selects[1].textContent).toContain('Einschränkung');
    expect(screen.getByRole('button', { name: 'Begründung bearbeiten' })).toBeTruthy();
    expect(screen.getAllByRole('button', { name: 'Begründung hinzufügen' })).toHaveLength(2);
    await expectNoAxeViolations();
  });

  it('shows the union note once above the list when several procedures set the levels (R-CAN-06)', async () => {
    await openTab({ answer: { ...TEST_ASSESSMENTS, ratingsFromSeveralProcedures: true } });

    expect(screen.getAllByText(/^Ihre Liste gilt für mehrere Anwendungstypen\./)).toHaveLength(1);
  });

  it('autosaves a chosen level as the whole row 800 ms later and announces it (R-CAN-06)', async () => {
    const { fixture } = await openTab();

    screen.getAllByRole('radio', { name: 'Barriere' })[2].click();
    await fixture.whenStable();
    backend.expectNone(`${URL}/13`);
    expect(screen.getAllByRole<HTMLInputElement>('radio', { name: 'Barriere' })[2].checked).toBe(true);

    await afterAutosave();
    const put = backend.expectOne(`${URL}/13`);
    expect(put.request.method).toBe('PUT');
    expect(put.request.body).toEqual({ value: 'barrier', note: null });
    put.flush({ criterionRevisionId: 13, value: 'barrier', note: null });
    await new Promise((resolve) => setTimeout(resolve));
    expect(announce).toHaveBeenCalledWith('candidate.assessment.saved', undefined, { throttleMs: 5000 });
  });

  it('autosaves the note with the row’s level once typing paused', async () => {
    const { fixture } = await openTab();

    const note = within(screen.getAllByRole('group')[1]).getByRole('textbox');
    fireEvent.input(note, { target: { value: 'Ab 300 % ' } });
    fireEvent.input(note, { target: { value: 'Ab 300 % überlappen Texte.' } });
    await fixture.whenStable();

    await afterAutosave();
    const put = backend.expectOne(`${URL}/12`);
    expect(put.request.body).toEqual({ value: 'restriction', note: 'Ab 300 % überlappen Texte.' });
    put.flush({});
  });

  it('saves a change at once when the tab is left, then reads progress and counts again for Überblick, keeping the candidate’s tag', async () => {
    const { fixture } = await openTab();

    screen.getAllByRole('radio', { name: 'Blockade' })[2].click();
    await fixture.whenStable();
    fixture.destroy();

    await Promise.resolve();
    backend.expectOne(`${URL}/13`).flush({});
    await new Promise((resolve) => setTimeout(resolve));
    const read = backend.expectOne('/api/v1/candidates/c-1');
    read.flush(
      { ...bodyOf(TEST_CANDIDATE), name: 'Umbenannt', progress: { done: 37, total: 46 }, fulfillment: { ...TEST_CANDIDATE.fulfillment!, blockade: 2, unknown: 9 } },
      { headers: { ETag: '"1-9"' } },
    );
    await new Promise((resolve) => setTimeout(resolve));

    const view = load.view();
    expect(view.kind === 'candidate' && view.candidate).toEqual({
      ...TEST_CANDIDATE,
      progress: { done: 37, total: 46 },
      fulfillment: { ...TEST_CANDIDATE.fulfillment!, blockade: 2, unknown: 9 },
    });
  });

  it('shows the S80 server text in place when the assessments cannot be read', async () => {
    const { fixture } = await openTab({ answer: null });
    backend.expectOne(URL).flush(null, { status: 500, statusText: 'Server Error' });
    await fixture.whenStable();

    expect(screen.getByText('Etwas ist schiefgelaufen')).toBeTruthy();
    expect(screen.queryByRole('group')).toBeNull();
  });

  it('without a list explains that products are assessed by a list, and reads nothing', async () => {
    await openTab({ view: candidateView({ project: { ...TEST_PROJECT, criteriaList: null } }), answer: null });

    expect(screen.getByText(/^Diesem Projekt ist noch keine Kriterienliste zugeordnet\./)).toBeTruthy();
    backend.expectNone(URL);
  });

  it('shows nothing to a Reader until S6-22 (owner decision S6-19 Q3 (a))', async () => {
    role.set('reader');
    const { fixture } = await openTab({ answer: null });

    backend.expectNone(URL);
    expect((fixture.nativeElement as HTMLElement).textContent?.trim()).toBe('');
  });

  it('shows nothing in an archived project (R-PRJ-05)', async () => {
    role.set('owner');
    const { fixture } = await openTab({ view: candidateView({ project: { ...TEST_PROJECT, status: 'archived' } }), answer: null });

    backend.expectNone(URL);
    expect((fixture.nativeElement as HTMLElement).textContent?.trim()).toBe('');
  });

  describe('progress, filter and legend (R-CAN-08, S6-19b)', () => {
    const legendButton = () => screen.getByRole('button', { name: /Bewertungsstufen/ });

    /** Sends the saves still waiting for their 800 ms and answers them, so none fires after the test. */
    async function settleSaves(): Promise<void> {
      const flushed = (TestBed.inject(ProcureAccessStore) as unknown as { flushAssessments(): Promise<void> }).flushAssessments();
      await new Promise((resolve) => setTimeout(resolve));
      backend.match(() => true).forEach((request) => request.flush({}));
      await flushed;
    }

    it('shows the progress of the requirement criteria and the list with its release; the derived rows are not counted', async () => {
      await openTab();

      expect(screen.getByText('2 von 3 Kriterien eingeschätzt')).toBeTruthy();
      expect(screen.getByText('Kriterienliste: HR-Software 2027 (Katalogversion T.1)')).toBeTruthy();
      await expectNoAxeViolations();
    });

    it('announces the progress when a choice changes it, at most every 5 s, and not for a note', async () => {
      const { fixture } = await openTab();

      fireEvent.input(within(screen.getAllByRole('group')[0]).getByRole('textbox'), { target: { value: 'Neu' } });
      await fixture.whenStable();
      expect(announce).not.toHaveBeenCalledWith('candidate.assessment.progress', expect.anything(), expect.anything());

      screen.getAllByRole('radio', { name: 'Barriere' })[2].click();
      await fixture.whenStable();

      expect(screen.getByText('3 von 3 Kriterien eingeschätzt')).toBeTruthy();
      expect(announce).toHaveBeenCalledWith('candidate.assessment.progress', { done: 3, total: 3 }, { throttleMs: 5000 });
      await settleSaves();
    });

    it('filters to the criteria not assessed when it is ticked, announces the count and keeps a row assessed meanwhile', async () => {
      const { fixture } = await openTab();
      const filter = screen.getByRole('checkbox', { name: 'Nur noch nicht eingeschätzte anzeigen' });

      filter.click();
      await fixture.whenStable();

      const groups = screen.getAllByRole('group');
      expect(groups.map((group) => group.querySelector('legend')?.textContent?.trim())).toEqual([TITLES[2]]);
      expect(screen.queryByText('Geprüft nach BIT inklusiv Software-Test')).toBeNull();
      expect(announce).toHaveBeenCalledWith('common.results.shown.one', { count: 1 }, { throttleMs: 1000, throttleKey: 'common.results.shown' });

      // Assessed while the filter is on: the row stays under the focus until the filter changes
      within(groups[0]).getByRole('radio', { name: 'Erfüllt' }).click();
      await fixture.whenStable();
      expect(screen.getAllByRole('group')).toHaveLength(1);
      await expectNoAxeViolations();

      filter.click();
      await fixture.whenStable();
      expect(screen.getAllByRole('group')).toHaveLength(3);
      expect(screen.getByText('Geprüft nach BIT inklusiv Software-Test')).toBeTruthy();

      filter.click();
      await fixture.whenStable();
      expect(screen.queryByRole('group')).toBeNull();
      expect(screen.getByText('Keine Kriterien passen zu Ihrem Filter.')).toBeTruthy();
      expect(announce).toHaveBeenLastCalledWith('common.results.shown.zero', { count: 0 }, { throttleMs: 1000, throttleKey: 'common.results.shown' });
      await settleSaves();
    });

    it('expands the legend from 600 px until the first assessment, with the levels of the list and „Nicht eingeschätzt“ (Handoff §4.11)', async () => {
      await openTab();

      expect(legendButton().textContent?.trim()).toBe('Erklärung der Bewertungsstufen ausblenden');
      expect(legendButton().getAttribute('aria-expanded')).toBe('true');
      const panel = document.getElementById(legendButton().getAttribute('aria-controls') ?? '')!;
      expect([...panel.querySelectorAll('dt')].map((term) => term.textContent?.trim())).toEqual([
        'Erfüllt',
        'Leichte Einschränkung',
        'Einschränkung',
        'Barriere',
        'Blockade',
        'Nicht anwendbar',
        'Nicht eingeschätzt',
      ]);
      // Above the list, after the progress row
      expect(panel.compareDocumentPosition(screen.getAllByRole('group')[0]) & Node.DOCUMENT_POSITION_FOLLOWING).toBeTruthy();
    });

    it('stores „collapsed“ at the first assessment, but keeps the legend open in this visit (owner decision S6-19b (a))', async () => {
      const { fixture } = await openTab();

      screen.getAllByRole('radio', { name: 'Blockade' })[2].click();
      preferences.set({ ...SYSTEM_PREFERENCES, assessmentLegend: 'collapsed' });
      await fixture.whenStable();

      expect(rememberAssessmentLegend).toHaveBeenCalledExactlyOnceWith('collapsed');
      expect(legendButton().getAttribute('aria-expanded')).toBe('true');
      await settleSaves();
    });

    it('stores what the user chooses by opening or closing the legend', async () => {
      const { fixture } = await openTab();

      legendButton().click();
      await fixture.whenStable();
      expect(legendButton().getAttribute('aria-expanded')).toBe('false');
      expect(rememberAssessmentLegend).toHaveBeenLastCalledWith('collapsed');

      legendButton().click();
      await fixture.whenStable();
      expect(rememberAssessmentLegend).toHaveBeenLastCalledWith('expanded');
    });

    it('starts collapsed after the user closed it or assessed before, and stores nothing at an assessment then', async () => {
      preferences.set({ ...SYSTEM_PREFERENCES, assessmentLegend: 'collapsed' });
      const { fixture } = await openTab();

      expect(legendButton().getAttribute('aria-expanded')).toBe('false');
      screen.getAllByRole('radio', { name: 'Blockade' })[2].click();
      await fixture.whenStable();
      expect(rememberAssessmentLegend).not.toHaveBeenCalled();
      await settleSaves();
    });

    it('starts collapsed below 600 px unless the user chose to see it (Handoff §2.2)', async () => {
      await openTab({ wide: false });
      expect(legendButton().getAttribute('aria-expanded')).toBe('false');
    });

    it('starts expanded below 600 px when the user chose so', async () => {
      preferences.set({ ...SYSTEM_PREFERENCES, assessmentLegend: 'expanded' });
      await openTab({ wide: false });
      expect(legendButton().getAttribute('aria-expanded')).toBe('true');
    });
  });
});
