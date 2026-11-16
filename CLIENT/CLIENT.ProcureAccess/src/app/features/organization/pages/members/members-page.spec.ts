// SPDX-FileCopyrightText: 2026 Blinden- und Sehbehindertenverein Hamburg e.V.
//
// SPDX-License-Identifier: MIT

import { HttpErrorResponse } from '@angular/common/http';
import { MATERIAL_ANIMATIONS } from '@angular/material/core';
import { ActivatedRoute, convertToParamMap, ParamMap, Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { render, screen, within } from '@testing-library/angular';
import { fireEvent } from '@testing-library/dom';

import { A11yAnnouncerService } from '@app/core/a11y/a11y-announcer.service';
import { PageTitleStrategy } from '@app/core/a11y/page-title.strategy';
import { ProcureAccessStore } from '@app/core/state/app.store';
import { expectNoAxeViolations } from '@app/testing/axe';
import { provideTestTranslations } from '@app/testing/provide-test-translations';
import { provideLayout } from '@app/testing/shell-test-doubles';
import { OrganizationMembers, PendingInvitation } from '../../models/organization';
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

const PENDING: PendingInvitation = { id: 'i-1', email: 'neu@example.test', role: 'reader', expiresAt: '2026-11-29T09:00:00Z' };

describe('MembersPage (S21, R-ORG-02, R-ORG-03, R-ORG-04)', () => {
  let api: {
    members: ReturnType<typeof vi.fn>;
    invitations: ReturnType<typeof vi.fn>;
    changeRole: ReturnType<typeof vi.fn>;
    remove: ReturnType<typeof vi.fn>;
    revokeInvitation: ReturnType<typeof vi.fn>;
    invite: ReturnType<typeof vi.fn>;
  };
  let announce: ReturnType<typeof vi.fn>;
  let setPageTitleState: ReturnType<typeof vi.fn>;
  let loadOrganizations: ReturnType<typeof vi.fn>;

  let load: MembersPageLoad;
  let paramMap: BehaviorSubject<ParamMap>;

  async function openPage(options: { userId?: string; wide?: boolean; view?: MembersPageView; invitations?: PendingInvitation[] } = {}) {
    const userId = options.userId ?? 'u-owner';
    const invitations = userId === 'u-owner' ? (options.invitations ?? []) : null;
    api = {
      members: vi.fn().mockResolvedValue(ORG),
      invitations: vi.fn().mockResolvedValue(invitations ?? []),
      changeRole: vi.fn().mockResolvedValue(undefined),
      remove: vi.fn().mockResolvedValue(undefined),
      revokeInvitation: vi.fn().mockResolvedValue(undefined),
      invite: vi.fn().mockResolvedValue({ id: 'i-new', validDays: 14 }),
    };
    announce = vi.fn().mockResolvedValue(undefined);
    setPageTitleState = vi.fn();
    loadOrganizations = vi.fn().mockResolvedValue(undefined);
    load = new MembersPageLoad();
    paramMap = new BehaviorSubject(convertToParamMap({ id: 'org-1' }));
    load.view = options.view ?? { kind: 'members', data: ORG, invitations };
    return render(MembersPage, {
      routes: [{ path: 'organisation', children: [] }],
      providers: [
        provideTestTranslations(),
        provideLayout(options.wide ?? false),
        { provide: MATERIAL_ANIMATIONS, useValue: { animationsDisabled: true } },
        { provide: MembersPageLoad, useValue: load },
        { provide: ActivatedRoute, useValue: { paramMap, snapshot: { data: {} } } },
        { provide: OrganizationApiService, useValue: api },
        { provide: A11yAnnouncerService, useValue: { announce } },
        { provide: PageTitleStrategy, useValue: { setPageTitleState } },
        { provide: ProcureAccessStore, useValue: { session: () => ({ id: userId, email: 'x@example.test' }), loadOrganizations } },
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

  it('leaves after the confirmation, reads the header\'s organisations again, announces it and opens S20 (R-ORG-04)', async () => {
    const { fixture } = await openPage({ userId: 'u-editor' });
    const navigate = vi.spyOn(fixture.debugElement.injector.get(Router), 'navigate');

    screen.getByRole('button', { name: 'Organisation verlassen' }).click();
    await settle(fixture);
    const dialog = screen.getByRole('dialog');
    expect(within(dialog).getByText(/Sie verlieren den Zugriff auf die Kriterienlisten und Projekte von Stadt Musterhausen/)).toBeTruthy();
    within(dialog).getByRole('button', { name: 'Organisation verlassen' }).click();
    await settle(fixture);

    expect(api.remove).toHaveBeenCalledWith('org-1', 'u-editor');
    expect(loadOrganizations).toHaveBeenCalledOnce();
    expect(announce).toHaveBeenCalledWith('org.left', { org: 'Stadt Musterhausen' });
    expect(navigate).toHaveBeenCalledWith(['organisation']);
  });

  it('shows an Owner the open invitations and the invite form, a Reader neither (R-ORG-02, owner decision S5-17 D4 (a))', async () => {
    await openPage({ invitations: [PENDING] });

    expect(screen.getByRole('heading', { level: 2, name: 'Offene Einladungen' })).toBeTruthy();
    expect(screen.getByText('neu@example.test')).toBeTruthy();
    expect(screen.getByText('Eingeladen, noch nicht angenommen')).toBeTruthy();
    const revoke = screen.getByRole('button', { name: 'Einladung zurücknehmen' });
    expect(revoke.getAttribute('aria-describedby')).toBe('pa-invitation-i-1');
    expect(screen.getByRole('heading', { level: 2, name: 'Mitglied einladen' })).toBeTruthy();
    await expectNoAxeViolations();
  });

  it('shows no open invitations section while there are none', async () => {
    await openPage();

    expect(screen.queryByRole('heading', { level: 2, name: 'Offene Einladungen' })).toBeNull();
    expect(screen.getByRole('heading', { level: 2, name: 'Mitglied einladen' })).toBeTruthy();
  });

  it('shows a Reader neither open invitations nor the invite form', async () => {
    await openPage({ userId: 'u-reader' });

    expect(screen.queryByRole('heading', { name: 'Offene Einladungen' })).toBeNull();
    expect(screen.queryByRole('heading', { name: 'Mitglied einladen' })).toBeNull();
  });

  it('revokes an invitation at once, announces it and reads both lists again (R-ORG-03)', async () => {
    const { fixture } = await openPage({ invitations: [PENDING] });
    api.invitations.mockResolvedValue([]);

    screen.getByRole('button', { name: 'Einladung zurücknehmen' }).click();
    await settle(fixture);

    expect(api.revokeInvitation).toHaveBeenCalledWith('org-1', 'i-1');
    expect(announce).toHaveBeenCalledWith('org.invite.revoked', { email: 'neu@example.test' });
    expect(api.members).toHaveBeenCalledWith('org-1');
    expect(api.invitations).toHaveBeenCalledWith('org-1');
    expect(screen.queryByRole('heading', { level: 2, name: 'Offene Einladungen' })).toBeNull();
  });

  it('reads the open invitations again after an invitation was sent', async () => {
    const { fixture } = await openPage();
    api.invitations.mockResolvedValue([PENDING]);

    fireEvent.input(screen.getByLabelText(/^E-Mail-Adresse/), { target: { value: 'neu@example.test' } });
    screen.getByRole('radio', { name: /^Lesen/ }).click();
    await fixture.whenStable();
    screen.getByRole('button', { name: 'Einladung senden' }).click();
    await settle(fixture);

    expect(api.invite).toHaveBeenCalledWith('org-1', { email: 'neu@example.test', role: 'reader' });
    expect(screen.getByRole('heading', { level: 2, name: 'Offene Einladungen' })).toBeTruthy();
  });

  it('shows the other organisation when the router keeps the page for another id (organisation switch, R-SHL-03)', async () => {
    const { fixture } = await openPage({ wide: true });
    const other: OrganizationMembers = {
      organization: { id: 'org-2', name: 'Verein Beispiel', type: 'association' },
      members: [{ userId: 'u-owner', email: 'owner@example.test', role: 'reader' }],
    };

    load.view = { kind: 'members', data: other, invitations: null };
    paramMap.next(convertToParamMap({ id: 'org-2' }));
    await settle(fixture);

    expect(screen.getByRole('heading', { level: 1, name: 'Mitglieder von Verein Beispiel' })).toBeTruthy();
    expect(screen.queryByRole('combobox')).toBeNull();
    expect(setPageTitleState).toHaveBeenLastCalledWith({ params: { org: 'Verein Beispiel' } });
  });

  it('shows „Seite nicht gefunden“ in place for an organisation the user cannot see (TEN-05)', async () => {
    await openPage({ view: { kind: 'notFound' } });

    expect(screen.getByRole('heading', { level: 1, name: 'Seite nicht gefunden' })).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'Organisation verlassen' })).toBeNull();
  });
});
