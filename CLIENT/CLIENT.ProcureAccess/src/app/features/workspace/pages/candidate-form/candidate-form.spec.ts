// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { BreakpointObserver } from '@angular/cdk/layout';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { Router } from '@angular/router';
import { fireEvent } from '@testing-library/dom';
import { render, screen, within } from '@testing-library/angular';
import { of } from 'rxjs';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { OrganizationRole } from '@app/features/organization/models/organization';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';

import { Candidate } from '../../models/candidates';
import { CandidatesApiService } from '../../services/api/candidates-api.service';
import { candidateView, TEST_CANDIDATE, TEST_PROJECT } from '../../testing/candidate-fixtures';
import { CandidateLoad, CandidateView } from '../candidate-detail/candidate-load';
import { CandidateFormPage } from './candidate-form';

@Component({ template: '' })
class Blank {}

const base = 'https://procureaccess.de/problems/';

function problem(status: number, body: object): HttpErrorResponse {
  return new HttpErrorResponse({ status, error: body });
}

const NEW_VIEW: CandidateView = candidateView({ candidate: null, criteria: null });

describe('CandidateFormPage (S33, R-CAN-02)', () => {
  let create: ReturnType<typeof vi.fn>;
  let change: ReturnType<typeof vi.fn>;
  let setPageTitleState: ReturnType<typeof vi.fn>;
  let focusPageHeading: ReturnType<typeof vi.fn>;
  let get: ReturnType<typeof vi.fn>;

  async function openForm(
    view: CandidateView = NEW_VIEW,
    { role = 'editor', api, stored }: { role?: OrganizationRole; api?: ReturnType<typeof vi.fn>; stored?: Candidate } = {},
  ) {
    create = api ?? vi.fn().mockResolvedValue(TEST_CANDIDATE);
    change = api ?? vi.fn().mockResolvedValue(TEST_CANDIDATE);
    get = vi.fn().mockResolvedValue(stored ?? TEST_CANDIDATE);
    setPageTitleState = vi.fn();
    focusPageHeading = vi.fn();
    const load = new CandidateLoad();
    load.view.set(view);
    const rendered = await render(CandidateFormPage, {
      routes: [{ path: '**', component: Blank }],
      providers: [
        provideTestTranslations(),
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        { provide: CandidateLoad, useValue: load },
        { provide: CandidatesApiService, useValue: { create, change, get } },
        { provide: PageTitleStrategy, useValue: { setPageTitleState, focusPageHeading } },
        {
          provide: ProcureAccessStore,
          useValue: {
            organizations: signal([{ id: 'org-1', name: 'Personalamt', type: 'publicBody', role }]),
            incrementLoadingCount: vi.fn(),
            decrementLoadingCount: vi.fn(),
          },
        },
        { provide: BreakpointObserver, useValue: { observe: () => of({ matches: true }), isMatched: () => true } },
      ],
    });
    await rendered.fixture.whenStable();
    return rendered;
  }

  const field = (name: string) => screen.getByRole<HTMLInputElement>('textbox', { name });
  const type = (name: string, value: string) => fireEvent.input(field(name), { target: { value } });

  async function save(fixture: { whenStable(): Promise<unknown> }) {
    screen.getByRole('button', { name: 'Speichern' }).click();
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  it('shows „Produkt hinzufügen“: the required note, the name required, the other fields „(optional)“, the private section apart', async () => {
    const { container } = await openForm();

    expect(screen.getByRole('heading', { level: 1, name: 'Produkt hinzufügen' })).toBeTruthy();
    expect(screen.queryByRole('tablist')).toBeNull();
    expect(screen.getByText('Alle Felder ohne „(optional)“ müssen ausgefüllt werden.')).toBeTruthy();
    expect(screen.getAllByRole('textbox').map((box) => box.id)).toEqual([
      'pa-candidate-name',
      'pa-candidate-vendor',
      'pa-candidate-website',
      'pa-candidate-functionalSummary',
      'pa-candidate-contactNotes',
      'pa-candidate-internalNotes',
    ]);
    expect(field('Produktname').required).toBe(true);
    expect(field('Anbieter (optional)')).toBeTruthy();
    expect(field('Webseite des Anbieters (optional)').type).toBe('url');
    expect(field('Was kann das Produkt? (Stichworte) (optional)').tagName).toBe('TEXTAREA');

    const privateSection = screen.getByRole('heading', { level: 2, name: 'Nur für Ihre Organisation' }).closest('section')!;
    expect(privateSection.classList).toContain('pa-candidate-form__card--private');
    expect(privateSection.textContent).toContain('Diese Angaben werden nie veröffentlicht');
    expect(privateSection.contains(field('Ansprechpersonen und Kontaktdaten (optional)'))).toBe(true);
    expect(privateSection.contains(field('Interne Notizen (optional)'))).toBe(true);
    expect(privateSection.contains(field('Anbieter (optional)'))).toBe(false);

    expect(screen.getByRole('link', { name: 'Abbrechen' }).getAttribute('href')).toBe('/projekte/p-1');
    expect(setPageTitleState).toHaveBeenCalledWith({ params: { project: 'Neues Bewerbermanagement 2027', product: '' } });
    expect(container.querySelector('section[aria-labelledby], section[aria-label]')).toBeNull();
    await expectNoAxeViolations();
  });

  it('checks on submit: the name is required, the website an https address, every field within its limit', async () => {
    const { fixture } = await openForm();

    await save(fixture);
    expect(screen.getAllByText('Bitte geben Sie Produktname an.').length).toBeGreaterThan(0);
    expect(document.activeElement?.closest('pa-error-summary')).not.toBeNull();
    expect(setPageTitleState).toHaveBeenLastCalledWith(expect.objectContaining({ error: true }));

    type('Produktname', 'a'.repeat(201));
    type('Webseite des Anbieters (optional)', 'http://www.beispiel.de');
    type('Interne Notizen (optional)', 'x'.repeat(2001));
    await save(fixture);
    expect(screen.getAllByText('Bitte kürzen Sie den Text auf höchstens 200 Zeichen. Derzeit sind es 201.').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Bitte geben Sie eine vollständige Webadresse ein, die mit https:// beginnt.').length).toBeGreaterThan(0);
    expect(screen.getAllByText('Bitte kürzen Sie den Text auf höchstens 2000 Zeichen. Derzeit sind es 2001.').length).toBeGreaterThan(0);
    expect(create).not.toHaveBeenCalled();
    await expectNoAxeViolations();
  });

  it('adds the product with every field trimmed and opens its overview S34', async () => {
    const { fixture } = await openForm();

    type('Produktname', '  BewerberPro  ');
    type('Webseite des Anbieters (optional)', 'https://www.beispiel-software.de');
    type('Interne Notizen (optional)', 'Demo vereinbart.');
    await save(fixture);

    expect(create).toHaveBeenCalledWith('p-1', {
      name: 'BewerberPro',
      vendor: '',
      website: 'https://www.beispiel-software.de',
      functionalSummary: '',
      contactNotes: '',
      internalNotes: 'Demo vereinbart.',
    });
    expect(TestBed.inject(Router).url).toBe('/projekte/p-1/produkte/c-1');
  });

  it('edits a product: „Produkt bearbeiten“, the fields filled, the tabs with Überblick selected; an emptied field is cleared', async () => {
    const { fixture } = await openForm(candidateView({ criteria: null }), { role: 'owner' });

    expect(screen.getByRole('heading', { level: 1, name: 'Produkt bearbeiten' })).toBeTruthy();
    expect(screen.getAllByRole('tab').map((tab) => tab.getAttribute('aria-selected'))).toEqual(['true', 'false', 'false']);
    expect(screen.getByRole('tabpanel', { name: 'Überblick' }).querySelector('form')).not.toBeNull();
    expect(field('Produktname').value).toBe('BewerberPro');
    expect(field('Interne Notizen (optional)').value).toBe('Demo am 20.10.2026 vereinbart.');
    expect(screen.getByRole('link', { name: 'Abbrechen' }).getAttribute('href')).toBe('/projekte/p-1/produkte/c-1');
    expect(setPageTitleState).toHaveBeenCalledWith({ params: { project: 'Neues Bewerbermanagement 2027', product: 'BewerberPro' } });
    await expectNoAxeViolations();

    type('Anbieter (optional)', '');
    await save(fixture);

    expect(change).toHaveBeenCalledWith('c-1', {
      name: 'BewerberPro',
      vendor: '',
      website: 'https://www.beispiel-software.de/bewerberpro',
      functionalSummary: 'Bewerbermanagement, Stellenausschreibungen, Terminplanung',
      contactNotes: 'Frau Beispiel, Vertrieb',
      internalNotes: 'Demo am 20.10.2026 vereinbart.',
    }, TEST_CANDIDATE.etag);
    expect(TestBed.inject(Router).url).toBe('/projekte/p-1/produkte/c-1');
  });

  describe('409 conflict when editing (R-CAN-11, S37)', () => {
    const conflict = () => problem(409, { type: `${base}conflict`, status: 409, editorDisplayName: 'a.schmidt@example.test' });
    const stored: Candidate = { ...TEST_CANDIDATE, etag: '"1-7"', vendor: 'Neuer Anbieter AG' };

    it('INV:API-03 opens S37; after Escape „Speichern“ sends nothing and opens S37 again', async () => {
      const { fixture } = await openForm(candidateView({ criteria: null }), { api: vi.fn().mockRejectedValue(conflict()) });

      type('Anbieter (optional)', 'Meine GmbH');
      await save(fixture);

      const s37 = screen.getByRole('alertdialog', { name: 'Inzwischen geändert' });
      expect(s37.textContent).toContain('a.schmidt@example.test hat diesen Inhalt geändert');
      // No error summary: S37 is the only message
      expect(document.querySelector('pa-error-summary')?.textContent?.trim() ?? '').toBe('');

      fireEvent.keyDown(s37, { key: 'Escape', code: 'Escape', keyCode: 27 });
      await fixture.whenStable();
      await new Promise((resolve) => setTimeout(resolve));
      expect(screen.queryByRole('alertdialog')).toBeNull();
      expect(field('Anbieter (optional)').value).toBe('Meine GmbH');

      await save(fixture);
      expect(change).toHaveBeenCalledTimes(1);
      expect(screen.getByRole('alertdialog', { name: 'Inzwischen geändert' })).toBeTruthy();
    });

    it('copies the changed fields with their labels', async () => {
      const writeText = vi.fn().mockResolvedValue(undefined);
      Object.defineProperty(navigator, 'clipboard', { value: { writeText }, configurable: true });
      try {
        const { fixture } = await openForm(candidateView({ criteria: null }), { api: vi.fn().mockRejectedValue(conflict()) });
        type('Anbieter (optional)', 'Meine GmbH');
        type('Interne Notizen (optional)', 'Rückruf am Montag');
        await save(fixture);

        within(screen.getByRole('alertdialog')).getByRole('button', { name: 'Meine Änderungen kopieren' }).click();
        await fixture.whenStable();
        await new Promise((resolve) => setTimeout(resolve));

        expect(writeText).toHaveBeenCalledWith('Anbieter: Meine GmbH\nInterne Notizen: Rückruf am Montag');
      } finally {
        Reflect.deleteProperty(navigator, 'clipboard');
      }
    });

    it('„Neue Fassung laden“ reads the product, fills the form with it, focuses the H1 and saves again with its tag', async () => {
      const { fixture } = await openForm(candidateView({ criteria: null }), { api: vi.fn().mockRejectedValue(conflict()), stored });

      type('Anbieter (optional)', 'Meine GmbH');
      await save(fixture);
      within(screen.getByRole('alertdialog')).getByRole('button', { name: 'Neue Fassung laden' }).click();
      await fixture.whenStable();
      await new Promise((resolve) => setTimeout(resolve));
      await fixture.whenStable();

      expect(get).toHaveBeenCalledWith(TEST_CANDIDATE.id);
      expect(field('Anbieter (optional)').value).toBe('Neuer Anbieter AG');
      expect(focusPageHeading).toHaveBeenCalled();

      change.mockResolvedValue(stored);
      await save(fixture);
      expect(change).toHaveBeenLastCalledWith(TEST_CANDIDATE.id, expect.objectContaining({ vendor: 'Neuer Anbieter AG' }), '"1-7"');
    });
  });

  it('INV:API-02 shows a server field error at its field and in the error summary', async () => {
    const { fixture } = await openForm(NEW_VIEW, {
      api: vi.fn().mockRejectedValue(problem(400, { type: `${base}validation`, status: 400, errors: { website: ['Nur https.'] } })),
    });

    type('Produktname', 'BewerberPro');
    await save(fixture);

    expect(screen.getAllByText('Nur https.', { exact: false }).length).toBeGreaterThan(1);
    expect(field('Webseite des Anbieters (optional)').getAttribute('aria-invalid')).toBe('true');
    expect(TestBed.inject(Router).url).not.toContain('produkte');
  });

  it('INV:SEC-10 shows the confirmation notice with the focus for an unconfirmed address and keeps the input', async () => {
    const { fixture } = await openForm(NEW_VIEW, {
      api: vi.fn().mockRejectedValue(problem(403, { type: `${base}email-unconfirmed`, status: 403 })),
    });

    type('Produktname', 'BewerberPro');
    await save(fixture);

    expect(document.activeElement?.textContent).toContain('Bitte bestätigen Sie zuerst Ihre E-Mail-Adresse.');
    expect(field('Produktname').value).toBe('BewerberPro');
  });

  it('shows antiforgery in the error summary', async () => {
    const { fixture } = await openForm(NEW_VIEW, {
      api: vi.fn().mockRejectedValue(problem(400, { type: `${base}antiforgery`, status: 400 })),
    });

    type('Produktname', 'BewerberPro');
    await save(fixture);

    expect(document.activeElement?.closest('pa-error-summary')?.textContent).toContain('aus Sicherheitsgründen nicht ausgeführt');
  });

  it('shows the archived note instead of the form (R-PRJ-05)', async () => {
    await openForm(candidateView({ project: { ...TEST_PROJECT, status: 'archived' }, candidate: null, criteria: null }));

    expect(screen.getByRole('heading', { level: 1, name: 'Produkt hinzufügen' })).toBeTruthy();
    expect(screen.getByText(/^Dieses Projekt ist archiviert\./)).toBeTruthy();
    expect(screen.queryByRole('textbox')).toBeNull();
  });

  it('shows „Kein Zugriff“ in place for a Reader', async () => {
    await openForm(NEW_VIEW, { role: 'reader' });

    expect(screen.getByRole('heading', { level: 1, name: 'Kein Zugriff' })).toBeTruthy();
    expect(screen.queryByRole('textbox')).toBeNull();
  });

  it('INV:TEN-01 shows „Seite nicht gefunden“ in place for a project the user cannot see', async () => {
    await openForm({ kind: 'notFound' });

    expect(screen.getByRole('heading', { level: 1, name: 'Seite nicht gefunden' })).toBeTruthy();
  });
});
