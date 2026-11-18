// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { render, screen, within } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { OrganizationSummary } from '@app/features/organization/models/organization';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { provideLayout } from '@app/testing/shell-test-doubles';

import { ProjectSummary } from '../../models/projects';
import { ProjectsApiService } from '../../services/api/projects-api.service';
import { testActiveOrganization, WorkspaceTestStore } from '../../testing/workspace-test-store';
import { ProjectsPage, sortProjects } from './projects-page';

const A: OrganizationSummary = { id: 'org-a', name: 'Stadt Musterhausen', type: 'publicBody', role: 'editor' };
const B: OrganizationSummary = { id: 'org-b', name: 'Verein Beispiel', type: 'association', role: 'reader' };

const PROJECTS_A: ProjectSummary[] = [
  { id: 'p-1', name: 'Zeiterfassung', status: 'open', productCount: 3, modifiedAt: '2026-11-15T09:00:00Z' },
  { id: 'p-2', name: 'Ärztliche Software', status: 'done', productCount: 1, modifiedAt: '2026-11-14T09:00:00Z' },
  { id: 'p-3', name: 'Bürgerportal', status: 'archived', productCount: 0, modifiedAt: '2026-11-16T09:00:00Z' },
];
const PROJECTS_B: ProjectSummary[] = [
  { id: 'p-9', name: 'Projekt von B', status: 'open', productCount: 0, modifiedAt: '2026-11-15T09:00:00Z' },
];

