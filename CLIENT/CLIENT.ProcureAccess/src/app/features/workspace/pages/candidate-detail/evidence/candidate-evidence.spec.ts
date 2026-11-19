// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { fireEvent } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { OrganizationRole } from '@app/features/organization/models/organization';
import { ConfirmDialog } from '@app/shared/components/confirm-dialog/confirm-dialog';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';

import { Candidate, TestEvidence } from '../../../models/candidates';
import { CandidatesApiService } from '../../../services/api/candidates-api.service';
import { candidateView, TEST_CANDIDATE, TEST_PROJECT } from '../../../testing/candidate-fixtures';
import { CandidateLoad, CandidateView } from '../candidate-load';
import { CandidateEvidence } from './candidate-evidence';

const base = 'https://procureaccess.de/problems/';

function problem(status: number, body: object): HttpErrorResponse {
  return new HttpErrorResponse({ status, error: body });
}

const SOFTWARE_TEST: TestEvidence = {
  id: 'e-1',
  etag: '"1-3"',
  testKind: 'procedure',
  testTypeId: 'softwareTest',
  otherTestTypeName: null,
  testedBy: 'Prüfstelle Beispiel',
  testDate: '2026-03-14',
  testedVersion: '4.2',
  evidenceUrl: 'https://pruefstelle-beispiel.de/berichte/bewerberpro',
  qualificationNote: 'Akkreditierte Prüfstelle',
};

const TESTED: Candidate = { ...TEST_CANDIDATE, testStatus: 'tested', testEvidences: [SOFTWARE_TEST] };

