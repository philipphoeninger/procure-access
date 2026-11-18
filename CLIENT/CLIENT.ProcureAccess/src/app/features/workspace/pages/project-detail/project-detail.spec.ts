// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { signal, WritableSignal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { render, screen, within } from '@testing-library/angular';
import { of } from 'rxjs';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { OrganizationRole } from '@app/features/organization/models/organization';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';

import { Project, ProjectCandidate } from '../../models/projects';
import { ProjectCriteriaFacts, ProjectLoad, ProjectView } from './project-detail-load';
import { ProjectDetailPage } from './project-detail';

const PROJECT: Project = {
  id: 'p-1',
  organizationId: 'org-1',
  name: 'Neues Bewerbermanagement 2027',
  status: 'open',
  productCount: 0,
  createdAt: '2026-11-18T09:00:00Z',
  modifiedAt: '2026-11-18T09:00:00Z',
  criteriaList: null,
  criteriaListLocked: false,
  candidates: [],
};

const FULFILLMENT = { fulfilled: 22, slightRestriction: 3, restriction: 6, barrier: 2, blockade: 1, notApplicable: 2, unknown: 10 };

/** The example data of the Figma frame, in the order the server lists them: as they were added. */
const TALENT_FLOW: ProjectCandidate = {
  id: 'c-3',
  name: 'TalentFlow',
  vendor: 'Muster AG',
  testStatus: 'notTested',
  testEvidences: [],
  progress: { done: 0, total: 46 },
  fulfillment: { fulfilled: 0, slightRestriction: 0, restriction: 0, barrier: 0, blockade: 0, notApplicable: 0, unknown: 46 },
};
const PERSONAL_PLUS: ProjectCandidate = {
  id: 'c-2',
  name: 'PersonalPlus Recruiting',
  vendor: null,
  testStatus: 'unknown',
  testEvidences: [],
  progress: { done: 12, total: 46 },
  fulfillment: { fulfilled: 8, slightRestriction: 1, restriction: 2, barrier: 1, blockade: 0, notApplicable: 0, unknown: 34 },
};
const BEWERBER_PRO: ProjectCandidate = {
  id: 'c-1',
  name: 'BewerberPro',
  vendor: 'Beispiel Software GmbH',
  testStatus: 'tested',
  testEvidences: [
    { testKind: 'procedure', testTypeId: 'softwareTest', otherTestTypeName: null },
    { testKind: 'procedure', testTypeId: 'bikWeb', otherTestTypeName: null },
  ],
  progress: { done: 36, total: 46 },
  fulfillment: FULFILLMENT,
};

const LIST = { id: 'copy-1', name: 'HR-Software 2027', releaseVersion: 'T.1' };
const WITH_LIST: Project = {
  ...PROJECT,
  productCount: 3,
  criteriaList: LIST,
  candidates: [TALENT_FLOW, PERSONAL_PLUS, BEWERBER_PRO],
};

const FACTS: ProjectCriteriaFacts = {
  listName: 'HR-Software 2027',
  releaseVersion: 'T.1',
  scope: { total: 46, a: 12, b: 28, info: 6, procedures: 2 },
  useCase: 'HR-Anwendung',
  appTypes: ['Website oder Webanwendung'],
  testTypes: ['BIT inklusiv Software-Test', 'BIK BITV-Test (Web)'],
};

const withList = (project: Partial<Project> = {}, criteria: ProjectCriteriaFacts = FACTS): ProjectView => ({
  kind: 'project',
  project: { ...WITH_LIST, ...project },
  criteria,
});
const withoutList = (project: Partial<Project> = {}): ProjectView => ({ kind: 'project', project: { ...PROJECT, ...project }, criteria: null });

/** The `dd` of the term `term` inside `container`. */
function valueOf(container: HTMLElement, term: string): HTMLElement {
  const dt = within(container).getByText(term, { selector: 'dt' });
  return dt.nextElementSibling as HTMLElement;
}

