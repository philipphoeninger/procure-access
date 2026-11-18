// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { signal } from '@angular/core';
import { render, screen } from '@testing-library/angular';

import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { OrganizationRole } from '@app/features/organization/models/organization';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';

import { Candidate, TestEvidence } from '../../../models/candidates';
import { candidateView, TEST_CANDIDATE, TEST_PROJECT } from '../../../testing/candidate-fixtures';
import { CandidateLoad, CandidateView } from '../candidate-load';
import { CandidateOverview } from './candidate-overview';

const EVIDENCE: TestEvidence = {
  id: 'e-1',
  testKind: 'procedure',
  testTypeId: 'softwareTest',
  otherTestTypeName: null,
  testedBy: 'Prüfstelle',
  testDate: '2026-09-01',
  testedVersion: null,
  evidenceUrl: null,
  qualificationNote: null,
};

describe('CandidateOverview (S34 Überblick, R-CAN-04)', () => {
  let setPageTitleState: ReturnType<typeof vi.fn>;

  async function openTab(view: CandidateView = candidateView(), role: OrganizationRole = 'editor') {
    setPageTitleState = vi.fn();
    const load = new CandidateLoad();
    load.view.set(view);
    const rendered = await render(CandidateOverview, {
      routes: [{ path: '**', children: [] }],
      providers: [
        provideTestTranslations(),
        { provide: CandidateLoad, useValue: load },
        { provide: PageTitleStrategy, useValue: { setPageTitleState } },
        { provide: ProcureAccessStore, useValue: { organizations: signal([{ id: 'org-1', name: 'Personalamt', type: 'publicBody', role }]) } },
      ],
    });
    await rendered.fixture.whenStable();
    return rendered;
  }

  const withCandidate = (changes: Partial<Candidate>) => candidateView({ candidate: { ...TEST_CANDIDATE, ...changes } });
  const facts = (container: Element, heading: string) => {
    const card = screen.getByRole('heading', { level: 2, name: heading }).closest('section')!;
    expect(container.contains(card)).toBe(true);
    return [...card.querySelectorAll('dl > div')].map((row) => [
      row.querySelector('dt')?.textContent?.trim(),
      row.querySelector('dd')?.textContent?.trim(),
    ]);
  };

  it('shows the four cards with the facts of the product; the page title names the tab, product and project (R-CAN-03)', async () => {
    const { container } = await openTab();

    expect(screen.getAllByRole('heading', { level: 2 }).map((heading) => heading.textContent?.trim())).toEqual([
      'Produktangaben',
      'Stand der Bewertung',
      'Veröffentlichung',
      'Nur für Ihre Organisation',
    ]);
    expect(facts(container, 'Produktangaben')).toEqual([
      ['Produktname', 'BewerberPro'],
      ['Anbieter', 'Beispiel Software GmbH'],
      ['Webseite des Anbieters', 'https://www.beispiel-software.de/bewerberpro'],
      ['Was kann das Produkt? (Stichworte)', 'Bewerbermanagement, Stellenausschreibungen, Terminplanung'],
      ['Anwendungstyp', 'Website oder Webanwendung, Mobile App'],
      ['Im Produktverzeichnis', 'Nicht verknüpft'],
    ]);
    expect(screen.getByRole('link', { name: 'https://www.beispiel-software.de/bewerberpro' }).getAttribute('href')).toBe(
      'https://www.beispiel-software.de/bewerberpro',
    );
    expect(setPageTitleState).toHaveBeenCalledWith({
      params: { tab: 'Überblick', product: 'BewerberPro', project: 'Neues Bewerbermanagement 2027' },
    });
    await expectNoAxeViolations();
  });

  it('leaves out optional facts that are empty, and the private list when both notes are', async () => {
    const { container } = await openTab(
      withCandidate({ vendor: null, website: null, functionalSummary: null, privateSection: { contactNotes: null, internalNotes: null } }),
    );

    expect(facts(container, 'Produktangaben').map(([term]) => term)).toEqual(['Produktname', 'Anwendungstyp', 'Im Produktverzeichnis']);
    expect(facts(container, 'Nur für Ihre Organisation')).toEqual([]);
  });

  describe('„Stand der Bewertung“', () => {
    it('INV:SIG-13 shows the server\'s progress and K1 with its seven counts in the order of the scale (D3 (b))', async () => {
      await openTab();

      expect(screen.getByText('Auf Barrierefreiheit geprüft: Nicht bekannt')).toBeTruthy();
      expect(
        screen.getByText('36 von 46 Kriterien eingeschätzt · Kriterienliste „HR-Software 2027“ (Katalogversion T.1)'),
      ).toBeTruthy();
      const breakdown = screen.getByRole('heading', { level: 4, name: 'Ihre Einschätzung nach „HR-Software 2027“' }).closest('section')!;
      expect([...breakdown.querySelectorAll('dd')].map((value) => value.textContent?.trim())).toEqual([
        '22 Kriterien',
        '3 Kriterien',
        '6 Kriterien',
        '2 Kriterien',
        '1 Kriterium',
        '2 Kriterien',
        '10 Kriterien',
      ]);
      expect(screen.getByRole('button', { name: 'Was bedeuten die Bewertungsstufen?' })).toBeTruthy();
      expect(screen.getByRole('link', { name: 'Zum Prüfnachweis' }).getAttribute('href')).toBe('/projekte/p-1/produkte/c-1/pruefnachweis');
      expect(screen.getByRole('link', { name: 'Zur Einschätzung' }).getAttribute('href')).toBe('/projekte/p-1/produkte/c-1/einschaetzung');
      await expectNoAxeViolations();
    });

    it('names the evidences as S32 does: „Ja – 2 Prüfnachweise: …“, the name of „Andere Prüfung“ and „Eigene Prüfung“', async () => {
      await openTab(
        withCandidate({
          testStatus: 'tested',
          testEvidences: [EVIDENCE, { ...EVIDENCE, id: 'e-2', testKind: 'other', testTypeId: null, otherTestTypeName: 'Hausprüfung' }],
        }),
      );
      expect(
        screen.getByText('Auf Barrierefreiheit geprüft: Ja – 2 Prüfnachweise: BIT inklusiv Software-Test, Hausprüfung'),
      ).toBeTruthy();
    });

    it('uses the singular for one evidence and „Nein“ / „Ja“ without evidences', async () => {
      await openTab(withCandidate({ testStatus: 'tested', testEvidences: [{ ...EVIDENCE, testKind: 'own', testTypeId: null }] }));
      expect(screen.getByText('Auf Barrierefreiheit geprüft: Ja – 1 Prüfnachweis: Eigene Prüfung ohne festes Verfahren')).toBeTruthy();
    });

    it('shows „Nein“ for a product that was not tested', async () => {
      await openTab(withCandidate({ testStatus: 'notTested' }));
      expect(screen.getByText('Auf Barrierefreiheit geprüft: Nein')).toBeTruthy();
    });

    it('without a list: „Noch keine Kriterienliste“ and the project notice instead of progress and K1', async () => {
      const { container } = await openTab(
        candidateView({
          project: { ...TEST_PROJECT, criteriaList: null },
          candidate: { ...TEST_CANDIDATE, progress: null, fulfillment: null },
          criteria: null,
        }),
      );

      expect(facts(container, 'Produktangaben')).toContainEqual(['Anwendungstyp', 'Noch keine Kriterienliste']);
      expect(screen.getByText(/^Diesem Projekt ist noch keine Kriterienliste zugeordnet\./)).toBeTruthy();
      expect(screen.queryByText(/Kriterien eingeschätzt/)).toBeNull();
      expect(screen.queryByRole('heading', { level: 4 })).toBeNull();
      expect(screen.getByRole('link', { name: 'Zur Einschätzung' })).toBeTruthy();
    });
  });

  it('facts only: the publication card says it is not published, and nothing anywhere is a verdict (SIG-01)', async () => {
    const { container } = await openTab();

    expect(screen.getByText('Noch nicht im Produktverzeichnis veröffentlicht.')).toBeTruthy();
    expect(screen.getByText(/^Wenn Sie veröffentlichen, erscheinen Prüfnachweis und Einschätzungen/)).toBeTruthy();
    // The legend's footnote says „kein Gesamturteil“; no share, score or grade appears
    expect(container.textContent).not.toMatch(/%|Punkte|Note:|Gesamturteil:/);
    expect(container.querySelector('progress, meter, [role="progressbar"], [role="meter"], svg, canvas')).toBeNull();
  });

  it('keeps the private notes in their own card on the overview, with the text that they are never published (PRV-02, client part; tagged with step 6)', async () => {
    const { container } = await openTab();

    const card = screen.getByRole('heading', { level: 2, name: 'Nur für Ihre Organisation' }).closest('section')!;
    expect(card.classList).toContain('pa-overview__card--private');
    expect(card.textContent).toContain('Diese Angaben werden nie veröffentlicht');
    expect(facts(container, 'Nur für Ihre Organisation')).toEqual([
      ['Ansprechpersonen und Kontaktdaten', 'Frau Beispiel, Vertrieb'],
      ['Interne Notizen', 'Demo am 20.10.2026 vereinbart.'],
    ]);
    const publication = screen.getByRole('heading', { level: 2, name: 'Veröffentlichung' }).closest('section')!;
    expect(publication.textContent).not.toContain('Frau Beispiel');
  });

  describe('„Bearbeiten“ → S33', () => {
    it('is offered to an Editor in both editable cards, each described by its card heading', async () => {
      await openTab();

      const links = screen.getAllByRole('link', { name: 'Bearbeiten' });
      expect(links.map((link) => link.getAttribute('href'))).toEqual([
        '/projekte/p-1/produkte/c-1/bearbeiten',
        '/projekte/p-1/produkte/c-1/bearbeiten',
      ]);
      expect(links.map((link) => document.getElementById(link.getAttribute('aria-describedby')!)?.textContent?.trim())).toEqual([
        'Produktangaben',
        'Nur für Ihre Organisation',
      ]);
    });

    it('is not offered to a Reader', async () => {
      await openTab(candidateView(), 'reader');
      expect(screen.queryByRole('link', { name: 'Bearbeiten' })).toBeNull();
    });

    it('is not offered for an archived project (R-PRJ-05)', async () => {
      await openTab(candidateView({ project: { ...TEST_PROJECT, status: 'archived' } }), 'owner');
      expect(screen.queryByRole('link', { name: 'Bearbeiten' })).toBeNull();
    });
  });
});