describe('CandidateEvidence (S34a, R-CAN-05)', () => {
  let api: Record<'get' | 'changeTestStatus' | 'addEvidence' | 'changeEvidence' | 'removeEvidence', ReturnType<typeof vi.fn>>;
  let announce: ReturnType<typeof vi.fn>;
  let setPageTitleState: ReturnType<typeof vi.fn>;
  let focusPageHeading: ReturnType<typeof vi.fn>;
  let load: CandidateLoad;

  async function openTab(view: CandidateView = candidateView(), role: OrganizationRole = 'editor') {
    const shown = view.kind === 'candidate' ? view.candidate : null;
    api = {
      get: vi.fn().mockResolvedValue(shown),
      changeTestStatus: vi.fn().mockResolvedValue(shown),
      addEvidence: vi.fn().mockImplementation((_id: string, body: object) => Promise.resolve({ id: 'e-new', ...body })),
      changeEvidence: vi.fn().mockImplementation((id: string, body: object) => Promise.resolve({ id, ...body })),
      removeEvidence: vi.fn().mockResolvedValue(undefined),
    };
    announce = vi.fn().mockResolvedValue(undefined);
    setPageTitleState = vi.fn();
    focusPageHeading = vi.fn();
    load = new CandidateLoad();
    load.view.set(view);
    const rendered = await render(CandidateEvidence, {
      routes: [{ path: '**', children: [] }],
      providers: [
        provideTestTranslations(),
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        { provide: CandidateLoad, useValue: load },
        { provide: CandidatesApiService, useValue: api },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: PageTitleStrategy, useValue: { setPageTitleState, focusPageHeading } },
        {
          provide: ProcureAccessStore,
          useValue: {
            organizations: signal([{ id: 'org-1', name: 'Personalamt', type: 'publicBody', role }]),
            incrementLoadingCount: vi.fn(),
            decrementLoadingCount: vi.fn(),
          },
        },
      ],
    });
    await rendered.fixture.whenStable();
    return rendered;
  }

  const textbox = (name: string | RegExp, index = 0) => screen.getAllByRole<HTMLInputElement>('textbox', { name })[index];
  const type = (name: string | RegExp, value: string, index = 0) => fireEvent.input(textbox(name, index), { target: { value } });
  const radio = (name: string) => screen.getByRole<HTMLInputElement>('radio', { name });

  async function settle(fixture: { whenStable(): Promise<unknown> }) {
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  async function choose(fixture: { whenStable(): Promise<unknown> }, status: string) {
    radio(status).click();
    await settle(fixture);
  }

  async function chooseKind(fixture: { whenStable(): Promise<unknown> }, option: string, index = 0) {
    screen.getAllByRole('combobox', { name: 'Art der Prüfung' })[index].click();
    await fixture.whenStable();
    // „Andere Prüfung“ has a second line in its name: match its start, without building a RegExp
    screen.getByRole('option', { name: (name) => name.startsWith(option) }).click();
    await settle(fixture);
  }

  async function save(fixture: { whenStable(): Promise<unknown> }) {
    screen.getByRole('button', { name: 'Speichern' }).click();
    await settle(fixture);
  }

  /** Fills the required fields of the card at `index` (not the kind). */
  function fillCard(index = 0) {
    type('Wer hat geprüft?', 'Prüfstelle Beispiel', index);
    type('Datum der Prüfung', '14.03.2026', index);
  }

  afterEach(() => vi.restoreAllMocks());

  it('shows the radio group „Nicht bekannt“ and the details hint, no evidence card; the page title names the tab (R-CAN-03, R-CAN-05)', async () => {
    const { container } = await openTab();

    const group = screen.getByRole('group', { name: 'Wurde das Produkt auf Barrierefreiheit geprüft?' });
    expect(group).toBeTruthy();
    expect(screen.getAllByRole('radio').map((option) => option.closest('mat-radio-button')?.textContent?.trim())).toEqual([
      'Ja',
      'Nein',
      'Nicht bekannt',
    ]);
    expect(radio('Nicht bekannt').checked).toBe(true);
    expect(screen.getByRole('radiogroup').getAttribute('aria-describedby')).toBe('pa-evidence-status-hint');
    expect(screen.getByText(/^Angaben zu Art, Stelle und Datum der Prüfung/)).toBeTruthy();
    expect(screen.queryByRole('heading', { level: 2 })).toBeNull();
    expect(setPageTitleState).toHaveBeenLastCalledWith({
      params: { tab: 'Prüfnachweis', product: 'BewerberPro', project: TEST_PROJECT.name },
      error: false,
    });
    await expectNoAxeViolations(container);
  });

  it('INV:WSP-01 choosing „Ja“ shows one card with its fields in fixed order after the group; an empty card is not sent (D4 (b))', async () => {
    const { container, fixture } = await openTab();
    await choose(fixture, 'Ja');

    expect(screen.getByRole('heading', { level: 2, name: 'Prüfnachweise' })).toBeTruthy();
    expect(screen.getByRole('heading', { level: 3, name: 'Prüfnachweis 1' })).toBeTruthy();
    expect(screen.queryByText(/^Angaben zu Art, Stelle und Datum der Prüfung/)).toBeNull();
    const card = screen.getByRole('heading', { level: 3 }).parentElement!;
    expect([...card.querySelectorAll('mat-label, label.pa-evidence__label')].map((label) => label.textContent?.trim())).toEqual([
      'Art der Prüfung',
      'Wer hat geprüft?',
      'Datum der Prüfung',
      'Geprüfte Version (optional)',
      'Link zum Prüfbericht (optional)',
      'Ihre Notiz zur Qualifikation der prüfenden Stelle (optional)',
    ]);
    // Too long to float at 320 px: the label stands above its field and still names it (D7 (a))
    const note = screen.getByRole('textbox', { name: 'Ihre Notiz zur Qualifikation der prüfenden Stelle (optional)' });
    const noteLabel = card.querySelector('label.pa-evidence__label')!;
    expect(noteLabel.closest('mat-form-field')).toBeNull();
    expect(noteLabel.getAttribute('for')).toBe(note.id);
    expect(screen.getByText(/^Nur für Ihre Organisation: Diese Angaben werden nie veröffentlicht/)).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Prüfnachweis 1 entfernen' })).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Weiteren Prüfnachweis hinzufügen' })).toBeTruthy();
    await expectNoAxeViolations(container);

    await save(fixture);
    expect(api.changeTestStatus).toHaveBeenCalledWith('c-1', 'tested', false, TEST_CANDIDATE.etag);
    expect(api.addEvidence).not.toHaveBeenCalled();
    expect(announce).toHaveBeenCalledWith('candidate.evidence.saved');
    expect(screen.queryByRole('alert')).toBeNull();
  });

  it('lists the options of „Art der Prüfung“: the five procedures, „Andere Prüfung“ with its hint, „Eigene Prüfung“; „Andere Prüfung“ requires a name', async () => {
    const { fixture } = await openTab();
    await choose(fixture, 'Ja');

    screen.getByRole('combobox', { name: 'Art der Prüfung' }).click();
    await fixture.whenStable();
    // The two lines of „Andere Prüfung“ are blocks: browsers read them with a space between
    const lines = (option: Element) => [...option.querySelectorAll('.mdc-list-item__primary-text > span')].map((span) => span.textContent?.trim());
    expect(screen.getAllByRole('option').map((option) => (lines(option).length > 0 ? lines(option).join(' ') : option.textContent?.trim()))).toEqual([
      'BIT inklusiv Software-Test',
      'BIT inklusiv App-Test',
      'BIT inklusiv PDF-Test',
      'BIK BITV-Test (Web)',
      'BIK BITV-Test (App)',
      'Andere Prüfung Sie geben den Namen des Verfahrens an.',
      'Eigene Prüfung ohne festes Verfahren',
    ]);
    screen.getByRole('option', { name: /^Andere Prüfung/ }).click();
    await settle(fixture);

    // The name field follows the select directly
    const labels = [...document.querySelectorAll('.pa-evidence__fields mat-label')].map((label) => label.textContent?.trim());
    expect(labels.slice(0, 3)).toEqual(['Art der Prüfung', 'Name des Prüfverfahrens', 'Wer hat geprüft?']);
    fillCard();
    await save(fixture);

    expect(api.addEvidence).not.toHaveBeenCalled();
    expect(screen.getByRole('link', { name: 'Prüfnachweis 1 – Name des Prüfverfahrens: Bitte geben Sie Name des Prüfverfahrens an.' })).toBeTruthy();

    type('Name des Prüfverfahrens', 'Hausinterner Leitfaden');
    await settle(fixture);
    expect(screen.getByRole('heading', { level: 3, name: 'Prüfnachweis 1: Hausinterner Leitfaden' })).toBeTruthy();
    await save(fixture);
    // The status first, with the candidate's tag; then the new evidence, which needs none (S6-20)
    expect(api.changeTestStatus).toHaveBeenCalledWith('c-1', 'tested', false, TEST_CANDIDATE.etag);
    expect(api.changeTestStatus.mock.invocationCallOrder[0]).toBeLessThan(api.addEvidence.mock.invocationCallOrder[0]);
    expect(api.addEvidence).toHaveBeenCalledWith('c-1', {
      testKind: 'other',
      testTypeId: null,
      otherTestTypeName: 'Hausinterner Leitfaden',
      testedBy: 'Prüfstelle Beispiel',
      testDate: '2026-03-14',
      testedVersion: '',
      evidenceUrl: '',
      qualificationNote: '',
    });
  });

  it('INV:API-02 shows the errors of a card in the summary with its number, focuses it and marks the page title', async () => {
    const { container, fixture } = await openTab();
    await choose(fixture, 'Ja');
    type('Wer hat geprüft?', 'Prüfstelle');
    type('Datum der Prüfung', '31.02.2026');
    type(/^Link zum Prüfbericht/, 'http://example.org');
    await save(fixture);

    const summary = screen.getByRole('heading', { level: 2, name: /3 Angaben müssen noch korrigiert werden$/ });
    expect(summary).toBeTruthy();
    expect(screen.getAllByRole('link').map((link) => link.textContent?.trim())).toEqual([
      'Prüfnachweis 1 – Art der Prüfung: Bitte wählen Sie eine Option.',
      'Prüfnachweis 1 – Datum der Prüfung: Bitte geben Sie ein Datum im Format TT.MM.JJJJ ein.',
      'Prüfnachweis 1 – Link zum Prüfbericht (optional): Bitte geben Sie eine vollständige Webadresse ein, die mit https:// beginnt.',
    ]);
    expect(document.activeElement?.closest('pa-notice')).not.toBeNull();
    expect(setPageTitleState).toHaveBeenLastCalledWith(expect.objectContaining({ error: true }));
    expect(api.changeTestStatus).not.toHaveBeenCalled();
    await expectNoAxeViolations(container);
  });

  it('refuses a test date after today', async () => {
    const { fixture } = await openTab();
    await choose(fixture, 'Ja');
    await chooseKind(fixture, 'BIT inklusiv App-Test');
    type('Wer hat geprüft?', 'Prüfstelle');
    type('Datum der Prüfung', '01.01.2999');
    await save(fixture);

    expect(screen.getByRole('link', { name: 'Prüfnachweis 1 – Datum der Prüfung: Das Datum liegt in der Zukunft. Bitte prüfen Sie es.' })).toBeTruthy();
    expect(api.changeTestStatus).not.toHaveBeenCalled();
  });

  it('INV:WSP-01 a second card with the same procedure shows `candidate.evidence.list.duplicate` before sending', async () => {
    const { fixture } = await openTab(candidateView({ candidate: TESTED }));
    screen.getByRole('button', { name: 'Weiteren Prüfnachweis hinzufügen' }).click();
    await settle(fixture);
    expect(document.activeElement?.id).toBe(screen.getAllByRole('combobox', { name: 'Art der Prüfung' })[1].id);
    await chooseKind(fixture, 'BIT inklusiv Software-Test', 1);
    fillCard(1);
    await save(fixture);

    expect(screen.getByRole('link', { name: 'Prüfnachweis 2 – Art der Prüfung: Für dieses Prüfverfahren gibt es schon einen Prüfnachweis.' })).toBeTruthy();
    expect(api.addEvidence).not.toHaveBeenCalled();
  });

  it('INV:WSP-01 shows the server’s duplicate (400 at testTypeId) at the select of the refused card and keeps its input', async () => {
    const { fixture } = await openTab(candidateView({ candidate: TESTED }));
    screen.getByRole('button', { name: 'Weiteren Prüfnachweis hinzufügen' }).click();
    await settle(fixture);
    await chooseKind(fixture, 'BIK BITV-Test (Web)', 1);
    fillCard(1);
    api.addEvidence.mockRejectedValue(
      problem(400, { type: `${base}validation`, errors: { testTypeId: ['This procedure already has a test evidence.'] } }),
    );
    await save(fixture);

    expect(screen.getByRole('link', { name: 'Prüfnachweis 2 – Art der Prüfung: Für dieses Prüfverfahren gibt es schon einen Prüfnachweis.' })).toBeTruthy();
    expect(textbox('Wer hat geprüft?', 1).value).toBe('Prüfstelle Beispiel');
    expect(announce).not.toHaveBeenCalledWith('candidate.evidence.saved');
  });

  it('INV:WSP-01 shows a saved evidence and sends a new version as a change of the same row', async () => {
    const { fixture } = await openTab(candidateView({ candidate: TESTED }));

    expect(radio('Ja').checked).toBe(true);
    expect(screen.getByRole('heading', { level: 3, name: 'Prüfnachweis 1: BIT inklusiv Software-Test' })).toBeTruthy();
    expect(textbox('Datum der Prüfung').value).toBe('14.03.2026');
    expect(textbox(/^Geprüfte Version/).value).toBe('4.2');

    type(/^Geprüfte Version/, '5.0');
    type('Datum der Prüfung', '01.09.2026');
    await save(fixture);

    expect(api.changeTestStatus).not.toHaveBeenCalled();
    expect(api.addEvidence).not.toHaveBeenCalled();
    expect(api.changeEvidence).toHaveBeenCalledWith(
      'e-1',
      expect.objectContaining({ testedVersion: '5.0', testDate: '2026-09-01' }),
      '"1-3"',
    );
    expect(api.get).toHaveBeenCalledWith('c-1');
    expect(announce).toHaveBeenCalledWith('candidate.evidence.saved');
  });

  describe('409 conflict (R-CAN-11, S37)', () => {
    const conflict = () => problem(409, { type: `${base}conflict`, status: 409, editorDisplayName: null });

    it('INV:API-03 opens S37 for a stale evidence; after Escape „Speichern“ sends nothing and opens S37 again', async () => {
      const { fixture } = await openTab(candidateView({ candidate: TESTED }));
      api.changeEvidence.mockRejectedValue(conflict());

      type(/^Geprüfte Version/, '5.0');
      await save(fixture);

      const s37 = screen.getByRole('alertdialog', { name: 'Inzwischen geändert' });
      expect(s37.textContent).toContain('Eine andere Person hat diesen Inhalt geändert');
      expect(announce).not.toHaveBeenCalledWith('candidate.evidence.saved');

      fireEvent.keyDown(s37, { key: 'Escape', code: 'Escape', keyCode: 27 });
      await settle(fixture);
      expect(screen.queryByRole('alertdialog')).toBeNull();
      expect(textbox(/^Geprüfte Version/).value).toBe('5.0');

      await save(fixture);
      expect(api.changeEvidence).toHaveBeenCalledTimes(1);
      expect(screen.getByRole('alertdialog', { name: 'Inzwischen geändert' })).toBeTruthy();
    });

    it('copies the changed status and each unsaved card under its heading', async () => {
      const writeText = vi.fn().mockResolvedValue(undefined);
      Object.defineProperty(navigator, 'clipboard', { value: { writeText }, configurable: true });
      try {
        const { fixture } = await openTab(candidateView({ candidate: TESTED }));
        api.changeEvidence.mockRejectedValue(conflict());
        type(/^Geprüfte Version/, '5.0');
        screen.getByRole('button', { name: 'Weiteren Prüfnachweis hinzufügen' }).click();
        await settle(fixture);
        await chooseKind(fixture, 'Eigene Prüfung', 1);
        fillCard(1);
        await save(fixture);

        within(screen.getByRole('alertdialog')).getByRole('button', { name: 'Meine Änderungen kopieren' }).click();
        await settle(fixture);

        expect(writeText).toHaveBeenCalledWith(
          'Prüfnachweis 1: BIT inklusiv Software-Test\nGeprüfte Version (optional): 5.0\n\n' +
            'Prüfnachweis 2: Eigene Prüfung ohne festes Verfahren\nArt der Prüfung: Eigene Prüfung ohne festes Verfahren\n' +
            'Wer hat geprüft?: Prüfstelle Beispiel\nDatum der Prüfung: 14.03.2026',
        );
      } finally {
        Reflect.deleteProperty(navigator, 'clipboard');
      }
    });

    it('„Neue Fassung laden“ fills the tab from the stored product and focuses the H1', async () => {
      const { fixture } = await openTab(candidateView({ candidate: TESTED }));
      api.changeEvidence.mockRejectedValue(conflict());
      type(/^Geprüfte Version/, '5.0');
      await save(fixture);

      const stored: Candidate = { ...TESTED, etag: '"1-9"', testEvidences: [{ ...SOFTWARE_TEST, etag: '"1-4"', testedVersion: '4.3' }] };
      api.get.mockResolvedValue(stored);
      within(screen.getByRole('alertdialog')).getByRole('button', { name: 'Neue Fassung laden' }).click();
      await settle(fixture);

      expect(textbox(/^Geprüfte Version/).value).toBe('4.3');
      expect(focusPageHeading).toHaveBeenCalled();

      api.changeEvidence.mockImplementation((id: string, body: object) => ({ id, ...body }));
      type(/^Geprüfte Version/, '5.0');
      await save(fixture);
      expect(api.changeEvidence).toHaveBeenLastCalledWith('e-1', expect.objectContaining({ testedVersion: '5.0' }), '"1-4"');
    });
  });

  it('removes a card: announces it, moves the focus to the add button, and removes the evidence on „Speichern“', async () => {
    const { fixture } = await openTab(candidateView({ candidate: TESTED }));
    screen.getByRole('button', { name: 'Prüfnachweis 1 entfernen' }).click();
    await settle(fixture);

    expect(screen.queryByRole('heading', { level: 3 })).toBeNull();
    expect(announce).toHaveBeenCalledWith('candidate.evidence.list.removed');
    expect(document.activeElement).toBe(screen.getByRole('button', { name: 'Weiteren Prüfnachweis hinzufügen' }));
    expect(api.removeEvidence).not.toHaveBeenCalled();

    await save(fixture);
    expect(api.removeEvidence).toHaveBeenCalledWith('e-1', '"1-3"');
    expect(api.changeTestStatus).not.toHaveBeenCalled();
  });

  it('takes an evidence another session removed meanwhile (404) as removed and saves the rest', async () => {
    const { fixture } = await openTab(candidateView({ candidate: TESTED }));
    screen.getByRole('button', { name: 'Prüfnachweis 1 entfernen' }).click();
    await settle(fixture);
    api.removeEvidence.mockRejectedValueOnce(problem(404, { type: `${base}not-found` }));
    await save(fixture);
    expect(announce).toHaveBeenCalledWith('candidate.evidence.saved');

    await save(fixture);
    expect(api.removeEvidence).toHaveBeenCalledTimes(1);
  });

  it('INV:WSP-01 leaving „Ja“ with evidences asks first; „Abbrechen“ keeps „Ja“ and the cards', async () => {
    // A real dialog answers after a change detection at the earliest: the native radio „Nein“ was rendered checked
    const confirm = vi.spyOn(ConfirmDialog, 'confirm').mockImplementation(() => new Promise((resolve) => setTimeout(() => resolve(false))));
    const { fixture } = await openTab(candidateView({ candidate: TESTED }));
    await choose(fixture, 'Nein');
    await settle(fixture);

    expect(confirm).toHaveBeenCalledWith(expect.anything(), {
      titleKey: 'candidate.evidence.list.statusChange.title',
      bodyKey: 'candidate.evidence.list.statusChange.one',
      bodyParams: { count: '1' },
      confirmKey: 'candidate.evidence.list.statusChange.confirm',
    });
    expect(radio('Ja').checked).toBe(true);
    expect(screen.getByRole('heading', { level: 3, name: 'Prüfnachweis 1: BIT inklusiv Software-Test' })).toBeTruthy();
  });

  it('INV:WSP-01 „Nachweise entfernen“ drops the cards; „Speichern“ sets the status with the candidate’s tag and removes them with it', async () => {
    vi.spyOn(ConfirmDialog, 'confirm').mockResolvedValue(true);
    const { fixture } = await openTab(candidateView({ candidate: TESTED }));
    await choose(fixture, 'Nein');

    expect(screen.queryByRole('heading', { level: 2 })).toBeNull();
    await save(fixture);
    // One request: separate removals would change the candidate's tag before the status (S6-20)
    expect(api.changeTestStatus).toHaveBeenCalledWith('c-1', 'notTested', true, TEST_CANDIDATE.etag);
    expect(api.removeEvidence).not.toHaveBeenCalled();
  });

  it('INV:WSP-01 a 409 test-status-conflict asks with the server’s count and repeats the change with removeEvidences', async () => {
    const confirm = vi.spyOn(ConfirmDialog, 'confirm').mockResolvedValue(true);
    const { fixture } = await openTab(candidateView({ candidate: { ...TEST_CANDIDATE, testStatus: 'tested' } }));
    // The empty card of „Ja“ goes without a question
    await choose(fixture, 'Nicht bekannt');
    expect(confirm).not.toHaveBeenCalled();
    api.changeTestStatus.mockRejectedValueOnce(
      problem(409, { type: `${base}test-status-conflict`, evidenceCount: 2, errors: { testStatus: ['…'] } }),
    );
    await save(fixture);

    expect(confirm).toHaveBeenCalledWith(expect.anything(), expect.objectContaining({ bodyKey: 'candidate.evidence.list.statusChange', bodyParams: { count: '2' } }));
    expect(api.changeTestStatus.mock.calls).toEqual([
      ['c-1', 'unknown', false, TEST_CANDIDATE.etag],
      ['c-1', 'unknown', true, TEST_CANDIDATE.etag],
    ]);
    expect(announce).toHaveBeenCalledWith('candidate.evidence.saved');
  });

  it('INV:SEC-10 shows the notice of the unconfirmed address with the focus and keeps the input', async () => {
    const { fixture } = await openTab();
    await choose(fixture, 'Nein');
    api.changeTestStatus.mockRejectedValue(problem(403, { type: `${base}email-unconfirmed` }));
    await save(fixture);

    expect(document.activeElement?.querySelector('pa-confirm-required-notice')).not.toBeNull();
    expect(radio('Nein').checked).toBe(true);
  });

  it.each<[string, CandidateView, OrganizationRole]>([
    ['a Reader', candidateView(), 'reader'],
    ['an archived project', candidateView({ project: { ...TEST_PROJECT, status: 'archived' } }), 'editor'],
  ])('shows no form for %s (Reader view: S6-22)', async (_case, view, role) => {
    await openTab(view, role);

    expect(screen.queryByRole('radiogroup')).toBeNull();
    expect(screen.queryByRole('button')).toBeNull();
  });
});
