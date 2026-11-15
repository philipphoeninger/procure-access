// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { Router } from '@angular/router';
import { render, screen, within } from '@testing-library/angular';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { provideLayout } from '@app/testing/shell-test-doubles';
import { OrganizationMembers } from '../../models/organization';
import { OrganizationApiService } from '../../services/api/organization-api.service';
import { MembersPage } from './members-page';
import { MembersPageLoad, MembersPageView } from './members-page-load';

const base = 'https://procureaccess.de/problems/';

const ORG: OrganizationMembers = {
  organization: { id: 'org-1', name: 'Stadt Musterhausen', type: 'publicBody' },
  members: [
    { userId: 'u-owner', email: 'owner@example.test', role: 'owner' },
    { userId: 'u-editor', email: 'editor@example.test', role: 'editor' },
    { userId: 'u-reader', email: null, role: 'reader' },
  ],
};

describe('MembersPage (S21, R-ORG-03, R-ORG-04)', () => {
  let api: { members: ReturnType<typeof vi.fn>; changeRole: ReturnType<typeof vi.fn>; remove: ReturnType<typeof vi.fn> };
  let announce: ReturnType<typeof vi.fn>;
  let setPageTitleState: ReturnType<typeof vi.fn>;

  async function openPage(options: { userId?: string; wide?: boolean; view?: MembersPageView } = {}) {
    api = {
      members: vi.fn().mockResolvedValue(ORG),
      changeRole: vi.fn().mockResolvedValue(undefined),
      remove: vi.fn().mockResolvedValue(undefined),
    };
    announce = vi.fn().mockResolvedValue(undefined);
    setPageTitleState = vi.fn();
    const load = new MembersPageLoad();
    load.view = options.view ?? { kind: 'members', data: ORG };
    return render(MembersPage, {
      routes: [{ path: 'organisation', children: [] }],
      providers: [
        provideTestTranslations(),
        provideLayout(options.wide ?? false),
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        { provide: MembersPageLoad, useValue: load },
        { provide: OrganizationApiService, useValue: api },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: PageTitleStrategy, useValue: { setPageTitleState } },
        { provide: ProcureAccessStore, useValue: { session: () => ({ id: options.userId ?? 'u-owner', email: 'x@example.test' }) } },
      ],
    });
  }

  async function settle(fixture: { whenStable(): Promise<unknown> }) {
    await fixture.whenStable();
    await new Promise((resolve) => setTimeout(resolve));
    await fixture.whenStable();
  }

  it('titles the page with the organisation and shows an Owner a table with a role select per member (wide)', async () => {
    await openPage({ wide: true });

    expect(screen.getByRole('heading', { level: 1, name: 'Mitglieder von Stadt Musterhausen' })).toBeTruthy();
    expect(setPageTitleState).toHaveBeenCalledWith({ params: { org: 'Stadt Musterhausen' } });
    expect(screen.getAllByRole('columnheader').map((header) => header.textContent?.trim())).toEqual(['Mitglied', 'Rolle', 'Aktionen']);
    expect(screen.getByRole('combobox', { name: 'Rolle von owner@example.test' })).toBeTruthy();
    expect(screen.getByRole('combobox', { name: 'Rolle von Gelöschtes Konto' })).toBeTruthy();
    expect(screen.getByText(/owner@example\.test\s+\(Sie\)/)).toBeTruthy();
    // „entfernen“ for the others only, described by their address; leaving is for everyone
    const remove = screen.getAllByRole('button', { name: 'Aus der Organisation entfernen' });
    expect(remove).toHaveLength(2);
    expect(remove[0].getAttribute('aria-describedby')).toBe('pa-member-u-editor');
    expect(screen.getByRole('button', { name: 'Organisation verlassen' })).toBeTruthy();
    await expectNoAxeViolations();
  });

  it('shows a Reader the roles as text in a list, without selects or „entfernen“ (narrow)', async () => {
    await openPage({ userId: 'u-reader' });

    expect(screen.getAllByRole('listitem')).toHaveLength(3);
    expect(screen.queryByRole('table')).toBeNull();
    expect(screen.queryByRole('combobox')).toBeNull();
    expect(screen.queryByRole('button', { name: 'Aus der Organisation entfernen' })).toBeNull();
    expect(screen.getByText('Verwalten')).toBeTruthy();
    expect(screen.getByText(/Gelöschtes Konto\s+\(Sie\)/)).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Organisation verlassen' })).toBeTruthy();
    await expectNoAxeViolations();
  });

  it('saves a role change at once, announces it and reads the list again', async () => {
    const { fixture } = await openPage();

    screen.getByRole('combobox', { name: 'Rolle von editor@example.test' }).click();
    await settle(fixture);
    screen.getByRole('option', { name: /^Lesen/ }).click();
    await settle(fixture);

    expect(api.changeRole).toHaveBeenCalledWith('org-1', 'u-editor', 'reader');
    expect(announce).toHaveBeenCalledWith('org.members.roleChanged', { email: 'editor@example.test', role: 'Lesen' });
    expect(api.members).toHaveBeenCalledWith('org-1');
  });

  it('shows the last-Owner refusal in an error notice above the list and reads the list again (TEN-11)', async () => {
    const { fixture } = await openPage();
    api.changeRole.mockRejectedValue(new HttpErrorResponse({ status: 409, error: { type: `${base}last-owner` } }));

    screen.getByRole('combobox', { name: 'Rolle von owner@example.test' }).click();
    await settle(fixture);
    screen.getByRole('option', { name: /^Bearbeiten/ }).click();
    await settle(fixture);

    const alert = screen.getByRole('alert');
    expect(within(alert).getByText(/Änderung nicht möglich/)).toBeTruthy();
    expect(within(alert).getByText(/Jede Organisation braucht mindestens ein Mitglied mit der Rolle „Verwalten“/)).toBeTruthy();
    expect(announce).not.toHaveBeenCalled();
    expect(api.members).toHaveBeenCalled();
    await expectNoAxeViolations();
  });

  it('removes a member after the confirmation and announces it', async () => {
    const { fixture } = await openPage();

    screen.getAllByRole('button', { name: 'Aus der Organisation entfernen' })[0].click();
    await settle(fixture);
    const dialog = screen.getByRole('dialog');
    expect(within(dialog).getByRole('heading', { name: 'Mitglied entfernen?' })).toBeTruthy();
    expect(
      within(dialog).getByText('editor@example.test verliert den Zugriff auf die Kriterienlisten und Projekte von Stadt Musterhausen.'),
    ).toBeTruthy();
    within(dialog).getByRole('button', { name: 'Aus der Organisation entfernen' }).click();
    await settle(fixture);

    expect(api.remove).toHaveBeenCalledWith('org-1', 'u-editor');
    expect(announce).toHaveBeenCalledWith('org.members.removed', { email: 'editor@example.test' });
  });

  it('removes nothing when the confirmation is cancelled', async () => {
    const { fixture } = await openPage();

    screen.getAllByRole('button', { name: 'Aus der Organisation entfernen' })[0].click();
    await settle(fixture);
    within(screen.getByRole('dialog')).getByRole('button', { name: 'Abbrechen' }).click();
    await settle(fixture);

    expect(api.remove).not.toHaveBeenCalled();
  });

  it('leaves after the confirmation, announces it and opens S20 (R-ORG-04)', async () => {
    const { fixture } = await openPage({ userId: 'u-editor' });
    const navigate = vi.spyOn(fixture.debugElement.injector.get(Router), 'navigate');

    screen.getByRole('button', { name: 'Organisation verlassen' }).click();
    await settle(fixture);
    const dialog = screen.getByRole('dialog');
    expect(within(dialog).getByText(/Sie verlieren den Zugriff auf die Kriterienlisten und Projekte von Stadt Musterhausen/)).toBeTruthy();
    within(dialog).getByRole('button', { name: 'Organisation verlassen' }).click();
    await settle(fixture);

    expect(api.remove).toHaveBeenCalledWith('org-1', 'u-editor');
    expect(announce).toHaveBeenCalledWith('org.left', { org: 'Stadt Musterhausen' });
    expect(navigate).toHaveBeenCalledWith(['organisation']);
  });

  it('shows „Seite nicht gefunden“ in place for an organisation the user cannot see (TEN-05)', async () => {
    await openPage({ view: { kind: 'notFound' } });

    expect(screen.getByRole('heading', { level: 1, name: 'Seite nicht gefunden' })).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'Organisation verlassen' })).toBeNull();
  });
});
