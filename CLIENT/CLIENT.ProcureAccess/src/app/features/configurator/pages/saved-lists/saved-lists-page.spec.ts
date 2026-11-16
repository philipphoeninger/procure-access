// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { signal } from '@angular/core';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { render, screen, within } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { OrganizationSummary } from '@app/features/organization/models/organization';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { provideLayout } from '@app/testing/shell-test-doubles';
import { SavedListSummary } from '../../models/saved-lists';
import { ConfiguratorApiService } from '../../services/api/configurator-api.service';
import { SavedListsPage, sortSavedLists } from './saved-lists-page';

const A: OrganizationSummary = { id: 'org-a', name: 'Stadt Musterhausen', type: 'publicBody', role: 'reader' };
const B: OrganizationSummary = { id: 'org-b', name: 'Verein Beispiel', type: 'association', role: 'owner' };

const LISTS_A: SavedListSummary[] = [
  { id: 'l-1', name: 'Zeiterfassung', releaseVersion: 'T.1', createdAt: '2026-11-10T09:00:00Z', modifiedAt: '2026-11-15T09:00:00Z' },
  { id: 'l-2', name: 'Ärztliche Software', releaseVersion: 'T.1', createdAt: '2026-11-14T09:00:00Z', modifiedAt: '2026-11-14T09:00:00Z' },
  { id: 'l-3', name: 'Bürgerportal', releaseVersion: 'T.2', createdAt: '2026-11-12T09:00:00Z', modifiedAt: '2026-11-12T09:00:00Z' },
];
const LISTS_B: SavedListSummary[] = [
  { id: 'l-9', name: 'Liste von B', releaseVersion: 'T.1', createdAt: '2026-11-15T09:00:00Z', modifiedAt: '2026-11-15T09:00:00Z' },
];

describe('SavedListsPage (S14, R-SAV-02)', () => {
  let list: ReturnType<typeof vi.fn>;
  let announce: ReturnType<typeof vi.fn>;
  let active: ReturnType<typeof signal<OrganizationSummary | null>>;

  async function openPage({ wide = false, organization = A, api }: { wide?: boolean; organization?: OrganizationSummary | null; api?: ReturnType<typeof vi.fn> } = {}) {
    list = api ?? vi.fn((id: string) => Promise.resolve(id === 'org-b' ? LISTS_B : LISTS_A));
    announce = vi.fn().mockResolvedValue(undefined);
    active = signal(organization);
    const rendered = await render(SavedListsPage, {
      routes: [
        { path: 'kriterien', children: [] },
        { path: 'organisation', children: [] },
        { path: 'kriterienlisten/:id', children: [] },
      ],
      providers: [
        provideTestTranslations(),
        provideLayout(wide),
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        { provide: ConfiguratorApiService, useValue: { list } },
        { provide: A11yAnnouncerService, useValue: { announce } },
        {
          provide: ProcureAccessStore,
          useValue: { activeOrganization: active, incrementLoadingCount: vi.fn(), decrementLoadingCount: vi.fn() },
        },
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

  it('lists the organisation’s lists as cards, the most recently changed first, and announces the number', async () => {
    await openPage();

    expect(list).toHaveBeenCalledWith('org-a');
    expect(screen.getByRole('heading', { level: 1, name: 'Gespeicherte Kriterienlisten' })).toBeTruthy();
    expect(names()).toEqual(['Zeiterfassung', 'Ärztliche Software', 'Bürgerportal']);
    const card = screen.getByRole('heading', { level: 2, name: 'Bürgerportal' }).closest('li')!;
    expect(card.textContent).toContain('Zuletzt geändert');
    expect(card.textContent).toContain('12.11.2026');
    expect(card.textContent).toContain('Katalogversion');
    expect(card.textContent).toContain('T.2');
    expect(screen.getByRole('link', { name: 'Bürgerportal' }).getAttribute('href')).toBe('/kriterienlisten/l-3');
    expect(announce).toHaveBeenCalledWith('common.loaded', { count: 3 });
    await expectNoAxeViolations();
  });

  it('shows a table with name, date and release from 600 px', async () => {
    await openPage({ wide: true });

    const table = screen.getByRole('table', { name: 'Gespeicherte Kriterienlisten' });
    expect(within(table).getAllByRole('columnheader').map((cell) => cell.textContent.trim())).toEqual([
      'Name',
      'Zuletzt geändert',
      'Katalogversion',
    ]);
    expect(within(table).getAllByRole('row')).toHaveLength(4);
    await expectNoAxeViolations();
  });

  it('sorts by name on request and announces the new order (S5-20b 2a)', async () => {
    const fixture = await openPage();

    screen.getByRole('combobox', { name: 'Sortieren nach' }).click();
    await fixture.whenStable();
    screen.getByRole('option', { name: 'Name' }).click();
    await fixture.whenStable();

    expect(names()).toEqual(['Ärztliche Software', 'Bürgerportal', 'Zeiterfassung']);
    expect(announce).toHaveBeenCalledWith('criteria.saved.sort.updated', { sort: 'Name' });
  });

  it('INV:TEN-01 loads the lists of the organisation the header switches to (R-SHL-03, V232)', async () => {
    const fixture = await openPage();

    active.set(B);
    await settle(fixture);

    expect(list).toHaveBeenLastCalledWith('org-b');
    expect(names()).toEqual(['Liste von B']);
    expect(announce).toHaveBeenLastCalledWith('common.loaded.one', { count: 1 });
  });

  it('shows the empty state with „Kriterien zusammenstellen“', async () => {
    await openPage({ api: vi.fn().mockResolvedValue([]) });

    expect(screen.getByText(/Ihre Organisation hat noch keine Kriterienlisten gespeichert/)).toBeTruthy();
    expect(screen.getByRole('link', { name: 'Kriterien zusammenstellen' }).getAttribute('href')).toBe('/kriterien');
    expect(screen.queryByRole('combobox')).toBeNull();
  });

  it('asks for an organisation without a membership and loads nothing', async () => {
    await openPage({ organization: null });

    expect(list).not.toHaveBeenCalled();
    expect(screen.getByRole('link', { name: 'Organisation anlegen oder beitreten' }).getAttribute('href')).toBe('/organisation');
  });

  it('shows the server page in place when the lists cannot be loaded', async () => {
    await openPage({ api: vi.fn().mockRejectedValue(new Error('500')) });

    expect(screen.getByRole('heading', { level: 1, name: 'Etwas ist schiefgelaufen' })).toBeTruthy();
  });
});

describe('sortSavedLists', () => {
  const collator = new Intl.Collator('de');

  it('orders by the last change, newest first, and by name with umlauts in German order', () => {
    expect(sortSavedLists(LISTS_A, 'modified', collator).map((l) => l.id)).toEqual(['l-1', 'l-2', 'l-3']);
    expect(sortSavedLists(LISTS_A, 'name', collator).map((l) => l.name)).toEqual(['Ärztliche Software', 'Bürgerportal', 'Zeiterfassung']);
  });

  it('breaks a tie of one key with the other', () => {
    const same = LISTS_A.map((l) => ({ ...l, modifiedAt: '2026-11-10T09:00:00Z' }));
    expect(sortSavedLists(same, 'modified', collator).map((l) => l.name)).toEqual(['Ärztliche Software', 'Bürgerportal', 'Zeiterfassung']);
  });
});