describe('ProjectsPage (S30, R-PRJ-01)', () => {
  let list: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;

  async function openPage({
    wide = false,
    organization = A,
    api,
  }: { wide?: boolean; organization?: OrganizationSummary | null; api?: ReturnType<typeof vi.fn> } = {}) {
    list = api ?? vi.fn((id: string) => Promise.resolve(id === 'org-b' ? PROJECTS_B : PROJECTS_A));
    announce = vi.fn().mockResolvedValue(undefined);
    testActiveOrganization.set(organization);
    const rendered = await render(ProjectsPage, {
      routes: [
        { path: 'organisation', children: [] },
        { path: 'projekte/:id', children: [] },
      ],
      providers: [
        provideTestTranslations(),
        provideLayout(wide),
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        { provide: ProjectsApiService, useValue: { list } },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: ProcureAccessStore, useClass: WorkspaceTestStore },
      ],
    });
    await settle(rendered.fixture);
    return rendered.fixture;
  }

  async function settle(fixture: { whenStable(): Promise<unknown> }) {
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  function names(): string[] {
    return screen.getAllByRole('link').map((link) => link.textContent.trim());
  }

  it('lists the organisation’s projects as cards without the archived ones, the most recently changed first, and announces the number', async () => {
    await openPage();

    expect(list).toHaveBeenCalledWith('org-a');
    expect(screen.getByRole('heading', { level: 1, name: 'Beschaffungsprojekte' })).toBeTruthy();
    expect(names()).toEqual(['Zeiterfassung', 'Ärztliche Software']);
    const card = screen.getByRole('heading', { level: 2, name: 'Ärztliche Software' }).closest('li')!;
    expect(card.textContent).toContain('Stand');
    expect(card.textContent).toContain('Abgeschlossen');
    expect(card.textContent).toContain('Produkte');
    expect(card.textContent).toContain('1');
    expect(card.textContent).toContain('Zuletzt geändert');
    expect(card.textContent).toContain('14.11.2026');
    expect(screen.getByRole('link', { name: 'Ärztliche Software' }).getAttribute('href')).toBe('/projekte/p-2');
    expect(announce).toHaveBeenCalledWith('common.loaded', { count: 2 });
    await expectNoAxeViolations();
  });

  it('shows a table with name, status, products and date from 600 px', async () => {
    await openPage({ wide: true });

    const table = screen.getByRole('table', { name: 'Beschaffungsprojekte' });
    expect(within(table).getAllByRole('columnheader').map((cell) => cell.textContent.trim())).toEqual([
      'Name',
      'Stand',
      'Produkte',
      'Zuletzt geändert',
    ]);
    expect(within(table).getAllByRole('row')).toHaveLength(3);
    expect(within(table).getAllByRole('row')[1].textContent).toContain('Laufend');
    await expectNoAxeViolations();
  });

  it('shows the archived projects with „Archivierte Projekte anzeigen“ and announces the number shown (R-PRJ-05)', async () => {
    const fixture = await openPage();

    screen.getByRole('checkbox', { name: 'Archivierte Projekte anzeigen' }).click();
    await fixture.whenStable();

    expect(names()).toEqual(['Bürgerportal', 'Zeiterfassung', 'Ärztliche Software']);
    expect(screen.getByRole('heading', { level: 2, name: 'Bürgerportal' }).closest('li')!.textContent).toContain('Archiviert');
    expect(announce).toHaveBeenCalledWith('projects.shown', { count: 3 });
  });

  it('sorts by name on request and announces the new order (U162)', async () => {
    const fixture = await openPage();

    screen.getByRole('combobox', { name: 'Sortieren nach' }).click();
    await fixture.whenStable();
    screen.getByRole('option', { name: 'Name' }).click();
    await fixture.whenStable();

    expect(names()).toEqual(['Ärztliche Software', 'Zeiterfassung']);
    expect(announce).toHaveBeenCalledWith('projects.sort.updated', { sort: 'Name' });
  });

  it('offers „Projekt anlegen“ to an Editor', async () => {
    await openPage();

    expect(screen.getByRole('button', { name: 'Projekt anlegen' })).toBeTruthy();
  });

  it('hides „Projekt anlegen“ from a Reader (R-ORG-05)', async () => {
    await openPage({ organization: { ...A, role: 'reader' } });

    expect(screen.queryByRole('button', { name: 'Projekt anlegen' })).toBeNull();
    expect(names()).toEqual(['Zeiterfassung', 'Ärztliche Software']);
  });

  it('shows projects.empty without a project, with „Projekt anlegen“ and without sort or filter', async () => {
    await openPage({ api: vi.fn().mockResolvedValue([]) });

    expect(screen.getByText(/^Noch keine Beschaffungsprojekte\./)).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Projekt anlegen' })).toBeTruthy();
    expect(screen.queryByRole('combobox', { name: 'Sortieren nach' })).toBeNull();
    expect(screen.queryByRole('checkbox')).toBeNull();
    expect(announce).toHaveBeenCalledWith('common.loaded.zero', { count: 0 });
    await expectNoAxeViolations();
  });

  it('says that every project is archived when only archived ones exist and the filter is off (U165)', async () => {
    await openPage({ api: vi.fn().mockResolvedValue([PROJECTS_A[2]]) });

    expect(screen.getByText(/^Alle Projekte Ihrer Organisation sind archiviert\./)).toBeTruthy();
    expect(screen.getByRole('checkbox', { name: 'Archivierte Projekte anzeigen' })).toBeTruthy();
    expect(screen.queryByText(/^Noch keine Beschaffungsprojekte/)).toBeNull();
  });

  it('asks for an organisation without a membership and leads to S20 (U166)', async () => {
    await openPage({ organization: null });

    expect(list).not.toHaveBeenCalled();
    expect(screen.getByText(/^Beschaffungsprojekte gehören zu einer Organisation\./)).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Organisation anlegen oder beitreten' }).getAttribute('href')).toBe('/organisation');
  });

  it('shows the S80 server text in place when the list fails', async () => {
    await openPage({ api: vi.fn().mockRejectedValue(new Error('500')) });

    expect(screen.getByRole('heading', { level: 1, name: 'Etwas ist schiefgelaufen' })).toBeTruthy();
    expect(announce).not.toHaveBeenCalled();
  });

  it('INV:TEN-01 loads the projects of the organisation the header switches to (R-SHL-03)', async () => {
    const fixture = await openPage();

    testActiveOrganization.set(B);
    await settle(fixture);

    expect(list).toHaveBeenLastCalledWith('org-b');
    expect(names()).toEqual(['Projekt von B']);
    expect(screen.queryByRole('button', { name: 'Projekt anlegen' })).toBeNull();
    expect(announce).toHaveBeenLastCalledWith('common.loaded.one', { count: 1 });
  });
});

describe('sortProjects', () => {
  const collator = new Intl.Collator('de');

  it('orders by date, newest first, and by name with umlauts in German order; never by a count', () => {
    expect(sortProjects(PROJECTS_A, 'modified', collator).map((p) => p.id)).toEqual(['p-3', 'p-1', 'p-2']);
    expect(sortProjects(PROJECTS_A, 'name', collator).map((p) => p.name)).toEqual(['Ärztliche Software', 'Bürgerportal', 'Zeiterfassung']);
  });

  it('breaks a tie of one order with the other', () => {
    const same = [
      { ...PROJECTS_B[0], id: 'x', name: 'B' },
      { ...PROJECTS_B[0], id: 'y', name: 'A' },
    ];
    expect(sortProjects(same, 'modified', collator).map((p) => p.name)).toEqual(['A', 'B']);
  });
});