describe('ProjectDetailPage (S32)', () => {
  let setPageTitleState: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;
  let takeCriteriaAdded: ReturnType<typeof vi.fn>;
  let openDialog: ReturnType<typeof vi.fn>;
  let activeOrganization: WritableSignal<{ id: string } | null>;

  async function openPage(
    view: ProjectView,
    { role = 'editor', added = 0, changed }: { role?: OrganizationRole; added?: number; changed?: Project } = {},
  ) {
    setPageTitleState = vi.fn();
    announce = vi.fn().mockResolvedValue(undefined);
    takeCriteriaAdded = vi.fn().mockReturnValue(added);
    openDialog = vi.fn().mockReturnValue({ afterClosed: () => of(changed) });
    activeOrganization = signal({ id: 'org-1' });
    const load = new ProjectLoad();
    load.view = view;
    const rendered = await render(ProjectDetailPage, {
      routes: [
        { path: 'projekte', children: [] },
        { path: 'projekte/:id/kriterienliste', children: [] },
        { path: 'projekte/:id/produkte/neu', children: [] },
        { path: 'projekte/:id/produkte/:pid', children: [] },
      ],
      providers: [
        provideTestTranslations(),
        { provide: ProjectLoad, useValue: load },
        { provide: PageTitleStrategy, useValue: { setPageTitleState } },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: MatDialog, useValue: { open: openDialog } },
        {
          provide: ProcureAccessStore,
          useValue: {
            organizations: signal([{ id: 'org-1', name: 'Personalamt Beispielstadt', type: 'publicBody', role }]),
            activeOrganization,
            takeCriteriaAdded,
          },
        },
      ],
    });
    await rendered.fixture.whenStable();
    return rendered;
  }

  const criteriaCard = () => screen.getByRole('heading', { level: 2, name: 'Kriterienliste' }).closest('.pa-project__card') as HTMLElement;

  it('shows the path, the name as H1, the status and the number of products, and fills the page title', async () => {
    await openPage(withoutList());

    const path = screen.getByRole('navigation', { name: 'Sie sind hier' });
    expect(within(path).getByRole('link', { name: 'Projekte' }).getAttribute('href')).toBe('/projekte');
    expect(within(path).getByText('Neues Bewerbermanagement 2027').getAttribute('aria-current')).toBe('page');
    expect(screen.getByRole('heading', { level: 1, name: 'Neues Bewerbermanagement 2027' })).toBeTruthy();
    expect(screen.getByText('Stand: Laufend · noch keine Produkte · nur für Ihre Organisation sichtbar')).toBeTruthy();
    expect(screen.queryByText(/archiviert/)).toBeNull();
    expect(setPageTitleState).toHaveBeenCalledWith({ params: { project: 'Neues Bewerbermanagement 2027' } });
    await expectNoAxeViolations();
  });

  describe('„Projekt bearbeiten“ (R-PRJ-05)', () => {
    it('opens the dialog for the project and shows the changed project afterwards', async () => {
      const changed = { ...PROJECT, name: 'Bewerbermanagement 2028', status: 'done' as const };
      await openPage(withoutList(), { changed });

      screen.getByRole('button', { name: 'Projekt bearbeiten' }).click();
      await screen.findByRole('heading', { level: 1, name: 'Bewerbermanagement 2028' });

      expect(openDialog).toHaveBeenCalledWith(expect.anything(), expect.objectContaining({ data: PROJECT }));
      expect(screen.getByText('Stand: Abgeschlossen · noch keine Produkte · nur für Ihre Organisation sichtbar')).toBeTruthy();
      expect(setPageTitleState).toHaveBeenLastCalledWith({ params: { project: 'Bewerbermanagement 2028' } });
    });

    it('is not offered to a Reader', async () => {
      await openPage(withList(), { role: 'reader' });

      expect(screen.queryByRole('button', { name: 'Projekt bearbeiten' })).toBeNull();
      expect(screen.queryByRole('button', { name: 'Produkt hinzufügen' })).toBeNull();
      expect(screen.queryByRole('link', { name: 'Kriterienliste anpassen' })).toBeNull();
      // Read-only, but everything shows and the names still link
      expect(screen.getByRole('link', { name: 'HR-Software 2027' })).toBeTruthy();
      expect(screen.getByRole('link', { name: 'BewerberPro' })).toBeTruthy();
    });

    it('stays for the status of an archived project, which is otherwise read-only', async () => {
      await openPage(withList({ status: 'archived' }), { role: 'owner' });

      expect(screen.getByText(/^Dieses Projekt ist archiviert\./)).toBeTruthy();
      expect(screen.getByRole('button', { name: 'Projekt bearbeiten' })).toBeTruthy();
      expect(screen.queryByRole('button', { name: 'Produkt hinzufügen' })).toBeNull();
      expect(screen.queryByRole('link', { name: 'Kriterienliste anpassen' })).toBeNull();
    });
  });

  describe('card „Kriterienliste“ (R-PRJ-03, R-PRJ-06)', () => {
    it('describes the copy: list, release, scope with „Geprüft nach …“ apart, use case, app types, test types', async () => {
      await openPage(withList());

      const card = criteriaCard();
      expect(within(valueOf(card, 'Liste')).getByRole('link', { name: 'HR-Software 2027' }).getAttribute('href')).toBe(
        '/projekte/p-1/kriterienliste',
      );
      expect(valueOf(card, 'Katalogversion').textContent?.trim()).toBe('T.1');
      expect(valueOf(card, 'Umfang').textContent?.trim()).toBe(
        '46 Kriterien: 12 Mindestanforderungen (A), 28 Bewertungskriterien (B), 6 Hinweise – dazu 2 × „Geprüft nach …“ (zählen im Abgleich nicht mit)',
      );
      expect(valueOf(card, 'Verwendungszweck').textContent?.trim()).toBe('HR-Anwendung');
      expect(valueOf(card, 'Anwendungstyp').textContent?.trim()).toBe('Website oder Webanwendung');
      expect(valueOf(card, 'Prüfverfahren').textContent?.trim()).toBe('BIT inklusiv Software-Test, BIK BITV-Test (Web)');
      expect(within(card).getByText(/^Dieses Projekt nutzt eine eigene Kopie der Liste „HR-Software 2027“\./)).toBeTruthy();
      expect(within(card).getByRole('link', { name: 'Kriterienliste anpassen' }).getAttribute('href')).toBe('/projekte/p-1/kriterienliste');
      expect(within(card).queryByText(/^Die Kriterienliste kann nicht mehr/)).toBeNull();
      await expectNoAxeViolations();
    });

    it('names a list without procedures, without use case and without test types', async () => {
      await openPage(withList({}, { ...FACTS, scope: { ...FACTS.scope, procedures: 0 }, useCase: null, testTypes: [] }));

      const card = criteriaCard();
      expect(valueOf(card, 'Umfang').textContent?.trim()).toBe(
        '46 Kriterien: 12 Mindestanforderungen (A), 28 Bewertungskriterien (B), 6 Hinweise',
      );
      expect(valueOf(card, 'Verwendungszweck').textContent?.trim()).toBe('Keiner davon passt');
      expect(within(card).queryByText('Prüfverfahren')).toBeNull();
    });

    it('INV:WSP-02 says the list can no longer be replaced once the server reports the lock (R-PRJ-04)', async () => {
      await openPage(withList({ criteriaListLocked: true }));

      expect(within(criteriaCard()).getByText(/^Die Kriterienliste kann nicht mehr gegen eine andere ausgetauscht werden/)).toBeTruthy();
      // Adjusting stays (C51)
      expect(within(criteriaCard()).getByRole('link', { name: 'Kriterienliste anpassen' })).toBeTruthy();
    });

    it('says when the project has no list yet, without an action to assign one (owner decision S6-14 D2 (a))', async () => {
      await openPage(withoutList());

      expect(screen.getByText(/^Diesem Projekt ist noch keine Kriterienliste zugeordnet\./)).toBeTruthy();
      expect(screen.queryByRole('link', { name: 'Kriterienliste anpassen' })).toBeNull();
      expect(screen.queryByRole('button', { name: 'Kriterienliste zuordnen' })).toBeNull();
    });

    it('INV:WSP-03 shows and announces the criteria S11 of the copy took into the list, once', async () => {
      await openPage(withList(), { added: 2 });

      expect(takeCriteriaAdded).toHaveBeenCalledWith('p-1');
      expect(screen.getByText('2 Kriterien neu in der Liste. Sie sind bei allen Produkten noch nicht eingeschätzt.')).toBeTruthy();
      expect(announce).toHaveBeenCalledWith('projects.detail.criteriaAdded', { count: 2 });
    });

    it('uses the singular for one added criterion', async () => {
      await openPage(withList(), { added: 1 });

      expect(screen.getByText('1 Kriterium neu in der Liste. Es ist bei allen Produkten noch nicht eingeschätzt.')).toBeTruthy();
      expect(announce).toHaveBeenCalledWith('projects.detail.criteriaAdded.one', { count: 1 });
    });
  });

  describe('card „Produkte in diesem Projekt“ (R-PRJ-03)', () => {
    it('lists the last added product first, after the order note, and the name is its only link, to S34', async () => {
      await openPage(withList());

      expect(screen.getByText(/^Reihenfolge: zuletzt hinzugefügt zuerst\. Die Liste ist kein Ranking/)).toBeTruthy();
      const names = screen.getAllByRole('heading', { level: 3 }).map((heading) => heading.textContent?.trim());
      expect(names).toEqual(['BewerberPro', 'PersonalPlus Recruiting', 'TalentFlow']);
      for (const heading of screen.getAllByRole('heading', { level: 3 })) {
        const item = heading.closest('li') as HTMLElement;
        const links = within(item).getAllByRole('link');
        expect(links).toHaveLength(1);
        expect(links[0].closest('h3')).toBe(heading);
      }
      expect(screen.getByRole('link', { name: 'BewerberPro' }).getAttribute('href')).toBe('/projekte/p-1/produkte/c-1');
      await expectNoAxeViolations();
    });

    it('shows vendor, test status with the evidences, progress and publication of each product', async () => {
      await openPage(withList());

      const pro = screen.getByRole('heading', { level: 3, name: 'BewerberPro' }).closest('li') as HTMLElement;
      expect(within(pro).getByText('Anbieter: Beispiel Software GmbH')).toBeTruthy();
      expect(valueOf(pro, 'Auf Barrierefreiheit geprüft').textContent?.trim()).toBe(
        'Ja – 2 Prüfnachweise: BIT inklusiv Software-Test, BIK BITV-Test (Web)',
      );
      expect(valueOf(pro, 'Einschätzung').textContent?.trim()).toBe('36 von 46 Kriterien eingeschätzt');
      expect(valueOf(pro, 'Veröffentlichung').textContent?.trim()).toBe('Nicht veröffentlicht');

      const plus = screen.getByRole('heading', { level: 3, name: 'PersonalPlus Recruiting' }).closest('li') as HTMLElement;
      expect(within(plus).queryByText(/^Anbieter:/)).toBeNull();
      expect(valueOf(plus, 'Auf Barrierefreiheit geprüft').textContent?.trim()).toBe('Nicht bekannt');
      const flow = screen.getByRole('heading', { level: 3, name: 'TalentFlow' }).closest('li') as HTMLElement;
      expect(valueOf(flow, 'Auf Barrierefreiheit geprüft').textContent?.trim()).toBe('Nein');
    });

    it('names one evidence in the singular, another test by its name and an own test', async () => {
      const one = { ...BEWERBER_PRO, testEvidences: [{ testKind: 'other' as const, testTypeId: null, otherTestTypeName: 'Hausinterner Test' }] };
      const two = {
        ...BEWERBER_PRO,
        id: 'c-4',
        name: 'Zweites Produkt',
        testEvidences: [
          { testKind: 'own' as const, testTypeId: null, otherTestTypeName: null },
          { testKind: 'procedure' as const, testTypeId: 'pdfTest', otherTestTypeName: null },
        ],
      };
      await openPage(withList({ candidates: [one, two] }));

      const first = screen.getByRole('heading', { level: 3, name: 'BewerberPro' }).closest('li') as HTMLElement;
      expect(valueOf(first, 'Auf Barrierefreiheit geprüft').textContent?.trim()).toBe('Ja – 1 Prüfnachweis: Hausinterner Test');
      const second = screen.getByRole('heading', { level: 3, name: 'Zweites Produkt' }).closest('li') as HTMLElement;
      expect(valueOf(second, 'Auf Barrierefreiheit geprüft').textContent?.trim()).toBe(
        'Ja – 2 Prüfnachweise: Eigene Prüfung ohne festes Verfahren, BIT inklusiv PDF-Test',
      );
    });

    it('INV:SIG-13 shows „Abgleich“ as a list of all seven counts in the order of the scale, label first, zeros included', async () => {
      await openPage(withList());

      const pro = screen.getByRole('heading', { level: 3, name: 'BewerberPro' }).closest('li') as HTMLElement;
      const counts = within(valueOf(pro, 'Abgleich')).getAllByRole('listitem').map((item) => item.textContent?.trim());
      expect(counts).toEqual([
        'Erfüllt: 22',
        'Leichte Einschränkung: 3',
        'Einschränkung: 6',
        'Barriere: 2',
        'Blockade: 1',
        'Nicht anwendbar: 2',
        'Nicht eingeschätzt: 10',
      ]);

      const flow = screen.getByRole('heading', { level: 3, name: 'TalentFlow' }).closest('li') as HTMLElement;
      const zeros = within(valueOf(flow, 'Abgleich')).getAllByRole('listitem').map((item) => item.textContent?.trim());
      expect(zeros).toEqual([
        'Erfüllt: 0',
        'Leichte Einschränkung: 0',
        'Einschränkung: 0',
        'Barriere: 0',
        'Blockade: 0',
        'Nicht anwendbar: 0',
        'Nicht eingeschätzt: 46',
      ]);
    });

    it('INV:SIG-13 shows no total, share or „Damals nicht im Katalog“, and keeps the order when the values would sort differently (SIG-08)', async () => {
      const skewed = { ...BEWERBER_PRO, fulfillment: { ...FULFILLMENT, fulfilled: 0, blockade: 40 } };
      await openPage(withList({ candidates: [skewed] }));

      const item = screen.getByRole('heading', { level: 3, name: 'BewerberPro' }).closest('li') as HTMLElement;
      const counts = within(valueOf(item, 'Abgleich')).getAllByRole('listitem').map((entry) => entry.textContent?.trim());
      expect(counts[0]).toBe('Erfüllt: 0');
      expect(counts[4]).toBe('Blockade: 40');
      expect(item.textContent).not.toMatch(/%|Gesamt|Damals nicht im Katalog/);
    });

    it('leaves out progress and counts while the project has no list', async () => {
      await openPage(withoutList({ productCount: 1, candidates: [{ ...BEWERBER_PRO, progress: null, fulfillment: null }] }));

      const item = screen.getByRole('heading', { level: 3, name: 'BewerberPro' }).closest('li') as HTMLElement;
      expect(within(item).queryByText('Einschätzung')).toBeNull();
      expect(within(item).queryByText('Abgleich')).toBeNull();
      expect(valueOf(item, 'Auf Barrierefreiheit geprüft')).toBeTruthy();
    });

    it('says when the project has no products yet', async () => {
      await openPage(withList({ productCount: 0, candidates: [] }));

      expect(screen.getByText(/^Noch keine Produkte in diesem Projekt\./)).toBeTruthy();
      expect(screen.queryByText(/^Reihenfolge:/)).toBeNull();
      expect(screen.queryByRole('heading', { level: 3 })).toBeNull();
    });

    it('offers „Produkt hinzufügen“ as a menu with „Neu erfassen“ to S33; „Aus dem Produktverzeichnis“ comes with M6', async () => {
      await openPage(withList());

      const button = screen.getByRole('button', { name: 'Produkt hinzufügen' });
      expect(button.getAttribute('aria-haspopup')).toBe('menu');
      button.click();
      const menu = await screen.findByRole('menu');
      const items = within(menu).getAllByRole('menuitem');
      expect(items.map((item) => item.textContent?.trim())).toEqual(['Neu erfassen']);
      expect(items[0].getAttribute('href')).toBe('/projekte/p-1/produkte/neu');
    });
  });

  it('opens S30 when the header switches to another organisation (R-SHL-03)', async () => {
    const { fixture } = await openPage(withList());
    const navigate = vi.spyOn(fixture.debugElement.injector.get(Router), 'navigateByUrl').mockResolvedValue(true);

    activeOrganization.set({ id: 'org-2' });
    await fixture.whenStable();

    expect(navigate).toHaveBeenCalledWith('/projekte');
  });

  it('leaves the navigation to the sign-out when no organisation is active any more', async () => {
    const { fixture } = await openPage(withList());
    const navigate = vi.spyOn(fixture.debugElement.injector.get(Router), 'navigateByUrl').mockResolvedValue(true);

    activeOrganization.set(null);
    await fixture.whenStable();

    expect(navigate).not.toHaveBeenCalled();
  });

  it('shows „Seite nicht gefunden“ in place for a project the user cannot see', async () => {
    await openPage({ kind: 'notFound' });

    expect(screen.getByRole('heading', { level: 1, name: 'Seite nicht gefunden' })).toBeTruthy();
    expect(setPageTitleState).not.toHaveBeenCalled();
  });
});
